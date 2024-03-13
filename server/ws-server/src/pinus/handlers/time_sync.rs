use crate::{
    pinus::msg::{Msg, MsgType},
    utils::time::get_epoch_ms,
};
use anyhow::Result;
use protocols::proto::{RequestTimeSync, ResponseTimeSync, ResponseTimeSyncArgs};

// import the flatbuffers runtime library
extern crate flatbuffers;

pub async fn sync(_msg: Msg) -> Result<Option<Msg>> {
    // in_msg : RequestTimeSync
    // out_msg : ResponseTimeSync

    if _msg.body.is_none() {
        return Err(anyhow::anyhow!("body is none"));
    }

    let buf = _msg.body.unwrap();
    let in_msg: RequestTimeSync<'_> = flatbuffers::root::<RequestTimeSync>(&buf).unwrap();

    let client_time = in_msg.client_time();
    let server_time = get_epoch_ms();

    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(16);

    let result_offset = ResponseTimeSync::create(
        &mut builder,
        &ResponseTimeSyncArgs {
            client_time,
            server_time,
        },
    );

    builder.finish(result_offset, None);

    let buf = builder.finished_data();

    Ok(Some(Msg {
        id: _msg.id,
        msg_type: MsgType::Response,
        route: _msg.route,
        body: Some(buf.to_vec()),
    }))
}
