#if UNITY_EDITOR && DEBUG
#define WRITE_MISSING_LOCALE
#endif

using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Diagnostics;
using App;

// i18n 处理
// Addressables 存放分为两部分
//      一部分是 locale 无关的, 或者使用默认 locale 的, 例如 Assets/Addrs/en-US/
//      一部分是 locale 相关的, 例如 Assets/Addrs/en-US/
//      一般情况下, locale 无关的文件只需要存放一份, locale 相关的文件需要存放多份, 每种语言一份
// 文本不在代码中写死, 存储为 tsv 格式,
// 例如: Assets/Addrs/en-US/en-US/strings.tsv.txt
//  用 tab 分隔, 第一列为 key, 第二列为 value, 第三列为注释
#pragma warning disable IDE1006
public class i18n
{
    // private const string LocaleAddrPrefix = "Assets/Addrs/en-US/Strs/";
    protected static readonly Dictionary<string, string> LocaleAddrs =
        new Dictionary<string, string> {
            { "en-US", "Assets/Addrs/en-US/Strs/strings.tsv.txt" },
        };

    private const string DefaultLocale = "en-US"; // 默认 en-US
    private static string _currentLocale = "en-US";
    public static string Locale => _currentLocale;

#if DEBUG
    private static bool _isLoggingMissing = true;
#else
    private static bool _isLoggingMissing = false;
#endif

    [Conditional("WRITE_MISSING_LOCALE")]
    private static void WriteMissedLocale(string key)
    {
        if (LocaleAddrs.TryGetValue(_currentLocale, out string localeAddr) == false)
        {
            localeAddr = LocaleAddrs[DefaultLocale];
        }

        var stackTrace = new StackTrace();
        var method = stackTrace.GetFrame(2).GetMethod();
        var clazz = method.DeclaringType;
        var func = method.Name;

        string newLine = key + "\t" + key + "\t" + clazz + ":" + func + "\n";
        TranslationData[key] = key;
        System.IO.File.AppendAllText(localeAddr, newLine);
    }

    private static Dictionary<string, string> TranslationData = null;

    static async UniTask InitConfig()
    {
        Log.W("i18n - InitConfig", _currentLocale);
        if (!LocaleAddrs.TryGetValue(_currentLocale, out string localeAddr))
        {
            if (_isLoggingMissing)
            {
                Log.W("i18n Missing locale [" + _currentLocale + "] not found in supported list, using default locale", DefaultLocale);
            }
            _currentLocale = DefaultLocale;
            localeAddr = LocaleAddrs[_currentLocale];
        }

        if (TranslationData != null)
        {
            TranslationData.Clear();
        }
        else
        {
            TranslationData = new Dictionary<string, string>();
        }

        TextAsset textAsset = null;
        try
        {
            textAsset = await Addressables.LoadAssetAsync<TextAsset>(localeAddr);
            if (textAsset != null)
            {
                Log.D("i18n - loaded locale:", localeAddr);
                var tsv = textAsset.text;
                if (string.IsNullOrEmpty(tsv) == false)
                {
                    string[] lines = tsv.Split('\n');
                    foreach (var l in lines)
                    {
                        var line = l.Trim();
                        Log.D("i18n - line:", line);
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }
                        string[] parts = line.Split('\t');
                        if (parts.Length != 2 && parts.Length != 3)
                        {
                            Log.W("i18n - invalid line: " + line);
                            continue;
                        }

                        Log.D("i18n -", parts[0], "=>", parts[1]);

                        TranslationData[parts[0]] = parts[1];
                    }
                }
                else
                {
                    Log.W("i18n - locale file is empty:", localeAddr);
                }
            }
            else
            {
                Log.W("i18n - locale file is empty:", localeAddr);
            }
        }
        catch (Exception e)
        {
            // pass
            Log.E("i18n - failed to load locale:", localeAddr, e);
        }

        if (textAsset != null)
        {
            Addressables.Release(textAsset);
        }
    }

    public static async UniTask Configure(string newLocale = null, bool? logMissing = null)
    {
        Log.D("i18n - Configure");
        if (newLocale != null)
        {
            _currentLocale = newLocale;
        }
        else
        {
            var prefsLocale = PlayerPrefs.GetString(Constants.PREFER_KEY_LOCALE, string.Empty);
            if (string.IsNullOrEmpty(prefsLocale) == false)
            {
                _currentLocale = prefsLocale;
            }
            else
            {
                var systemLocale = Application.systemLanguage;
                switch (systemLocale)
                {
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                    case SystemLanguage.ChineseTraditional:
                        _currentLocale = "en-US";
                        break;
                    default:
                        _currentLocale = "en-US";
                        break;
                }
            }
        }
        if (logMissing.HasValue)
        {
            _isLoggingMissing = logMissing.Value;
        }
        await InitConfig();
    }

    public static string k(string key, params object[] args)
    {
        if (TranslationData == null)
        {
            Log.E("i18n - translationData is null");
        }

        string translation;
        if (TranslationData.TryGetValue(key, out translation) == false)
        {
            if (_isLoggingMissing)
            {
                Log.D("i18n missing:", key);
                WriteMissedLocale(key);
            }
            translation = key;
        }

        if (args.Length > 0)
        {
            translation = string.Format(translation, args);
        }

        return translation;
    }
}