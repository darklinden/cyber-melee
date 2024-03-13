using System;
using UnityEngine;

namespace Wtf
{
    public class TimeLock
    {
        private string _lockTag = string.Empty;
        private long _defaultLockMs = 5000;
        private long _unlockMs = 0;
        private bool _isLocked = false;
        private bool _logLocked = false;

        public TimeLock(string lockTag)
        {
            _lockTag = lockTag;
        }

        public TimeLock(string lockTag, long defaultMs, bool logLocked = true)
        {
            _lockTag = lockTag;
            _defaultLockMs = defaultMs;
            _logLocked = logLocked;
        }

        public bool IsLocked
        {
            get
            {
                if (!_isLocked) return false;

                var nowMs = TimeUtil.NowMs;
                if (nowMs > _unlockMs)
                {
                    _isLocked = false;
                    // Log.D("TimeLock", _lockTag, "Unlocked By Time");
                }
                else
                {
                    if (_logLocked)
                        Log.D("TimeLock", _lockTag, "Still Locked", _unlockMs - nowMs);
                }

                return _isLocked;
            }
        }

        public void Lock(long lockMs = -1)
        {
            _isLocked = true;
            if (lockMs > 0)
            {
                _unlockMs = TimeUtil.NowMs + lockMs;
            }
            else
            {
                _unlockMs = TimeUtil.NowMs + _defaultLockMs;
            }
        }

        public void Unlock()
        {
            // D.Log("TimeLock.Unlock");
            _isLocked = false;
        }
    }
}