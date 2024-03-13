use std::time::Duration;

const SERVER_LOG_LEVEL: &str = "SERVER_LOG_LEVEL";
const SERVER_LOG_BACKTRACE: &str = "SERVER_LOG_BACKTRACE";
const SERVER_PORT: &str = "SERVER_PORT";
const HEARTBEAT_SECOND: &str = "HEARTBEAT_SECOND";
const LOCKSTEP_FRAME_MS: &str = "LOCKSTEP_FRAME_MS";
const LOCKSTEP_FRAME_CHECK_INTERVAL_MS: &str = "LOCKSTEP_FRAME_CHECK_INTERVAL_MS";
const ROOM_PLAYER_COUNT_REQUIRED: &str = "ROOM_PLAYER_COUNT_REQUIRED";
const ROOM_CAMP_COUNT: &str = "ROOM_CAMP_COUNT";
const ROOM_LOADING_TIMEOUT_MS: &str = "ROOM_LOADING_TIMEOUT_MS";
const ROOM_DUR_PER_GAME_MS: &str = "ROOM_DUR_PER_GAME_MS";

fn get_config_str(key: &str) -> String {
    let value = dotenv::var(key);
    match value {
        Ok(value) => value,
        Err(_) => "".to_string(),
    }
}

pub fn get_log_level() -> String {
    let value = get_config_str(SERVER_LOG_LEVEL);
    if value.is_empty() {
        return "info".to_string();
    }
    value
}

pub fn get_log_backtrace() -> String {
    let value = get_config_str(SERVER_LOG_BACKTRACE);
    if value.is_empty() {
        if get_log_level() == "debug" {
            return "1".to_string();
        }
        return "0".to_string();
    }
    value
}

pub fn get_server_port() -> u16 {
    let value = get_config_str(SERVER_PORT);
    value.parse::<u16>().unwrap_or(3000)
}

static mut S_ROOM_PLAYER_COUNT_REQUIRED: Option<usize> = None;
pub fn get_room_player_count_required() -> usize {
    let room_player_count_required = unsafe { S_ROOM_PLAYER_COUNT_REQUIRED };
    if let Some(room_player_count_required) = room_player_count_required {
        return room_player_count_required;
    }
    let value = get_config_str(ROOM_PLAYER_COUNT_REQUIRED);
    let room_player_count_required = value.parse::<usize>().unwrap_or(2);

    unsafe {
        S_ROOM_PLAYER_COUNT_REQUIRED = Some(room_player_count_required);
    }

    room_player_count_required
}

static mut S_ROOM_CAMP_COUNT: Option<usize> = None;
pub fn get_room_camp_count() -> usize {
    let room_camp_count = unsafe { S_ROOM_CAMP_COUNT };
    if let Some(room_camp_count) = room_camp_count {
        return room_camp_count;
    }
    let value = get_config_str(ROOM_CAMP_COUNT);
    let room_camp_count = match value.parse::<usize>() {
        Ok(value) => Some(value),
        Err(_) => Some(2),
    };
    unsafe {
        S_ROOM_CAMP_COUNT = room_camp_count;
    }
    room_camp_count.unwrap()
}

static mut S_HEARTBEAT_SECOND: Option<u16> = None;
pub fn get_heartbeat_second() -> u16 {
    let heartbeat_second = unsafe { S_HEARTBEAT_SECOND };
    if let Some(heartbeat_second) = heartbeat_second {
        return heartbeat_second;
    }
    let value = get_config_str(HEARTBEAT_SECOND);
    let heartbeat_second = match value.parse::<u16>() {
        Ok(value) => Some(value),
        Err(_) => Some(5),
    };
    unsafe {
        S_HEARTBEAT_SECOND = heartbeat_second;
    }
    heartbeat_second.unwrap()
}

static mut S_HEARTBEAT_TIMEOUT: Option<Duration> = None;
pub fn get_heartbeat_timeout() -> Duration {
    let heartbeat_timeout = unsafe { S_HEARTBEAT_TIMEOUT };
    if let Some(heartbeat_timeout) = heartbeat_timeout {
        return heartbeat_timeout;
    }
    let heartbeat_second = get_heartbeat_second() as u64;
    let heartbeat_timeout = Duration::from_secs(heartbeat_second * 2);

    unsafe {
        S_HEARTBEAT_TIMEOUT = Some(heartbeat_timeout);
    }

    heartbeat_timeout
}

static mut S_LOCKSTEP_FRAME_MS: Option<u64> = None;
pub fn get_lockstep_frame_ms() -> u64 {
    let lockstep_frame_ms = unsafe { S_LOCKSTEP_FRAME_MS };
    if let Some(lockstep_frame_ms) = lockstep_frame_ms {
        return lockstep_frame_ms;
    }
    let value = get_config_str(LOCKSTEP_FRAME_MS);
    let lockstep_frame_ms = match value.parse::<u64>() {
        Ok(value) => Some(value),
        Err(_) => Some(100),
    };
    unsafe {
        S_LOCKSTEP_FRAME_MS = lockstep_frame_ms;
    }
    lockstep_frame_ms.unwrap()
}

static mut S_LOCKSTEP_FRAME_CHECK_INTERVAL_MS: Option<u64> = None;
pub fn get_lockstep_frame_check_interval_ms() -> u64 {
    let lockstep_frame_check_interval_ms = unsafe { S_LOCKSTEP_FRAME_CHECK_INTERVAL_MS };
    if let Some(lockstep_frame_check_interval_ms) = lockstep_frame_check_interval_ms {
        return lockstep_frame_check_interval_ms;
    }
    let value = get_config_str(LOCKSTEP_FRAME_CHECK_INTERVAL_MS);
    let lockstep_frame_check_interval_ms = match value.parse::<u64>() {
        Ok(value) => Some(value),
        Err(_) => Some(20),
    };
    unsafe {
        S_LOCKSTEP_FRAME_CHECK_INTERVAL_MS = lockstep_frame_check_interval_ms;
    }
    lockstep_frame_check_interval_ms.unwrap()
}

static mut S_ROOM_LOADING_TIMEOUT_MS: Option<u64> = None;
pub fn get_room_loading_timeout_ms() -> u64 {
    let room_loading_timeout_ms = unsafe { S_ROOM_LOADING_TIMEOUT_MS };
    if let Some(room_loading_timeout_ms) = room_loading_timeout_ms {
        return room_loading_timeout_ms;
    }
    let value = get_config_str(ROOM_LOADING_TIMEOUT_MS);
    let room_loading_timeout_ms = match value.parse::<u64>() {
        Ok(value) => Some(value),
        Err(_) => Some(10000),
    };
    unsafe {
        S_ROOM_LOADING_TIMEOUT_MS = room_loading_timeout_ms;
    }
    room_loading_timeout_ms.unwrap()
}

static mut S_ROOM_DUR_PER_GAME_MS: Option<u64> = None;
pub fn get_room_dur_per_game_ms() -> u64 {
    let room_dur_per_game_ms = unsafe { S_ROOM_DUR_PER_GAME_MS };
    if let Some(room_dur_per_game_ms) = room_dur_per_game_ms {
        return room_dur_per_game_ms;
    }
    let value = get_config_str(ROOM_DUR_PER_GAME_MS);
    let room_dur_per_game_ms = match value.parse::<u64>() {
        Ok(value) => Some(value),
        Err(_) => Some(180000),
    };
    unsafe {
        S_ROOM_DUR_PER_GAME_MS = room_dur_per_game_ms;
    }
    room_dur_per_game_ms.unwrap()
}
