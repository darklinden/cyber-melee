using System;
using System.Collections.Generic;

namespace App
{
    public static class AtlasGroupExtensions
    {
        private static Dictionary<int, string> s_enumToAddrs = null;

        private static void PrepareEnumAddrs()
        {
            if (s_enumToAddrs == null)
            {
                System.Collections.IList enumValues = Enum.GetValues(typeof(AtlasGroup));

                var addrs = new Dictionary<int, string>(enumValues.Count);

                for (int i = 0; i < enumValues.Count; i++)
                {
                    var atlasGroup = (AtlasGroup)enumValues[i];
                    addrs.Add((int)atlasGroup, $"Assets/Addrs/{i18n.Locale}/Sprites/{atlasGroup}.spriteatlasv2");
                }

                s_enumToAddrs = addrs;
            }
        }

        public static string ToAddr(this AtlasGroup myEnum)
        {
            PrepareEnumAddrs();
            return s_enumToAddrs[(int)myEnum];
        }
    }
}