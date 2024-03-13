use log::{debug, error, info};
use protocols::proto::{RequestTimeSync, RequestTimeSyncArgs, ResponseTimeSync};
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::env;
use tungstenite::{connect, Message};
use url::Url;

mod pinus;
mod utils;

use crate::utils::config::{get_log_backtrace, get_log_level, get_server_url};
use crate::{
    pinus::{
        msg::{Msg, MsgType, Route},
        pkg::{Pkg, PkgBody, PkgType},
    },
    utils::config::ROUTE_TIME_SYNC,
};

use std::time::{SystemTime, UNIX_EPOCH};

fn get_epoch_ms() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap()
        .as_millis() as u64
}

#[derive(Serialize, Deserialize)]
pub struct Sys {
    heartbeat: u16,
    dict: HashMap<String, u16>,
}

#[derive(Serialize, Deserialize)]
pub struct Handshake {
    code: u16,
    sys: Sys,
}

#[derive(Serialize, Deserialize)]
pub struct Pingpong {
    c: i64,
    s: i64,
}

fn main() -> std::io::Result<()> {
    dotenv::dotenv().ok();

    let log_level = get_log_level();
    env::set_var("RUST_LOG", &log_level);
    println!("Log Level: {:?}", &log_level);
    env_logger::init();

    let log_backtrace = get_log_backtrace();
    env::set_var("RUST_BACKTRACE", &log_backtrace);

    let server_url = get_server_url();
    info!("Server URL: {}", &server_url);
    let server_url = Url::parse(&server_url).unwrap();

    let (mut socket, _response) = connect(server_url).expect("Can't connect");

    debug!("Connected to the server");

    // send handshake
    let handshake = r#"{"sys":{"type":"ws","version":"0.0.1","rsa":{}},"user":{}}"#.to_string();
    let pkg = Pkg {
        pkg_type: PkgType::Handshake,
        content: PkgBody::StrMsg(handshake),
    };
    let wsm = pkg.encode().expect("Error encoding handshake pkg");
    socket.send(Message::Binary(wsm)).unwrap();

    let route_str = ROUTE_TIME_SYNC;
    let mut route: u16 = 0;

    let msg = socket.read().expect("Error reading message");
    if msg.is_binary() {
        let pkg = Pkg::decode(&msg.into_data()).expect("Error decoding handshake pkg");
        for p in pkg.iter() {
            match p.content {
                PkgBody::StrMsg(ref s) => {
                    let handshake: Handshake = serde_json::from_str(s).unwrap();
                    handshake.sys.dict.get(route_str).map(|r| route = *r);
                    info!("Handshake get route: {:?}", route);
                }
                _ => {
                    error!("Handshake: unexpected pkg type {}", p.pkg_type);
                }
            }
        }
    } else {
        info!("Handshake: {}", msg);
    }

    // send handshake ack
    let pkg = Pkg {
        pkg_type: PkgType::HandshakeAck,
        content: PkgBody::None,
    };
    let wsm = pkg.encode().expect("Error encoding handshake ack pkg");
    socket.send(Message::Binary(wsm)).unwrap();

    // wait 1 second
    info!("Waiting 1 second");
    std::thread::sleep(std::time::Duration::from_secs(1));

    // send pingpong
    info!("Send TimeSync");
    let client_time = get_epoch_ms();

    let mut builder = flatbuffers::FlatBufferBuilder::with_capacity(16);

    let request_offset =
        RequestTimeSync::create(&mut builder, &RequestTimeSyncArgs { client_time });

    builder.finish(request_offset, None);

    let buf = builder.finished_data();

    let ret_msg = Msg {
        id: 1,
        msg_type: MsgType::Request,
        route: Route {
            code: Some(route),
            name: None,
        },
        body: Some(buf.to_vec()),
    };

    let pkg = Pkg {
        pkg_type: PkgType::Data,
        content: PkgBody::Msg(ret_msg),
    };
    let wsm = pkg.encode().expect("Error encoding pingpong pkg");

    debug!("TimeSync message Sending");
    socket.send(Message::Binary(wsm)).unwrap();
    debug!("TimeSync message Sent");

    'l: loop {
        let msg = socket.read().expect("Error reading TimeSync response");

        if msg.is_binary() {
            let pkg = Pkg::decode(&msg.into_data()).expect("Error decoding TimeSync response pkg");
            for p in pkg.iter() {
                match p.content {
                    PkgBody::Msg(ref msg) => {
                        let buf = msg.body.as_ref().unwrap();
                        let in_msg = flatbuffers::root::<ResponseTimeSync>(&buf).unwrap();

                        let client_time = in_msg.client_time();
                        let server_time = in_msg.server_time();
                        let client_end = get_epoch_ms();
                        info!(
                            "TimeSync: {} cost {} server time: {}",
                            client_time,
                            client_end - client_time,
                            server_time
                        );
                        break 'l;
                    }
                    _ => {
                        info!("TimeSync recv: unexpected pkg type {}", p.pkg_type);
                    }
                }
            }
        } else {
            info!("TimeSync: {}", msg);
        }
    }

    let _ = socket.close(None).expect("Error closing connection");

    Ok(())
}
