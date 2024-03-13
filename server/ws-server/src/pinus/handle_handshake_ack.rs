use super::pkg::{Pkg, PkgBody, PkgType};

pub async fn handle_handshake_ack(_pkg: Pkg) -> Option<Pkg> {
    log::debug!("handle {}", PkgType::HandshakeAck);

    // handshake ack no content
    Some(Pkg {
        pkg_type: PkgType::Heartbeat,
        content: PkgBody::None,
    })
}
