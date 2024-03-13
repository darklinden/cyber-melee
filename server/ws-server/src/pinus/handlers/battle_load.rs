use crate::{
    actor_messages::{ServerMessage, SessionLoadProgress},
    pinus::msg::Msg,
    session::WsSessionData,
};
use anyhow::Result;
use protocols::proto::ClientPushBattleLoadProgress;

// import the flatbuffers runtime library
extern crate flatbuffers;

pub async fn progress_push(_session: &WsSessionData, _msg: Msg) -> Result<Option<Msg>> {
    // in_msg : ClientPushBattleLoadProgress
    // out_msg : None

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg = flatbuffers::root::<ClientPushBattleLoadProgress>(&buf).unwrap();

    let session_id = _session.player_data.session_id;

    log::debug!(
        "progress_push: session_id: {}, progress: {}",
        session_id,
        in_msg.progress()
    );
    _session
        .server
        .do_send(ServerMessage::RoomBroadcastLoadProgress(
            SessionLoadProgress {
                room_id: _session.player_data.room_id,
                session_id,
                progress: in_msg.progress(),
            },
        ));

    Ok(None)
}
