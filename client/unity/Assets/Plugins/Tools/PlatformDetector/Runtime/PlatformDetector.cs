using System.Runtime.InteropServices;

namespace Wtf
{
    public enum DevicePlatform
    {
        Unknown = 0,
        Android = 1,
        IOS = 2,
        Win = 3,
        Mac = 4,
        Linux = 5,
        WebGL_Android = 6,
        WebGL_IOS = 7,
        WebGL_Win = 8,
        WebGL_Mac = 9,
        WebGL_Linux = 10,
    };

    public class PlatformDetector
    {

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern int WebGLPlatform();
#endif

        public static DevicePlatform Platform()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return DevicePlatform.Mac;
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return DevicePlatform.Win;
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            return DevicePlatform.Linux;
#elif UNITY_IOS
            return DevicePlatform.IOS;
#elif UNITY_ANDROID
            return DevicePlatform.Android;
#elif UNITY_WEBGL

#if UNITY_EDITOR_WIN
            return DevicePlatform.WebGL_Win;
#elif UNITY_EDITOR_OSX
            return DevicePlatform.WebGL_Mac;
#elif UNITY_EDITOR_LINUX
            return DevicePlatform.WebGL_Linux;
#else
            return (DevicePlatform)WebGLPlatform();
#endif

#else
            return DevicePlatform.Unknown;
#endif

        }
    }
}