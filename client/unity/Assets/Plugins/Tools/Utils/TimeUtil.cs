using System;
using System.Collections;
using UnityEngine;

namespace Wtf
{
    public static class TimeUtil
    {
        public enum DeltaTimeType
        {
            DELTA_TIME,
            FIXED_DELTA_TIME,
            UNSCALED_DELTA_TIME
        }

        public static long CurrentTimeInTicks
        {
            get
            {
                return DateTime.Now.Ticks;
            }
        }

        public static long CurrentTimeInMilliseconds
        {
            get
            {
                return TicksToMillis(DateTime.Now.Ticks);
            }
        }

        /// <summary>
        /// Get Milliseconds Since 1970
        /// </summary>
        /// <returns></returns>
        public static long NowMs { get => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }

        /// <summary>
        /// Get Seconds Since 1970
        /// </summary>
        /// <returns></returns>
        public static long Now { get => DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }

        public static float GetDeltaTime(DeltaTimeType dtType)
        {
            switch (dtType)
            {
                case DeltaTimeType.DELTA_TIME:
                    return Time.deltaTime;
                case DeltaTimeType.FIXED_DELTA_TIME:
                    return Time.fixedDeltaTime;
                case DeltaTimeType.UNSCALED_DELTA_TIME:
                    return Time.unscaledDeltaTime;
                default:
                    return Time.maximumDeltaTime;
            }
        }

        public static long TicksToSeconds(long ticks)
        {
            return ticks / 10000000;
        }

        public static long DaysToTicks(int days)
        {
            return days * 864000000000L;
        }

        public static long HoursToTicks(int hours)
        {
            return hours * 36000000000L;
        }

        public static long MinutesToTicks(int minutes)
        {
            return (long)minutes * 600000000L;
        }

        public static long SecondsToTicks(int seconds)
        {
            return (long)seconds * 10000000L;
        }

        public static long TicksToMillis(long ticks)
        {
            return ticks / 10000;
        }

        public static long MillisToTicks(long millis)
        {
            return millis * 10000;
        }

        public static IEnumerator WaitForUnscaledSeconds(float seconds)
        {
            float timer = seconds;
            while (timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public static IEnumerator WaitForFixedSeconds(float seconds)
        {
            float timer = seconds;
            while (timer > 0f)
            {
                timer -= Time.fixedDeltaTime * Time.timeScale;
                yield return Yielders.WaitForFixedUpdate();
            }
        }

        public static string SecondsToStr(float seconds)
        {
            if (seconds <= 0)
                return null;

            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.Days > 0)
                return $"{ts.Days} {ts.Hours:D2}";
            else if (ts.Hours > 0)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}";
            else if (ts.Minutes > 0)
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            else
                return $"{ts.Seconds:D2}s";
        }

        public static DateTime UnixTimestampToDateTime(long unixTimestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(unixTimestamp));
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        /// <summary>
        /// The real time in seconds since the game started (Read Only).
        /// Get Milliseconds Since App Start
        /// </summary>
        /// <returns></returns>
        public static long TickTimeMs { get => (long)Math.Floor(Time.realtimeSinceStartupAsDouble * 1000); }

        /// <summary>
        /// The timeScale-independent time for this frame (Read Only).
        /// Get Milliseconds Since App Start 
        /// </summary>
        public static long TickTimeFrameMs { get => (long)Math.Floor(Time.unscaledTimeAsDouble * 1000); }
    }
}