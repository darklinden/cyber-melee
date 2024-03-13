use crate::{
    actor_messages::{RoomCompletion, RoomFrameAction, ServerMessage},
    pinus::msg::{Msg, MsgType},
    session::WsSessionData,
};
use anyhow::Result;
use protocols::proto::{ClientPushBattleAction, RequestBattleEnd};

// import the flatbuffers runtime library
extern crate flatbuffers;

pub async fn action_push(_session: &WsSessionData, _msg: Msg) -> Result<Option<Msg>> {
    // in_msg : ClientPushBattleAction
    // out_msg : None

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg = flatbuffers::root::<ClientPushBattleAction>(&buf).unwrap();

    let player_data = &_session.player_data;
    let session_id = player_data.session_id;
    let room_id = player_data.room_id;

    let action_type = in_msg.action_type();
    let action_params = in_msg
        .action_params()
        .unwrap_or_default()
        .into_iter()
        .collect();

    _session
        .server
        .do_send(ServerMessage::RoomPushFrameAction(RoomFrameAction {
            room_id,
            session_id,
            action_type,
            action_params,
        }));

    Ok(None)
}

pub async fn request_end(
    _session: &WsSessionData,
    _msg: Msg,
) -> std::prelude::v1::Result<Option<Msg>, anyhow::Error> {
    // in_msg : RequestBattleEnd
    // out_msg : ResponseBattleEnd

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg = flatbuffers::root::<RequestBattleEnd>(&buf).unwrap();

    let player_data = &_session.player_data;
    let session_id = player_data.session_id;
    let room_id = player_data.room_id;
    let win_camp_rank: Vec<u64> = in_msg.win_camp_rank().unwrap_or_default().iter().collect();

    //  tell server to enter queue
    _session
        .server
        .do_send(ServerMessage::RoomPushCompletion(RoomCompletion {
            room_id,
            from_session_id: session_id,
            win_camp_rank,
        }));

    Ok(Some(Msg {
        id: _msg.id,
        msg_type: MsgType::Response,
        route: _msg.route,
        body: None,
    }))
}
