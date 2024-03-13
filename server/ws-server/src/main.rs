use std::{str::FromStr as _, time::Instant};

use actix::*;
use actix_web::{middleware::Logger, web, App, Error, HttpRequest, HttpResponse, HttpServer};
use actix_web_actors::ws;

mod actor_messages;
mod pinus;
mod room;
mod server;
mod session;
mod utils;

use crate::{
    actor_messages::PlayerData,
    utils::config::{get_log_backtrace, get_log_level, get_server_port},
};

/// Entry point for our websocket route
async fn route(
    req: HttpRequest,
    stream: web::Payload,
    srv: web::Data<Addr<server::WsServer>>,
) -> Result<HttpResponse, Error> {
    log::debug!("web route request: {:?}", req);

    let player_data = PlayerData {
        player_id: 0,
        name: "".to_string(),
        other_info: vec![],
        reconnect_secret: 0,
        session_id: 0,
        room_id: 0,
        camp_id: 0,
    };
    ws::start(
        session::WsSession {
            id: 0,
            heartbeat: Instant::now(),
            addr: srv.get_ref().clone(),
            player_data,
        },
        &req,
        stream,
    )
}

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    dotenv::dotenv().ok();

    let log_level = get_log_level();
    let log_backtrace = get_log_backtrace();
    println!("log_level: {}", log_level);
    println!("log_backtrace: {}", log_backtrace);
    let log_level = log::LevelFilter::from_str(&log_level).unwrap_or(log::LevelFilter::Info);
    env_logger::builder()
        .filter_level(log_level)
        .format_timestamp(Some(env_logger::TimestampPrecision::Millis))
        .init();
    std::env::set_var("RUST_BACKTRACE", log_backtrace);

    // config for server
    let server_port = get_server_port();

    // start chat server actor
    let server = server::WsServer::new().start();

    let server = HttpServer::new(move || {
        App::new()
            .app_data(web::Data::new(server.clone()))
            .route("/", web::get().to(route))
            .wrap(Logger::default())
    })
    .bind(("0.0.0.0", server_port))?
    .run();

    log::info!("starting HTTP server at http://localhost:{}", server_port);

    server.await
}
