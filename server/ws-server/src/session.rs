use actix::prelude::*;
use actix_web_actors::ws;
use rand::{self};
use std::time::Instant;

use crate::actor_messages::{
    PlayerData, ServerMessage, SessionConnect, SessionDisconnect, SessionMessage,
};
use crate::pinus::handle_pkgs::handle_pkgs;
use crate::pinus::pkg::{Pkg, PkgType};
use crate::server;
use crate::utils::config::{get_heartbeat_second, get_heartbeat_timeout};

// session actor for websocket

#[derive(Debug)]
pub struct WsSession {
    /// unique session id
    pub id: u64,

    /// Client must send ping at least once per 10 seconds (CLIENT_TIMEOUT),
    /// otherwise we drop connection.
    pub heartbeat: Instant,

    /// server
    pub addr: Addr<server::WsServer>,

    pub player_data: PlayerData,
}

pub struct WsSessionData {
    pub player_data: PlayerData,
    pub session: Addr<WsSession>,
    pub server: Addr<server::WsServer>,
}

impl WsSession {
    /// helper method that sends ping to client
    fn heartbeat(&self, ctx: &mut ws::WebsocketContext<Self>) {
        let hb = get_heartbeat_second() as u64;
        let heartbeat_interval: std::time::Duration = std::time::Duration::from_secs(hb);

        ctx.run_interval(heartbeat_interval, |act, ctx| {
            let timeout = get_heartbeat_timeout();
            // check client heartbeats
            if Instant::now().duration_since(act.heartbeat) > timeout {
                // heartbeat timed out
                log::info!("ws session heartbeat timeout: {}", act.id);

                // notify chat server
                act.addr
                    .do_send(ServerMessage::SessionDisconnect(SessionDisconnect {
                        session_id: act.id,
                        player_id: act.player_data.player_id,
                        room_id: act.player_data.room_id,
                    }));

                // stop actor
                ctx.stop();
            }
        });
    }
}

impl Actor for WsSession {
    type Context = ws::WebsocketContext<Self>;

    /// Method is called on actor start.
    /// We register ws session with Server
    fn started(&mut self, ctx: &mut Self::Context) {
        // we'll start heartbeat process on session start.
        self.heartbeat(ctx);

        let mut secret = self.player_data.reconnect_secret;
        while secret == 0 {
            secret = rand::random();
        }
        self.player_data.reconnect_secret = secret;

        // register self in server. `AsyncContext::wait` register
        // future within context, but context waits until this future resolves
        // before processing any other events.
        // HttpContext::state() is instance of WsSessionState, state is shared
        // across all routes within application
        let addr: Addr<WsSession> = ctx.address();
        self.addr
            .send(ServerMessage::SessionConnect(SessionConnect {
                player_data: self.player_data.clone(),
                addr,
            }))
            .into_actor(self)
            .then(|res, act, ctx| {
                match res {
                    Ok(res) => {
                        log::debug!("ws session started: {}", res);
                        act.id = res;
                        act.player_data.session_id = res;
                        act.player_data.player_id = res;
                    }
                    // something is wrong with server
                    _ => {
                        log::error!("ws session start failed: {:#?}", act.player_data);
                        ctx.stop()
                    }
                }
                fut::ready(())
            })
            .wait(ctx);
    }

    fn stopping(&mut self, _: &mut Self::Context) -> Running {
        log::debug!("ws session stopping: {} {:#?}", self.id, self.player_data);

        // notify server
        self.addr
            .do_send(ServerMessage::SessionDisconnect(SessionDisconnect {
                session_id: self.id,
                player_id: self.player_data.player_id,
                room_id: self.player_data.room_id,
            }));

        Running::Stop
    }
}

/// WebSocket message handler
/// for response client requests
impl StreamHandler<Result<ws::Message, ws::ProtocolError>> for WsSession {
    fn handle(&mut self, msg: Result<ws::Message, ws::ProtocolError>, ctx: &mut Self::Context) {
        let msg = match msg {
            Err(e) => {
                log::info!("ws session recv message error: {} {:#?}", self.id, e);
                ctx.stop();
                return;
            }
            Ok(msg) => msg,
        };

        match msg {
            ws::Message::Ping(_) => {}
            ws::Message::Pong(_) => (),
            ws::Message::Text(_) => (),
            ws::Message::Binary(bytes) => {
                let pkgs = Pkg::decode(&bytes).unwrap();

                let addr = ctx.address();
                let session_data = WsSessionData {
                    player_data: self.player_data.clone(),
                    session: addr.clone(),
                    server: self.addr.clone(),
                };

                let future = async move {
                    let reader = handle_pkgs(session_data, pkgs).await;
                    for pkg in reader {
                        addr.do_send(SessionMessage::SessionSendPkg(pkg));
                    }
                };

                future.into_actor(self).spawn(ctx);
            }
            ws::Message::Close(reason) => {
                log::info!(
                    "ws session recv close: {} \n{:#?}",
                    self.id,
                    self.player_data
                );
                ctx.close(reason);
                ctx.stop();
            }
            ws::Message::Continuation(_) => {
                log::info!(
                    "ws session recv continuation: {} \n{:#?}",
                    self.id,
                    self.player_data
                );
            }
            ws::Message::Nop => (),
        }
    }
}

/// Handler for Package message.
/// for notify bytes to client
impl Handler<SessionMessage> for WsSession {
    type Result = u64;

    fn handle(&mut self, msg: SessionMessage, ctx: &mut Self::Context) -> Self::Result {
        match msg {
            SessionMessage::SessionEnter(enter) => {
                self.player_data.name = enter.name;
                self.player_data.other_info = enter.other_info;
                log::debug!("session handle SessionEnter: \n{:#?}", self.player_data);
                0
            }
            SessionMessage::SessionReconnected(reconnect) => {
                if self.id == reconnect.current_session_id {
                    log::debug!("session handle SessionReconnected: \n{:#?}", reconnect);
                    self.player_data.session_id = reconnect.current_session_id;
                    self.player_data.player_id = reconnect.player_id;
                    self.player_data.reconnect_secret = reconnect.reconnect_secret;
                    self.player_data.name = reconnect.name;
                }
                0
            }
            SessionMessage::SessionSendPkg(pkg) => {
                log::debug!("session {} send package \n{:#?}", self.id, pkg);

                match pkg.pkg_type {
                    PkgType::Heartbeat => {
                        // refresh time
                        self.heartbeat = Instant::now();

                        // send message
                        let msg_bytes = pkg.encode();
                        if msg_bytes.is_err() {
                            log::error!("session handle encode pkg fail {}", pkg.pkg_type);
                            return 0;
                        }
                        ctx.binary(msg_bytes.unwrap());
                    }
                    PkgType::Handshake | PkgType::Data => {
                        // send message only
                        let msg_bytes = pkg.encode();
                        if msg_bytes.is_err() {
                            log::error!("session handle encode pkg fail {}", pkg.pkg_type);
                            return 0;
                        }
                        ctx.binary(msg_bytes.unwrap());
                    }
                    PkgType::Kick => {
                        // send message and close
                        let msg_bytes = pkg.encode();
                        if msg_bytes.is_err() {
                            log::error!("session handle encode pkg fail {}", pkg.pkg_type);
                            return 0;
                        }
                        ctx.binary(msg_bytes.unwrap());
                        ctx.close(None);
                        log::info!("session {} handle kick {:#?}, stop", self.id, pkg);
                        ctx.stop();
                    }
                    _ => {
                        log::error!("session handle unknown package type {}", pkg.pkg_type);
                    }
                }

                0
            }
            SessionMessage::SessionRoomInfo(room_info) => {
                if self.id == room_info.session_id {
                    log::debug!(
                        "session {} handle SessionRoomInfo: \n{:#?}",
                        self.id,
                        room_info
                    );
                    self.player_data.room_id = room_info.room_id;
                    self.player_data.camp_id = room_info.camp_id;
                }
                0
            }
            SessionMessage::SessionExitByRoomOver(room_id) => {
                if self.player_data.room_id == room_id {
                    log::info!(
                        "session {} handle SessionExitByRoomOver {}, stop",
                        self.id,
                        room_id
                    );
                    ctx.close(None);
                    ctx.stop();
                }
                0
            }
        }
    }
}
