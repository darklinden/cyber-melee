mergeInto(LibraryManager.library, {
    WebGLPlatform: function () {
        var ua = window.navigator.userAgent.toLowerCase();

        // Unknown = 0,
        // Android = 1,
        // IOS = 2,
        // Win = 3,
        // Mac = 4,
        // Linux = 5,
        // WebGL_Android = 6,
        // WebGL_IOS = 7,
        // WebGL_Win = 8,
        // WebGL_Mac = 9,
        // WebGL_Linux = 10,

        var osType = 0;
        if (ua.indexOf("android") != -1) osType = 6;
        else if (ua.indexOf("iphone") != -1) osType = 7;
        else if (ua.indexOf("ipad") != -1) osType = 7;
        else if (ua.indexOf("ipod") != -1) osType = 7;
        else if (ua.indexOf("win") != -1) osType = 8;
        else if (ua.indexOf("mac") != -1) osType = 9;
        else /* if (ua.indexOf("linux") != -1) */ osType = 10;

        return osType;
    },
}); 