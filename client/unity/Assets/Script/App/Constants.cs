using System;
using System.Collections.Generic;

namespace App
{
    public class Constants
    {
        internal static string VERSION = "1.0";

        // 默认地图 ID
        internal const int MAP_ID = 0;

        // 默认角色 ID
        internal const int CHARACTER_CONFIG_ID = 1;

        // 服务地址
        // internal const string SERVICE_URL = "ws://local.darklinden.site:3000";
        internal const string SERVICE_URL = "wss://ws-service.darklinden.site:21213";

        // 服务连接检查间隔
        internal const int SERVICE_CONNECTION_CHECK_INTERVAL = 3000;
        // 服务连接超时时间
        internal const long SERVICE_CONNECT_TIMEOUT = SERVICE_CONNECTION_CHECK_INTERVAL * 2;

        // 逻辑帧长度 毫秒
        internal const long FRAME_LENGTH_MS = 50;

        // 逻辑帧长度 秒
        internal const float FRAME_LENGTH_SEC = FRAME_LENGTH_MS / 1000f;

        // 操作相对实现的延迟帧数
        internal const long FRAME_ACTION_DELAY_COUNT = 2;

        // 每次移动的帧数
        internal const long FRAME_COUNT_PER_MOVE = 2;

        // 前端缓存的 帧数
        internal const long FRAME_COUNT_TO_CACHE = 16;

        // 标记量 无效值
        internal const int INVALID_VALUE = -1;

        // PlayerPrefs Key 当前语言
        internal const string PREFER_KEY_LOCALE = "prefer.settings.locale";

        // 精度值 1000 = 1
        internal const int PRECISION = 1000;

        internal const int PROJECTILE_BUFFER_SHOW_SIZE = 7;

        internal const ulong IS_DRAW = 0xff00;
        internal const ulong IS_NOT_DRAW = 0xffff;

        internal static readonly string[] DefaultNames = new string[]
        {
            "Alice",
            "Bob",
            "Cindy",
            "David",
            "Eva",
            "Frank",
            "Grace",
            "Henry",
            "Ivy",
            "Jack",
            "Kelly",
            "Leo",
            "Marry",
            "Nancy",
            "Oscar",
            "Penny",
            "Quincy",
            "Rose",
            "Sam",
            "Tina",
            "Ulysses",
            "Vicky",
            "Walter",
            "Xena",
            "Yoyo",
            "Zack",
        };

    }
}