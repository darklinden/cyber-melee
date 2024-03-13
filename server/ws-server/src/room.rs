use std::collections::HashMap;

use crate::actor_messages::{
    PlayerData, RoomExit, RoomMessage, RoomStarted, ServerMessage, SessionMessage, SessionRoomInfo,
};
use crate::pinus::handlers::{
    route, ROUTE_BATTLE_ACTION_BROADCAST, ROUTE_BATTLE_FINISHED_BROADCAST,
    ROUTE_BATTLE_LOAD_PROGRESS_BROADCAST, ROUTE_BATTLE_SHOULD_FINISH_BROADCAST,
    ROUTE_BATTLE_STARTED_BROADCAST, ROUTE_BATTLE_START_BROADCAST, ROUTE_RECONNECTED_BATTLE_STATE,
};
use crate::pinus::msg::Msg;
use crate::pinus::msg::MsgType;
use crate::pinus::pkg::PkgBody;
use crate::pinus::pkg::PkgType;
use crate::session::WsSession;
use crate::utils::config::get_room_dur_per_game_ms;
use crate::{
    pinus::pkg::Pkg, server, utils::config::get_lockstep_frame_check_interval_ms,
    utils::config::get_lockstep_frame_ms, utils::config::get_room_camp_count,
    utils::config::get_room_loading_timeout_ms, utils::config::get_room_player_count_required,
    utils::time::get_epoch_ms,
};
use actix::prelude::*;
use protocols::proto::{ReconnectedBattleState, ReconnectedBattleStateArgs};
use rand::{self, Rng};
use std::time::Duration;

#[derive(Debug)]
pub struct FrameAction {
    pub session_id: u64,
    pub action_type: i32,
    pub action_params: Vec<i32>,
}

#[derive(Debug)]
pub struct GameRoom {
    /// unique room id
    pub id: u64,

    // < session_id : session >
    session_map: HashMap<u64, Addr<WsSession>>,

    // < player_id : player_data >
    player_data_map: HashMap<u64, PlayerData>,

    // < session_id : player_id>
    session_player_map: HashMap<u64, u64>,

    /// camp list key: camp_id, value: session list
    pub camp_map: HashMap<i32, Vec<u64>>,

    /// server
    pub addr: Addr<server::WsServer>,

    /// frame actions
    pub frame_actions: HashMap<u64, Vec<FrameAction>>,
    pub cumulative_action_list: Vec<FrameAction>,

    pub load_start_time: u64,
    pub game_start_time: u64,
    pub frame_sent_time: u64,
    pub rng: rand::rngs::ThreadRng,

    pub session_load_progresses: HashMap<u64, i32>,
    pub room_completion_result: HashMap<u64, Vec<u64>>,
}

impl GameRoom {
    pub fn new(
        addr: Addr<server::WsServer>,
        players: Vec<(PlayerData, Addr<WsSession>)>,
    ) -> GameRoom {
        let camp_count = get_room_camp_count();
        let player_count_required = get_room_player_count_required();
        log::debug!("room player count required: {}", player_count_required);
        if players.len() != player_count_required {
            panic!("players.len() != player_count_required");
        }

        let mut sessions: HashMap<u64, Addr<WsSession>> = HashMap::new();
        let mut player_data_map = HashMap::new();
        let mut session_player_map = HashMap::new();
        for (player_data, session) in players.iter() {
            sessions.insert(player_data.player_id, session.clone());
            player_data_map.insert(player_data.player_id, player_data.clone());
            session_player_map.insert(player_data.session_id, player_data.player_id);
        }

        log::debug!("room sessions data: {:#?}", sessions);
        let player_ids = player_data_map.keys().cloned().collect::<Vec<u64>>();
        log::debug!("room player ids: {:?}", player_ids);

        let player_count_per_camp = player_count_required / camp_count;
        let mut camps: HashMap<i32, Vec<u64>> = HashMap::new();
        for i in 0..camp_count {
            let camp_id = i as i32 + 1;
            let mut camp_players: Vec<u64> = Vec::new();
            for j in 0..player_count_per_camp {
                let player_id = player_ids[i * player_count_per_camp + j];
                camp_players.push(player_id);
            }
            camps.insert(camp_id, camp_players);
        }

        GameRoom {
            id: 0,
            session_map: sessions,
            player_data_map,
            session_player_map,
            camp_map: camps,
            addr,
            frame_actions: HashMap::new(),
            cumulative_action_list: Vec::new(),
            load_start_time: 0,
            game_start_time: 0,
            frame_sent_time: 0,
            rng: rand::thread_rng(),
            session_load_progresses: HashMap::new(),
            room_completion_result: HashMap::new(),
        }
    }

    fn battle_start(&mut self) {
        let room_id = self.id;

        self.load_start_time = get_epoch_ms();
        log::debug!(
            "room battle start: {} {} \n{:#?}",
            self.id,
            self.load_start_time,
            self.camp_map
        );

        // Push ServerBroadcastBattleStart
        let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(1024);

        // prepare camps
        let mut camps = Vec::new();

        for (camp_id, players) in self.camp_map.iter() {
            let mut camp_players = Vec::new();
            for player_id in players {
                let player_data = self.player_data_map.get(player_id).unwrap();
                let player_name = builder.create_string(&player_data.name);
                let other_info = builder.create_vector(player_data.other_info.as_slice());

                camp_players.push(protocols::proto::PlayerInfo::create(
                    &mut builder,
                    &protocols::proto::PlayerInfoArgs {
                        player_id: *player_id,
                        name: Some(player_name),
                        seed: self.rng.gen::<u64>(),
                        other_info: Some(other_info),
                    },
                ));

                // notify session
                let addr = self.session_map.get(player_id).unwrap();
                addr.do_send(SessionMessage::SessionRoomInfo(SessionRoomInfo {
                    session_id: *player_id,
                    room_id,
                    camp_id: *camp_id,
                }));
            }

            let camp_players = Some(builder.create_vector(&camp_players));
            let camp = protocols::proto::BattleCamp::create(
                &mut builder,
                &protocols::proto::BattleCampArgs {
                    camp_id: *camp_id,
                    players: camp_players,
                },
            );
            camps.push(camp);
        }

        let camps = Some(builder.create_vector(&camps));

        let result_offset = protocols::proto::ServerBroadcastBattleStart::create(
            &mut builder,
            &protocols::proto::ServerBroadcastBattleStartArgs {
                room_id: room_id as i64,
                camps,
            },
        );

        builder.finish(result_offset, None);

        let buf = builder.finished_data();

        let pkg = Pkg {
            pkg_type: PkgType::Data,
            content: PkgBody::Msg(Msg {
                id: 0,
                msg_type: MsgType::Push,
                route: route(ROUTE_BATTLE_START_BROADCAST),
                body: Some(buf.to_vec()),
            }),
        };

        // send message
        log::debug!("room battle start: {}", self.id);
        for (_player_id, addr) in self.session_map.iter() {
            addr.do_send(SessionMessage::SessionSendPkg(pkg.to_owned()));
        }
    }
}

impl GameRoom {
    /// helper method that sends ping to client every 5 seconds (HEARTBEAT_INTERVAL).
    ///
    /// also this method checks heartbeats from client
    fn timer_lockstep_frame(&self, ctx: &mut Context<Self>) {
        let frame_check_interval = get_lockstep_frame_check_interval_ms();
        let frame_ms = get_lockstep_frame_ms();
        log::debug!(
            "room {} frame check interval: {} frame_ms: {}",
            self.id,
            frame_check_interval,
            frame_ms
        );
        let lockstep_frame_check_interval_ms = Duration::from_millis(frame_check_interval);

        ctx.run_interval(lockstep_frame_check_interval_ms, |act, ctx| {
            if act.game_start_time == 0 {
                // game not started, loading
                // detect loading timeout
                let timeout_ms = get_room_loading_timeout_ms();
                let now = get_epoch_ms();
                if now - act.load_start_time > timeout_ms {
                    // loading timeout
                    log::info!("room {} loading timeout, stop", act.id);

                    // notify server
                    let pkg = Pkg {
                        pkg_type: PkgType::Kick,
                        content: PkgBody::StrMsg("loading timeout".to_string()),
                    };
                    for (_session_id, addr) in act.session_map.iter() {
                        addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                    }

                    // stop actor
                    ctx.stop();
                }
            } else {
                let room_dur_per_game_ms: u64 = get_room_dur_per_game_ms();

                if act.frame_sent_time < act.game_start_time + room_dur_per_game_ms {
                    // game started
                    // check frame and send frame actions
                    // ServerBroadcastBattleAction

                    // | -- | -- | -- | -- |
                    // 0    1    2    3    4
                    // begin = 0 end = 1 send 1 begin = 1
                    // begin = 1 end = 2 send 2 begin = 2
                    // ...

                    let frame_ms = get_lockstep_frame_ms();
                    let now = get_epoch_ms();
                    // nearest frame time
                    let now = now - (now % frame_ms);

                    if now < act.game_start_time {
                        return;
                    }

                    if now - act.frame_sent_time >= frame_ms {
                        // log::info!("room {} frame check: {}", act.id, now);
                        let mut time_begin = act.frame_sent_time;
                        if time_begin == 0 {
                            time_begin = act.game_start_time;
                        }

                        let time_end = now;

                        act.frame_sent_time = time_begin;

                        while act.frame_sent_time < time_end {
                            act.frame_sent_time += frame_ms;
                            if act.cumulative_action_list.is_empty() {
                                // no frame actions
                                let mut builder =
                                    flatbuffers::FlatBufferBuilder::with_capacity(1024);

                                let result_offset =
                                    protocols::proto::ServerBroadcastBattleAction::create(
                                        &mut builder,
                                        &protocols::proto::ServerBroadcastBattleActionArgs {
                                            server_time: act.frame_sent_time,
                                            actions: None,
                                        },
                                    );

                                builder.finish(result_offset, None);
                                let buf = builder.finished_data();

                                let pkg = Pkg {
                                    pkg_type: PkgType::Data,
                                    content: PkgBody::Msg(Msg {
                                        id: 0,
                                        msg_type: MsgType::Push,
                                        route: route(ROUTE_BATTLE_ACTION_BROADCAST),
                                        body: Some(buf.to_vec()),
                                    }),
                                };

                                // send message
                                for (_player_id, addr) in act.session_map.iter() {
                                    addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                                }
                            } else {
                                log::debug!(
                                    "room {} frame has actions: {}",
                                    act.id,
                                    act.frame_sent_time
                                );

                                // send frame actions
                                let mut builder =
                                    flatbuffers::FlatBufferBuilder::with_capacity(1024);

                                let mut frame_actions = Vec::new();

                                for action in act.cumulative_action_list.iter() {
                                    let action_params =
                                        Some(builder.create_vector(&action.action_params));
                                    let frame_action = protocols::proto::BattleAction::create(
                                        &mut builder,
                                        &protocols::proto::BattleActionArgs {
                                            player_id: action.session_id,
                                            action_type: action.action_type,
                                            action_params,
                                        },
                                    );
                                    frame_actions.push(frame_action);
                                }

                                act.cumulative_action_list.clear();

                                if !frame_actions.is_empty() {
                                    log::debug!(
                                        "room {} frame has actions: {}",
                                        act.id,
                                        frame_actions.len()
                                    );
                                }

                                let frame_actions = Some(builder.create_vector(&frame_actions));

                                let result_offset =
                                    protocols::proto::ServerBroadcastBattleAction::create(
                                        &mut builder,
                                        &protocols::proto::ServerBroadcastBattleActionArgs {
                                            server_time: act.frame_sent_time,
                                            actions: frame_actions,
                                        },
                                    );

                                builder.finish(result_offset, None);

                                let buf = builder.finished_data();

                                let pkg = Pkg {
                                    pkg_type: PkgType::Data,
                                    content: PkgBody::Msg(Msg {
                                        id: 0,
                                        msg_type: MsgType::Push,
                                        route: route(ROUTE_BATTLE_ACTION_BROADCAST),
                                        body: Some(buf.to_vec()),
                                    }),
                                };

                                // send message
                                for (_player_id, addr) in act.session_map.iter() {
                                    addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                                }
                            }
                        }
                    }
                } else {
                    // game over
                    log::info!("room {} game duration over", act.id);

                    if act.room_completion_result.is_empty() {
                        // broadcast ServerBroadcastBattleShouldFinish
                        let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(1024);

                        let result_offset =
                            protocols::proto::ServerBroadcastBattleShouldFinish::create(
                                &mut builder,
                                &protocols::proto::ServerBroadcastBattleShouldFinishArgs {
                                    server_time: act.game_start_time + room_dur_per_game_ms,
                                },
                            );

                        builder.finish(result_offset, None);
                        let buf = builder.finished_data();

                        let pkg = Pkg {
                            pkg_type: PkgType::Data,
                            content: PkgBody::Msg(Msg {
                                id: 0,
                                msg_type: MsgType::Push,
                                route: route(ROUTE_BATTLE_SHOULD_FINISH_BROADCAST),
                                body: Some(buf.to_vec()),
                            }),
                        };

                        // send message
                        for (_player_id, addr) in act.session_map.iter() {
                            addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                        }
                    }
                }
            }
        });
    }
}

impl Actor for GameRoom {
    type Context = Context<Self>;

    /// Method is called on actor start.
    /// We register ws session with ChatServer
    fn started(&mut self, ctx: &mut Self::Context) {
        log::debug!("room started: {}", self.id);
        // we'll start heartbeat process on session start.
        self.timer_lockstep_frame(ctx);

        let addr = ctx.address();
        self.addr
            .send(ServerMessage::RoomStarted(RoomStarted { addr }))
            .into_actor(self)
            .then(|res, act, ctx| {
                match res {
                    Ok(res) => {
                        log::debug!("room started: {}", res);
                        act.id = res;
                        act.battle_start();
                    }
                    // something is wrong with server
                    _ => {
                        log::error!("room started failed, stop");
                        ctx.stop()
                    }
                }
                fut::ready(())
            })
            .wait(ctx);
    }

    fn stopping(&mut self, _: &mut Self::Context) -> Running {
        log::debug!("room stopping: {}", self.id);

        self.addr
            .do_send(ServerMessage::RoomExit(RoomExit { room_id: self.id }));

        for (_session_id, addr) in self.session_map.iter() {
            addr.do_send(SessionMessage::SessionExitByRoomOver(self.id));
        }

        Running::Stop
    }
}

impl Handler<RoomMessage> for GameRoom {
    type Result = u64;

    fn handle(&mut self, msg: RoomMessage, _ctx: &mut Context<Self>) -> Self::Result {
        match msg {
            RoomMessage::SessionReconnected(player_reconnect) => {
                let player_id = player_reconnect.player_id;
                let session_id = player_reconnect.current_session_id;
                let new_session = player_reconnect.new_addr;

                let player_data = self.player_data_map.get_mut(&player_id);
                if player_data.is_none() {
                    log::error!("player not found: {}", player_id);

                    let pkg = Pkg {
                        pkg_type: PkgType::Kick,
                        content: PkgBody::StrMsg("player not found".to_string()),
                    };

                    new_session.do_send(SessionMessage::SessionSendPkg(pkg));

                    return 1;
                }

                let player_data = player_data.unwrap();
                let old_session_id = player_data.session_id;
                player_data.session_id = session_id;

                self.session_player_map.remove(&old_session_id);
                self.session_player_map.insert(session_id, player_id);
                self.session_map.remove(&session_id);
                self.session_map.insert(session_id, new_session.clone());

                // send room info to new session
                let frame_ms = get_lockstep_frame_ms();

                let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(1024);

                let mut battle_state_frame_actions = Vec::new();

                let mut t = self.game_start_time;
                while t <= self.frame_sent_time {
                    let frame_actions = self.frame_actions.get(&t);
                    t += frame_ms;
                    match frame_actions {
                        None => (),
                        Some(actions) => {
                            let mut frame_actions = Vec::new();

                            // send frame actions
                            for action in actions.iter() {
                                let action_params =
                                    Some(builder.create_vector(&action.action_params));
                                let frame_action = protocols::proto::BattleAction::create(
                                    &mut builder,
                                    &protocols::proto::BattleActionArgs {
                                        player_id: action.session_id,
                                        action_type: action.action_type,
                                        action_params,
                                    },
                                );
                                frame_actions.push(frame_action);
                            }

                            let frame_actions = Some(builder.create_vector(&frame_actions));

                            let passed_actions_offset =
                                protocols::proto::ServerBroadcastBattleAction::create(
                                    &mut builder,
                                    &protocols::proto::ServerBroadcastBattleActionArgs {
                                        server_time: t,
                                        actions: frame_actions,
                                    },
                                );

                            battle_state_frame_actions.push(passed_actions_offset);
                        }
                    }
                }

                let passed_actions = builder.create_vector(&battle_state_frame_actions);
                let result_offset = ReconnectedBattleState::create(
                    &mut builder,
                    &ReconnectedBattleStateArgs {
                        start_server_time: self.game_start_time,
                        current_server_time: self.frame_sent_time,
                        passed_actions: Some(passed_actions),
                    },
                );

                builder.finish(result_offset, None);

                let buf = builder.finished_data();

                let pkg = Pkg {
                    pkg_type: PkgType::Data,
                    content: PkgBody::Msg(Msg {
                        id: 0,
                        msg_type: MsgType::Push,
                        route: route(ROUTE_RECONNECTED_BATTLE_STATE),
                        body: Some(buf.to_vec()),
                    }),
                };

                // send message
                new_session.do_send(SessionMessage::SessionSendPkg(pkg));

                0
            }
            RoomMessage::RoomBroadcastLoadProgress(load_progress) => {
                let op_err_elem = self
                    .session_load_progresses
                    .get_mut(&load_progress.session_id);
                match op_err_elem {
                    None => {
                        self.session_load_progresses
                            .insert(load_progress.session_id, load_progress.progress);
                    }
                    Some(err_elem) => {
                        *err_elem = load_progress.progress;
                    }
                }

                let mut all_loaded = true;
                for (session_id, _) in self.session_map.iter() {
                    let progress = self.session_load_progresses.get(session_id);
                    match progress {
                        None => {
                            all_loaded = false;
                            log::debug!("loading: {} no progress", session_id);
                            break;
                        }
                        Some(progress) => {
                            if *progress < 100 {
                                all_loaded = false;
                                log::debug!("loading: {} progress {}", session_id, progress);
                                break;
                            }
                        }
                    }
                }

                if all_loaded {
                    // all loaded
                    log::debug!("all loaded: {}", self.id);

                    if self.game_start_time != 0 {
                        // already started
                        return 0;
                    }

                    // will start at integer second after 5 seconds
                    let start_time = get_epoch_ms() + 5000;

                    // get nearest second
                    let start_time = start_time - (start_time % 1000);

                    self.game_start_time = start_time;

                    log::debug!("room game start time: {}", start_time);

                    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(16);
                    let result_offset = protocols::proto::ServerBroadcastBattleStarted::create(
                        &mut builder,
                        &protocols::proto::ServerBroadcastBattleStartedArgs {
                            start_server_time: start_time,
                        },
                    );
                    builder.finish(result_offset, None);
                    let buf = builder.finished_data();

                    let pkg = Pkg {
                        pkg_type: PkgType::Data,
                        content: PkgBody::Msg(Msg {
                            id: 0,
                            msg_type: MsgType::Push,
                            route: route(ROUTE_BATTLE_STARTED_BROADCAST),
                            body: Some(buf.to_vec()),
                        }),
                    };

                    log::debug!("room battle started: {} \n{:#?}", self.id, pkg);

                    for (_session_id, addr) in self.session_map.iter() {
                        addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                    }
                } else {
                    // not all loaded
                    log::debug!("not all loaded: {}", self.id);

                    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(16);
                    let result_offset = protocols::proto::ServerBroadcastBattleLoadProgress::create(
                        &mut builder,
                        &protocols::proto::ServerBroadcastBattleLoadProgressArgs {
                            player_id: load_progress.session_id,
                            progress: load_progress.progress,
                        },
                    );
                    builder.finish(result_offset, None);
                    let buf = builder.finished_data();

                    let pkg = Pkg {
                        pkg_type: PkgType::Data,
                        content: PkgBody::Msg(Msg {
                            id: 0,
                            msg_type: MsgType::Push,
                            route: route(ROUTE_BATTLE_LOAD_PROGRESS_BROADCAST),
                            body: Some(buf.to_vec()),
                        }),
                    };

                    for (_session_id, addr) in self.session_map.iter() {
                        addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                    }
                }

                0
            }
            RoomMessage::RoomBroadcastPkg(pkg) => {
                for (_session_id, addr) in self.session_map.iter() {
                    addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                }

                0
            }
            RoomMessage::RoomPushFrameAction(frame_action) => {
                log::debug!("room push frame action: {:?}", frame_action);

                let now = get_epoch_ms();
                if now >= self.game_start_time
                    && now < self.game_start_time + get_room_dur_per_game_ms()
                {
                    // 游戏时间内才接受操作
                    let action = FrameAction {
                        session_id: frame_action.session_id,
                        action_type: frame_action.action_type,
                        action_params: frame_action.action_params,
                    };

                    self.cumulative_action_list.push(action);
                }

                0
            }
            RoomMessage::RoomPushCompletion(completion) => {
                let m_completion = self
                    .room_completion_result
                    .get_mut(&completion.from_session_id);
                match m_completion {
                    None => {
                        self.room_completion_result
                            .insert(completion.from_session_id, completion.win_camp_rank);
                    }
                    Some(comp) => {
                        *comp = completion.win_camp_rank;
                    }
                }

                let mut all_completed = true;
                let mut win_camp_rank_types: Vec<Vec<u64>> = Vec::new();
                // < index : count >
                let mut win_camp_rank_type_count: HashMap<usize, i32> = HashMap::new();
                for (_session_id, _addr) in self.session_map.iter() {
                    let result = self.room_completion_result.get(_session_id);
                    match result {
                        None => {
                            all_completed = false;
                            break;
                        }
                        Some(result) => {
                            if win_camp_rank_types.is_empty() {
                                win_camp_rank_types.push(result.to_owned());
                                win_camp_rank_type_count.insert(win_camp_rank_types.len() - 1, 1);
                            } else {
                                let mut found = false;
                                for (i, rank) in win_camp_rank_types.iter().enumerate() {
                                    if rank.len() != result.len() {
                                        continue;
                                    } else {
                                        let mut all_equal = true;
                                        for (i, v) in result.iter().enumerate() {
                                            if rank[i] != *v {
                                                all_equal = false;
                                                break;
                                            }
                                        }
                                        if all_equal {
                                            let count =
                                                win_camp_rank_type_count.get_mut(&i).unwrap();
                                            *count += 1;
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if !found {
                                    win_camp_rank_types.push(result.to_owned());
                                    win_camp_rank_type_count
                                        .insert(win_camp_rank_types.len() - 1, 1);
                                }
                            }
                        }
                    }
                }

                if !all_completed {
                    // not all completed
                    log::debug!("not all completed: {}", self.id);
                } else {
                    // all completed
                    // if only one type of result, return it
                    // if multiple types of result, return the most frequent one
                    log::debug!("all : {}", self.id);
                    let win_camp_rank: Vec<u64>;
                    if win_camp_rank_types.len() == 1 {
                        win_camp_rank = win_camp_rank_types[0].to_owned();
                    } else {
                        for rnk in win_camp_rank_types.iter() {
                            log::debug!("win_camp_rank_type_count: {:#?}", rnk);
                        }

                        let mut max_count = 0;
                        let mut max_count_index = 0;
                        for (i, count) in win_camp_rank_type_count.iter() {
                            if *count > max_count {
                                max_count = *count;
                                max_count_index = *i;
                            }
                        }
                        win_camp_rank = win_camp_rank_types[max_count_index].to_owned();
                    }

                    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(16);
                    let rank = builder.create_vector(win_camp_rank.as_slice());
                    let result_offset = protocols::proto::ServerBroadcastBattleFinished::create(
                        &mut builder,
                        &protocols::proto::ServerBroadcastBattleFinishedArgs {
                            result_same: win_camp_rank_types.len() == 1,
                            win_camp_rank: Some(rank),
                        },
                    );

                    builder.finish(result_offset, None);
                    let buf = builder.finished_data();

                    let pkg = Pkg {
                        pkg_type: PkgType::Data,
                        content: PkgBody::Msg(Msg {
                            id: 0,
                            msg_type: MsgType::Push,
                            route: route(ROUTE_BATTLE_FINISHED_BROADCAST),
                            body: Some(buf.to_vec()),
                        }),
                    };

                    for (_session_id, addr) in self.session_map.iter() {
                        addr.do_send(SessionMessage::SessionSendPkg(pkg.clone()));
                    }

                    // stop actor
                    log::info!("room {} battle finished, stop", self.id);
                    _ctx.stop();
                }

                0
            }
            RoomMessage::SessionDisconnect(exit) => {
                let session_id = exit.session_id;

                self.session_map.remove(&session_id);
                self.session_player_map.remove(&session_id);

                if self.session_player_map.keys().count() == 0 {
                    // no player left
                    log::info!("room {} no player left, stop", self.id);
                    _ctx.stop();
                }

                0
            }
        }
    }
}
