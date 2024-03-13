use crate::session::WsSessionData;

#[cfg(debug_assertions)]
use super::handlers::code2route;
use super::handlers::handle_data_msg;
use super::pkg::{Pkg, PkgBody, PkgType};

pub async fn handle_data(session: &WsSessionData, pkg: Pkg) -> Option<Pkg> {
    // log::debug!("handle {}", PkgType::Data);
    match pkg.content {
        PkgBody::None => Some(Pkg {
            pkg_type: PkgType::Kick,
            content: PkgBody::StrMsg("handle data fail".to_string()),
        }),
        PkgBody::Msg(msg) => handle_msg(session, msg).await,
        PkgBody::StrMsg(s) => handle_str_msg(s).await,
    }
}

#[cfg(debug_assertions)]
fn add_route_name_for_debug(msg: &mut super::msg::Msg) {
    if msg.route.code.is_some() && msg.route.name.is_none() {
        let route_code = msg.route.code.unwrap();
        let route_name = code2route(route_code);
        if route_name.is_some() {
            let route_name = route_name.unwrap();
            msg.route.name = Some(route_name);
        }
    }
}

async fn handle_msg(session: &WsSessionData, msg: super::msg::Msg) -> Option<Pkg> {
    log::debug!("request \n{:#?}", msg);

    #[cfg(debug_assertions)]
    let mut msg = msg;

    #[cfg(debug_assertions)]
    add_route_name_for_debug(&mut msg);

    let msg = handle_data_msg(session, msg).await;

    log::debug!("response \n{:#?}", msg);

    msg.map(|msg| Pkg {
        pkg_type: PkgType::Data,
        content: PkgBody::Msg(msg),
    })
}

async fn handle_str_msg(msg: String) -> Option<Pkg> {
    log::debug!("handle str {}", msg);

    Some(Pkg {
        pkg_type: PkgType::Kick,
        content: PkgBody::StrMsg("handle data fail".to_string()),
    })
}
