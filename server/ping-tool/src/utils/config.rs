const SERVER_URL: &str = "SERVER_URL";
const LOG_LEVEL: &str = "LOG_LEVEL";
const LOG_BACKTRACE: &str = "LOG_BACKTRACE";
pub const ROUTE_TIME_SYNC: &str = "time.sync";

fn get_config_str(key: &str) -> String {
    let value = dotenv::var(key);
    match value {
        Ok(value) => value,
        Err(_) => "".to_string(),
    }
}

pub fn get_server_url() -> String {
    let value = get_config_str(SERVER_URL);
    if value.is_empty() {
        panic!("SERVER_URL is not set");
    }
    value
}

pub fn get_log_level() -> String {
    let value = get_config_str(LOG_LEVEL);
    if value.is_empty() {
        return "info".to_string();
    }
    value
}

pub fn get_log_backtrace() -> String {
    let value = get_config_str(LOG_BACKTRACE);
    if value.is_empty() {
        if get_log_level() == "debug" {
            return "1".to_string();
        }
        return "0".to_string();
    }
    value
}
