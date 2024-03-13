use std::future::IntoFuture;

use crate::{
    actor_messages::{
        ServerMessage, SessionEnter, SessionEnterWaitingQueue, SessionMessage, SessionReconnect,
    },
    pinus::msg::{Msg, MsgType},
    session::WsSessionData,
};
use anyhow::Result;
use protocols::proto::{
    RequestEnter, RequestReconnect, ResponseEnter, ResponseEnterArgs, ResponseReconnect,
    ResponseReconnectArgs,
};

// import the flatbuffers runtime library
extern crate flatbuffers;

pub async fn player_enter(_session: &WsSessionData, _msg: Msg) -> Result<Option<Msg>> {
    // in_msg : ResponseEnter
    // out_msg : ResponseEnterArgs

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg = flatbuffers::root::<RequestEnter>(&buf).unwrap();
    let name = in_msg.name().unwrap_or_default();
    let mut other_info = Vec::new();
    in_msg
        .other_info()
        .unwrap_or_default()
        .iter()
        .for_each(|x| {
            other_info.push(x);
        });

    _session
        .session
        .do_send(SessionMessage::SessionEnter(SessionEnter {
            session_id: _session.player_data.session_id,
            name: name.to_string(),
            other_info: other_info.clone(),
        }));

    //  tell server to enter queue
    _session
        .server
        .do_send(ServerMessage::SessionEnterWaitingQueue(
            SessionEnterWaitingQueue {
                session_id: _session.player_data.session_id,
                player_id: _session.player_data.player_id,
                name: name.to_string(),
                other_info: other_info.clone(),
            },
        ));

    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(32);

    let result_offset = ResponseEnter::create(
        &mut builder,
        &ResponseEnterArgs {
            player_id: _session.player_data.player_id,
            reconnect_secret: _session.player_data.reconnect_secret,
        },
    );

    builder.finish(result_offset, None);

    let buf = builder.finished_data();

    let ret_msg_type = if _msg.msg_type == MsgType::Request {
        MsgType::Response
    } else {
        MsgType::Push
    };

    Ok(Some(Msg {
        id: _msg.id,
        msg_type: ret_msg_type,
        route: _msg.route,
        body: Some(buf.to_vec()),
    }))
}

pub async fn player_reconnect(_session: &WsSessionData, _msg: Msg) -> Result<Option<Msg>> {
    // in_msg : RequestReconnect
    // out_msg : ResponseReconnect

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg = flatbuffers::root::<RequestReconnect>(&buf).unwrap();

    //  tell server to enter queue
    let result = _session
        .server
        .send(ServerMessage::SessionReconnected(SessionReconnect {
            current_session_id: _session.player_data.session_id,
            player_id: in_msg.player_id(),
            reconnect_secret: in_msg.reconnect_secret(),
            name: "".to_string(),
        }))
        .into_future()
        .await?;

    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(32);

    let result_offset = ResponseReconnect::create(
        &mut builder,
        &ResponseReconnectArgs {
            game_state: result as i32,
        },
    );

    builder.finish(result_offset, None);

    let buf = builder.finished_data();

    let ret_msg_type = if _msg.msg_type == MsgType::Request {
        MsgType::Response
    } else {
        MsgType::Push
    };

    Ok(Some(Msg {
        id: _msg.id,
        msg_type: ret_msg_type,
        route: _msg.route,
        body: Some(buf.to_vec()),
    }))
}
