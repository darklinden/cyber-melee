// --------------------------------------------------------------------------------------------------------------------
// <copyright>
// Copyright © 2012 - 2016 
//
// Email: jim@strategicforge.com
// </copyright> 
// <summary> 
// File: Yielders.cs
// Cached collection of WaitFor YieldInstructions that reduce garbage allocation.
// </summary> 
// -------------------------------------------------------------------------------------------------------------------- 


using System.Collections.Generic;
using UnityEngine;

namespace Wtf
{
    public static class Yielders
    {
        private static WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        public static WaitForEndOfFrame WaitForEndOfFrame()
        {
            return _waitForEndOfFrame;
        }

        private static WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
        public static WaitForFixedUpdate WaitForFixedUpdate()
        {
            return _waitForFixedUpdate;
        }

        const int precision = 1000;
        static int HashCode(float seconds)
        {
            return Mathf.FloorToInt(seconds * precision);
        }

        private static Dictionary<int, WaitForSeconds> _waitForSecsLookup = new Dictionary<int, WaitForSeconds>();
        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            WaitForSeconds wfs;
            var hash = HashCode(seconds);
            if (!_waitForSecsLookup.TryGetValue(hash, out wfs))
            {
                wfs = new WaitForSeconds(seconds);
                _waitForSecsLookup.Add(hash, wfs);
            }
            return wfs;
        }

        private static Dictionary<int, WaitForSecondsRealtime> _WaitForSecondsRealtimeLookup = new Dictionary<int, WaitForSecondsRealtime>();
        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            WaitForSecondsRealtime wfs;
            var hash = HashCode(seconds);
            if (!_WaitForSecondsRealtimeLookup.TryGetValue(hash, out wfs))
            {
                wfs = new WaitForSecondsRealtime(seconds);
                _WaitForSecondsRealtimeLookup.Add(hash, wfs);
            }
            return wfs;
        }
    }
}