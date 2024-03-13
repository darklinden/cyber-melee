use crate::session::WsSessionData;

use super::{super::pinus::msg::Msg, msg::Route};
use anyhow::Result;
use const_for::const_for;
use const_str::equal;

mod battle_load;
mod battle_run;
mod enter;
mod time_sync;

pub const ROUTE_NO_ROUTE: &str = "no-route";
pub const ROUTE_TIME_SYNC: &str = "time.sync";
pub const ROUTE_PLAYER_ENTER: &str = "player.enter";
pub const ROUTE_BATTLE_START_BROADCAST: &str = "battle.start.broadcast";
pub const ROUTE_BATTLE_LOAD_PROGRESS_PUSH: &str = "battle.load.progress.push";
pub const ROUTE_BATTLE_LOAD_PROGRESS_BROADCAST: &str = "battle.load.progress.broadcast";
pub const ROUTE_BATTLE_STARTED_BROADCAST: &str = "battle.started.broadcast";
pub const ROUTE_BATTLE_ACTION_PUSH: &str = "battle.action.push";
pub const ROUTE_BATTLE_ACTION_BROADCAST: &str = "battle.action.broadcast";
pub const ROUTE_BATTLE_END: &str = "battle.end";
pub const ROUTE_BATTLE_SHOULD_FINISH_BROADCAST: &str = "battle.should.finish.broadcast";
pub const ROUTE_BATTLE_FINISHED_BROADCAST: &str = "battle.finished.broadcast";
pub const ROUTE_RECONNECT: &str = "battle.reconnect";
pub const ROUTE_RECONNECTED_BATTLE_STATE: &str = "battle.reconnected.state";

#[allow(dead_code)]
pub const ROUTE_LIST: &[&str] = &[
    ROUTE_NO_ROUTE,                       // 0
    ROUTE_TIME_SYNC,                      // RequestTimeSync ResponseTimeSync
    ROUTE_PLAYER_ENTER,                   // RequestEnter ResponseEnter
    ROUTE_BATTLE_START_BROADCAST,         // ServerBroadcastBattleStart
    ROUTE_BATTLE_LOAD_PROGRESS_PUSH,      // ClientPushBattleLoadProgress
    ROUTE_BATTLE_LOAD_PROGRESS_BROADCAST, // ServerBroadcastBattleLoadProgress
    ROUTE_BATTLE_STARTED_BROADCAST,       // ServerBroadcastBattleStarted
    ROUTE_BATTLE_ACTION_PUSH,             // ClientPushBattleAction
    ROUTE_BATTLE_ACTION_BROADCAST,        // ServerBroadcastBattleAction
    ROUTE_BATTLE_END,                     // RequestBattleEnd ResponseBattleEnd
    ROUTE_BATTLE_SHOULD_FINISH_BROADCAST, // ServerBroadcastBattleShouldFinish
    ROUTE_BATTLE_FINISHED_BROADCAST,      // ServerBroadcastBattleFinished
    ROUTE_RECONNECT,                      // RequestReconnect ResponseReconnect
    ROUTE_RECONNECTED_BATTLE_STATE,       // ReconnectedBattleState
];

// a constant function
const fn const_search(arr: &[&str], key: &&str) -> u16 {
    let size = arr.len();
    let mut index = 0;
    const_for!(i in 0..size => {
        if equal!(arr[i], *key) {
            index = i;
            break;
        }
    });
    index as u16
}

const ROUTE_TIME_SYNC_CODE: u16 = const_search(ROUTE_LIST, &ROUTE_TIME_SYNC);
const ROUTE_PLAYER_ENTER_CODE: u16 = const_search(ROUTE_LIST, &ROUTE_PLAYER_ENTER);
const ROUTE_BATTLE_LOAD_PROGRESS_PUSH_CODE: u16 =
    const_search(ROUTE_LIST, &ROUTE_BATTLE_LOAD_PROGRESS_PUSH);
const ROUTE_BATTLE_ACTION_PUSH_CODE: u16 = const_search(ROUTE_LIST, &ROUTE_BATTLE_ACTION_PUSH);
const ROUTE_BATTLE_END_CODE: u16 = const_search(ROUTE_LIST, &ROUTE_BATTLE_END);
const ROUTE_RECONNECT_CODE: u16 = const_search(ROUTE_LIST, &ROUTE_RECONNECT);

#[allow(dead_code)]
pub fn code2route(route_code: u16) -> Option<String> {
    let route_list = ROUTE_LIST;
    let index = route_code as usize;
    if index > 0 && index < route_list.len() {
        return Some(route_list[index].to_string());
    }
    None
}

#[allow(dead_code)]
pub fn route2code(route: &str) -> Option<u16> {
    let route_list = ROUTE_LIST;
    for (index, item) in route_list.iter().enumerate() {
        if item == &route {
            return Some(index as u16);
        }
    }
    None
}

#[allow(dead_code)]
pub fn route(route: &str) -> Route {
    let code = route2code(route);
    Route {
        code,
        name: Some(route.to_string()),
    }
}

#[allow(dead_code)]
pub async fn handle_data_msg(_session: &WsSessionData, msg: Msg) -> Option<Msg> {
    let route_code = msg.route.to_owned().code.unwrap_or(0);
    let result = match route_code {
        ROUTE_TIME_SYNC_CODE => time_sync::sync(msg).await, // RequestTimeSync ResponseTimeSync
        ROUTE_PLAYER_ENTER_CODE => enter::player_enter(_session, msg).await, // RequestEnterMatchQueue ResponseEnterMatchQueue
        ROUTE_BATTLE_LOAD_PROGRESS_PUSH_CODE => battle_load::progress_push(_session, msg).await, // ClientPushBattleLoadProgress
        ROUTE_BATTLE_ACTION_PUSH_CODE => battle_run::action_push(_session, msg).await, // ClientPushBattleAction
        ROUTE_BATTLE_END_CODE => battle_run::request_end(_session, msg).await, // RequestBattleEnd ResponseBattleEnd
        ROUTE_RECONNECT_CODE => enter::player_reconnect(_session, msg).await, // RequestReconnect ResponseReconnect
        _ => Result::Err(anyhow::anyhow!("route not found")),
    };

    match result {
        Ok(msg) => msg,
        Err(e) => {
            log::error!("handle_data_msg error: {:?}", e);
            None
        }
    }
}
