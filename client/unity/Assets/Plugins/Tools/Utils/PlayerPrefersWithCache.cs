using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public static class PlayerPrefersWithCache
    {
        private static readonly Dictionary<string, string> PrefersData = new Dictionary<string, string>();

        /// <summary>
        /// Load data load by PlayerPrefs, set to buttons level on map scene 
        /// </summary>
        /// <returns></returns>
        public static string GetString(string key)
        {
            string value = null;
            if (PrefersData.TryGetValue(key, out value))
            {
                return value;
            }

            value = PlayerPrefs.GetString(key, "");
            PrefersData[key] = value;
            return value;
        }

        public static void SetString(string key, string value, bool save = true)
        {
            PrefersData[key] = value;
            PlayerPrefs.SetString(key, value);
            if (save)
            {
                PlayerPrefs.Save();
            }
        }

        public static int GetInt(string key)
        {
            var value = GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return int.Parse(value);
        }

        public static void SetInt(string key, int value, bool save = true)
        {
            SetString(key, value.ToString(), save);
        }

        public static long GetLong(string key)
        {
            var value = GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return long.Parse(value);
        }

        public static void SetLong(string key, long value, bool save = true)
        {
            SetString(key, value.ToString(), save);
        }

        public static float GetFloat(string key)
        {
            var value = GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return float.Parse(value);
        }

        public static void SetFloat(string key, float value, bool save = true)
        {
            SetString(key, value.ToString(), save);
        }

        private static void Test()
        {
            // [15:53:58.113][L]:  Dict add:  570970
            // [15:53:58.142][L]:  Dict get:  278570
            // [15:53:58.329][L]:  PlayerPrefs set:  1872040
            // [15:55:17.536][L]:  PlayerPrefs get:  1460080

            var ds = System.DateTime.Now.Ticks;
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < 100000; i++)
            {
                dict.Add(i.ToString(), i.ToString());
            }
            var de = System.DateTime.Now.Ticks;
            Log.D("Dict add:", de - ds);

            ds = System.DateTime.Now.Ticks;
            for (int i = 0; i < 100000; i++)
            {
                dict.TryGetValue(i.ToString(), out var v);
            }
            de = System.DateTime.Now.Ticks;
            Log.D("Dict get:", de - ds);

            ds = System.DateTime.Now.Ticks;
            for (int i = 0; i < 100000; i++)
            {
                PlayerPrefs.SetString(i.ToString(), i.ToString());
            }
            de = System.DateTime.Now.Ticks;
            Log.D("PlayerPrefs set:", de - ds);

            PlayerPrefs.Save();

            ds = System.DateTime.Now.Ticks;
            for (int i = 0; i < 100000; i++)
            {
                PlayerPrefs.GetString(i.ToString(), "");
            }
            de = System.DateTime.Now.Ticks;
            Log.D("PlayerPrefs get:", de - ds);

            PlayerPrefs.DeleteAll();
        }
    }
}
