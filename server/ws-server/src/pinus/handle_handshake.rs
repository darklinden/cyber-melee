use std::collections::HashMap;

use serde::{Deserialize, Serialize};

use crate::utils::config::get_heartbeat_second;

use super::handlers::ROUTE_LIST;
use super::pkg::{Pkg, PkgBody, PkgType};

pub async fn handle_handshake(pkg: Pkg) -> Option<Pkg> {
    // log::debug!("handle {}", PkgType::Handshake);
    match pkg.content {
        PkgBody::None => Some(Pkg {
            pkg_type: PkgType::Kick,
            content: PkgBody::StrMsg("handshake fail".to_string()),
        }),
        PkgBody::Msg(msg) => handle_msg(msg).await,
        PkgBody::StrMsg(s) => handle_str_msg(s).await,
    }
}

async fn handle_msg(_msg: super::msg::Msg) -> Option<Pkg> {
    // log::debug!("handle msg");

    Some(Pkg {
        pkg_type: PkgType::Kick,
        content: PkgBody::StrMsg("handshake fail".to_string()),
    })
}

#[derive(Serialize, Deserialize, Debug)]
pub struct Sys {
    heartbeat: u16,
    dict: HashMap<String, u16>,
}

#[derive(Serialize, Deserialize, Debug)]
pub struct Handshake {
    code: u16,
    sys: Sys,
}

#[allow(clippy::needless_range_loop)]
async fn handle_str_msg(_msg: String) -> Option<Pkg> {
    log::debug!("handle str {}", _msg);

    let mut dict = HashMap::new();
    for i in 1..ROUTE_LIST.len() {
        dict.insert(ROUTE_LIST[i].to_string(), i as u16);
    }

    let sys = Sys {
        heartbeat: get_heartbeat_second(),
        dict,
    };

    let handshake = Handshake { code: 200, sys };
    log::debug!("return handshake: \n{:#?}", handshake);

    let handshake = serde_json::to_string(&handshake).unwrap();

    Some(Pkg {
        pkg_type: PkgType::Handshake,
        content: PkgBody::StrMsg(handshake),
    })
}
