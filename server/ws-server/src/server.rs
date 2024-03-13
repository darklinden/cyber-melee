use std::collections::{HashMap, VecDeque};
use std::ops::Index;

use actix::prelude::*;
use rand::{self, rngs::ThreadRng, Rng};

use crate::actor_messages::{
    PlayerData, RoomMessage, RoomSessionReconnect, ServerMessage, SessionMessage, SessionReconnect,
};
use crate::pinus::pkg::{Pkg, PkgBody, PkgType};
use crate::room::GameRoom;
use crate::session::WsSession;
use crate::utils::config::get_room_player_count_required;

#[derive(Debug)]
pub struct WsServer {
    #[allow(dead_code)]
    server_id: u64,

    // < session_id : session >
    session_map: HashMap<u64, Addr<WsSession>>,

    // < player_id : player_data >
    player_data_map: HashMap<u64, PlayerData>,

    // < session_id : player_id>
    session_player_map: HashMap<u64, u64>,

    // < player_id >
    waiting_queue: VecDeque<u64>,

    // 存储 room 列表 key: game_type_id:player_required_count, value: room list
    room_map: HashMap<u64, Addr<GameRoom>>,

    // 随机数生成器
    rng: ThreadRng,
}

static mut SERVER_ID_SEQ: u64 = 0;

impl WsServer {
    pub fn new() -> WsServer {
        let server_id = unsafe {
            SERVER_ID_SEQ += 1;
            SERVER_ID_SEQ
        };
        log::info!("new server id: {}", server_id);
        WsServer {
            server_id,
            session_map: HashMap::new(),
            player_data_map: HashMap::new(),
            session_player_map: HashMap::new(),
            waiting_queue: VecDeque::new(),
            room_map: HashMap::new(),
            rng: rand::thread_rng(),
        }
    }
}

impl WsServer {
    /// Send message to user by id
    #[allow(dead_code)]
    fn send_message_by_id(&self, session_id: u64, pkg: &Pkg) {
        if let Some(addr) = self.session_map.get(&session_id) {
            addr.do_send(SessionMessage::SessionSendPkg(pkg.to_owned()));
        }
    }
}

/// Make actor from `WsServer`
impl Actor for WsServer {
    /// We are going to use simple Context, we just need ability to communicate
    /// with other actors.
    type Context = Context<Self>;
}

/// Handler for Connect message.
///
/// Register new session and assign unique id to this session
impl Handler<ServerMessage> for WsServer {
    type Result = u64;

    fn handle(&mut self, msg: ServerMessage, _ctx: &mut Context<Self>) -> Self::Result {
        // register session with random id
        match msg {
            ServerMessage::SessionConnect(session_connect) => {
                let mut player_data = PlayerData {
                    player_id: 0,
                    name: "".to_string(),
                    other_info: vec![],
                    reconnect_secret: 0,
                    session_id: 0,
                    room_id: 0,
                    camp_id: 0,
                };
                player_data.reconnect_secret = session_connect.player_data.reconnect_secret;

                let mut session_id: u64 = self.rng.gen::<u64>();
                while self.session_map.contains_key(&session_id) || session_id == 0 {
                    session_id = self.rng.gen::<u64>();
                }
                self.session_map.insert(session_id, session_connect.addr);

                player_data.session_id = session_id;

                // default player data
                player_data.player_id = session_id;
                self.player_data_map.insert(session_id, player_data.clone());

                // session to player map
                self.session_player_map
                    .insert(session_id, player_data.player_id);

                log::info!("current session count: {}", self.session_map.len());

                // send id back
                session_id
            }
            ServerMessage::SessionReconnected(session_reconnect) => {
                // result
                // 0: in match queue
                // 1: in room
                // other: error

                let player_id = session_reconnect.player_id;
                let session_id = session_reconnect.current_session_id;
                let reconnect_secret = session_reconnect.reconnect_secret;

                let pre_player_data = self.player_data_map.get_mut(&player_id);
                if pre_player_data.is_none() {
                    // kick self
                    let session = self.session_map.get(&session_id);
                    if let Some(session) = session {
                        session.do_send(SessionMessage::SessionSendPkg(Pkg {
                            pkg_type: PkgType::Kick,
                            content: PkgBody::StrMsg("Reconnect Fail".to_string()),
                        }));
                    }
                    return 2;
                }

                let pre_player_data = pre_player_data.unwrap();

                if pre_player_data.reconnect_secret != reconnect_secret {
                    // kick self
                    let session = self.session_map.get(&session_id);
                    if let Some(session) = session {
                        session.do_send(SessionMessage::SessionSendPkg(Pkg {
                            pkg_type: PkgType::Kick,
                            content: PkgBody::StrMsg("Reconnect Fail".to_string()),
                        }));
                    }
                    return 2;
                }

                if (pre_player_data.player_id == player_id)
                    && (pre_player_data.session_id == session_id)
                    && (pre_player_data.reconnect_secret == reconnect_secret)
                {
                    // not need to reconnect
                    return 3;
                }

                let old_session_id = pre_player_data.session_id;
                // kick old if exist

                let old_session = self.session_map.get(&old_session_id);
                if let Some(old_session) = old_session {
                    old_session.do_send(SessionMessage::SessionSendPkg(Pkg {
                        pkg_type: PkgType::Kick,
                        content: PkgBody::StrMsg("Reconnect Kick Old".to_string()),
                    }));
                }

                // update session id
                pre_player_data.session_id = session_id;
                self.session_player_map.insert(session_id, player_id);

                // update session
                let session = self.session_map.get(&session_id).unwrap();

                // tell session player data
                session.do_send(SessionMessage::SessionReconnected(SessionReconnect {
                    player_id,
                    current_session_id: session_id,
                    reconnect_secret,
                    name: pre_player_data.name.clone(),
                }));

                let mut result = 0;

                // tell room player data
                if pre_player_data.room_id != 0 {
                    let room = self.room_map.get(&pre_player_data.room_id);
                    if room.is_some() {
                        result = 1;
                        room.unwrap().do_send(RoomMessage::SessionReconnected(
                            RoomSessionReconnect {
                                player_id,
                                current_session_id: session_id,
                                reconnect_secret,
                                name: pre_player_data.name.clone(),
                                new_addr: session.clone(),
                            },
                        ));
                    }
                }

                result
            }
            ServerMessage::SessionDisconnect(session_disconnect) => {
                // let mut channels_leave_msg: Vec<String> = Vec::new();

                // remove address
                if self
                    .session_map
                    .remove(&session_disconnect.session_id)
                    .is_some()
                {
                    // remove session from all channels and deliver `Leave` to other users
                    log::info!("current session count: {}", self.session_map.len());
                }

                if let Some(room) = self.room_map.get(&session_disconnect.room_id) {
                    // tell room
                    room.do_send(RoomMessage::SessionDisconnect(session_disconnect));
                }

                0
            }
            ServerMessage::SessionEnterWaitingQueue(session_enter_waiting_queue) => {
                if let Some(player_data) = self
                    .player_data_map
                    .get_mut(&session_enter_waiting_queue.player_id)
                {
                    player_data.name = session_enter_waiting_queue.name;
                    player_data.other_info = session_enter_waiting_queue.other_info;
                }

                self.waiting_queue
                    .push_back(session_enter_waiting_queue.player_id);

                let room_player_count_required = get_room_player_count_required();

                let mut player_count = 0;

                // check if match player count required
                let mut i = 0;
                while i < self.waiting_queue.len() {
                    let player_id = self.waiting_queue.index(i);
                    let player_data = self.player_data_map.get(player_id);
                    if player_data.is_none() {
                        self.waiting_queue.remove(i);
                        continue;
                    }
                    let player_data = player_data.unwrap();
                    if self.session_map.contains_key(&player_data.session_id) {
                        // player online
                        player_count += 1;
                        i += 1;
                    } else {
                        self.waiting_queue.remove(i);
                    }

                    if player_count >= room_player_count_required {
                        break;
                    }
                }

                // check if match player count required
                if player_count >= room_player_count_required {
                    // create room
                    let mut players: Vec<(PlayerData, Addr<WsSession>)> = Vec::new();
                    for _ in 0..room_player_count_required {
                        let player_id = self.waiting_queue.pop_front().unwrap();
                        let player_data = self.player_data_map.get_mut(&player_id);
                        if player_data.is_none() {
                            continue;
                        }
                        let player_data = player_data.unwrap();
                        let session = self.session_map.get(&player_data.session_id).unwrap();
                        players.push((player_data.clone(), session.clone()));
                    }

                    // start room
                    let _room = GameRoom::new(_ctx.address().clone(), players).start();
                }

                0
            }
            ServerMessage::RoomStarted(room_started) => {
                // register session with random id
                let mut room_id: u64 = self.rng.gen::<u64>();
                while self.room_map.contains_key(&room_id) || room_id == 0 {
                    room_id = self.rng.gen::<u64>();
                }
                self.room_map.insert(room_id, room_started.addr);

                log::info!("current rooms count: {}", self.room_map.len());

                // send id back
                room_id
            }
            ServerMessage::RoomBroadcastLoadProgress(msg) => {
                log::debug!("RoomBroadcastLoadProgress: {:?}", msg);
                if let Some(addr) = self.room_map.get(&msg.room_id) {
                    addr.do_send(RoomMessage::RoomBroadcastLoadProgress(msg));
                }
                0
            }
            ServerMessage::RoomBroadcastPkg(msg) => {
                if let Some(addr) = self.room_map.get(&msg.room_id) {
                    addr.do_send(RoomMessage::RoomBroadcastPkg(msg.pkg));
                }
                0
            }
            ServerMessage::RoomPushFrameAction(msg) => {
                if let Some(addr) = self.room_map.get(&msg.room_id) {
                    addr.do_send(RoomMessage::RoomPushFrameAction(msg));
                }
                0
            }
            ServerMessage::RoomPushCompletion(msg) => {
                if let Some(addr) = self.room_map.get(&msg.room_id) {
                    addr.do_send(RoomMessage::RoomPushCompletion(msg));
                }
                0
            }
            ServerMessage::RoomExit(msg) => {
                self.room_map.remove(&msg.room_id);

                log::debug!("current rooms count: {}", self.room_map.len());
                0
            }
        }
    }
}
