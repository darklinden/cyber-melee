use crate::{pinus::pkg::Pkg, room::GameRoom, session::WsSession};
use actix::prelude::*;

#[derive(Message, Debug)]
#[rtype(u64)]
pub enum ServerMessage {
    /// New session is created
    SessionConnect(SessionConnect),

    /// Session is disconnected
    SessionDisconnect(SessionDisconnect),

    /// Session enter queue
    SessionEnterWaitingQueue(SessionEnterWaitingQueue),

    /// Session reconnect
    SessionReconnected(SessionReconnect),

    /// Session in queue is ready to create room
    RoomStarted(RoomStarted),

    RoomExit(RoomExit),

    RoomBroadcastLoadProgress(SessionLoadProgress),

    /// Push pkg to sessions
    #[allow(dead_code)]
    RoomBroadcastPkg(RoomBroadcastPkg),

    RoomPushFrameAction(RoomFrameAction),

    RoomPushCompletion(RoomCompletion),
}

#[allow(clippy::enum_variant_names)]
#[derive(Message, Debug)]
#[rtype(u64)]
pub enum SessionMessage {
    /// Session bind room info
    SessionEnter(SessionEnter),

    SessionReconnected(SessionReconnect),

    /// Session bind room info
    SessionRoomInfo(SessionRoomInfo),

    /// Send pkg to session
    SessionSendPkg(Pkg),

    /// Session exit when room is over
    SessionExitByRoomOver(u64),
}

#[derive(Message, Debug)]
#[rtype(u64)]
pub enum RoomMessage {
    RoomBroadcastLoadProgress(SessionLoadProgress),

    /// Send pkg to room
    RoomBroadcastPkg(Pkg),

    RoomPushFrameAction(RoomFrameAction),

    RoomPushCompletion(RoomCompletion),

    SessionDisconnect(SessionDisconnect),

    SessionReconnected(RoomSessionReconnect),
}

/// New session is created
#[derive(Debug)]
pub struct SessionConnect {
    pub player_data: PlayerData,
    pub addr: Addr<WsSession>,
}

/// New session is created
#[derive(Debug)]
pub struct SessionReconnect {
    pub current_session_id: u64,
    pub player_id: u64,
    pub reconnect_secret: u64,
    pub name: String,
}

/// New session is created
#[derive(Debug)]
pub struct RoomSessionReconnect {
    pub current_session_id: u64,
    pub player_id: u64,
    pub reconnect_secret: u64,
    pub name: String,
    pub new_addr: Addr<WsSession>,
}

/// Session is disconnected
#[derive(Debug)]
pub struct SessionDisconnect {
    pub session_id: u64,
    pub player_id: u64,
    pub room_id: u64,
}

/// Session enter queue
#[derive(Debug)]
pub struct SessionEnterWaitingQueue {
    pub session_id: u64,
    pub player_id: u64,
    pub name: String,
    pub other_info: Vec<i32>,
}

/// Session in queue is ready to create room
#[derive(Debug)]
pub struct RoomStarted {
    // room id
    pub addr: Addr<GameRoom>,
}

/// Session in queue is ready to create room
#[derive(Debug)]
pub struct RoomExit {
    // room id
    pub room_id: u64,
}

#[derive(Debug, Clone)]
pub struct SessionEnter {
    pub session_id: u64,
    pub name: String,
    pub other_info: Vec<i32>,
}

#[derive(Debug, Clone)]
pub struct SessionRoomInfo {
    pub session_id: u64,
    pub room_id: u64,
    pub camp_id: i32,
}

#[derive(Debug, Clone)]
pub struct SessionLoadProgress {
    pub room_id: u64,
    pub session_id: u64,
    pub progress: i32,
}

/// New session is created
#[derive(Debug)]
pub struct RoomFrameAction {
    pub room_id: u64,
    pub session_id: u64,
    pub action_type: i32,
    pub action_params: Vec<i32>,
}

/// Push pkg to sessions
#[derive(Debug)]
pub struct RoomBroadcastPkg {
    pub room_id: u64,
    pub pkg: Pkg,
}

/// Push pkg to sessions
#[derive(Debug)]
pub struct RoomCompletion {
    pub room_id: u64,
    pub from_session_id: u64,
    pub win_camp_rank: Vec<u64>,
}

/// Push pkg to sessions
#[derive(Debug, Clone)]
pub struct PlayerData {
    pub player_id: u64,
    pub name: String,
    pub other_info: Vec<i32>,
    pub reconnect_secret: u64,
    pub session_id: u64,
    pub room_id: u64,
    pub camp_id: i32,
}
