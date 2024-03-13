#if UNITY_EDITOR || (DEBUG && !UNITY_WEBGL)
#define LOG_STACK_LINE 
#endif

using System.IO;
using System;
using System.Text;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public static partial class Log
{
    private static Queue<StringBuilder> m_StringBuilderQueue = new Queue<StringBuilder>();

    private static StringBuilder GetSb()
    {
        if (m_StringBuilderQueue.Count > 0)
        {
            return m_StringBuilderQueue.Dequeue();
        }

        return new StringBuilder();
    }

    private static void ReturnSb(StringBuilder sb)
    {
        sb.Clear();
        m_StringBuilderQueue.Enqueue(sb);
    }

    private static string NowTimeStr()
    {
        return DateTime.Now.ToString("[HH:mm:ss.fff]");
    }

    private static void InsertDepth(StringBuilder sb, int depth)
    {
        while (depth-- > 0)
        {
            sb.Insert(0, "  ");
        }
    }

    private static void AppendDepth(StringBuilder sb, int depth)
    {
        while (depth-- > 0)
        {
            sb.Append("  ");
        }
    }

    private static string BytesToStr(byte[] bytes, int depth = 0)
    {
        StringBuilder buff = GetSb();
        buff.Append("[");
        for (var i = 0; i < bytes.Length; i++)
        {
            if (buff.Length > 1)
            {
                buff.Append(" ");
            }

            buff.AppendFormat("{0:x2}", bytes[i]);
        }

        buff.Append("]>");

        buff.Insert(0, " ");
        buff.Insert(0, bytes.Length);
        buff.Insert(0, "<bytes len: ");
        InsertDepth(buff, depth);

        var ret = buff.ToString();
        ReturnSb(buff);
        return ret;
    }

    private static string ObjectToStr(object obj, int depth = 0)
    {
        if (obj == null)
        {
            return "null";
        }

        if (obj.GetType().IsValueType)
        {
            return obj.ToString();
        }

        if (obj.GetType().Name == "Byte[]")
        {
            return BytesToStr((byte[])obj, depth);
        }

        if (obj is IList)
        {
            StringBuilder buff = GetSb();
            buff.Append("[");
            foreach (var item in (IList)obj)
            {
                if (buff.Length > 1)
                {
                    buff.Append(",");
                    buff.Append("\n");
                }

                buff.Append(ObjectToStr(item));
            }

            buff.Append("]");
            var ret = buff.ToString();
            ReturnSb(buff);
            return ret;
        }

        if (obj is IDictionary)
        {
            StringBuilder buff = GetSb();
            buff.Append("{");
            foreach (DictionaryEntry item in (IDictionary)obj)
            {
                if (buff.Length > 1)
                {
                    buff.Append(",");
                    buff.Append("\n");
                }

                buff.Append(ObjectToStr(item.Key));
                buff.Append(":");
                buff.Append(ObjectToStr(item.Value));
            }

            buff.Append("}");
            var ret = buff.ToString();
            ReturnSb(buff);
            return ret;
        }

        if (obj is UnityEngine.Object)
        {
            return obj.ToString();
        }

        if (obj is Exception)
        {
            return obj.ToString();
        }

        if (obj is string)
        {
            return obj as string;
        }

        return LitJson.JsonMapper.ToJson(obj);
    }

    private static string ObjectsToString(string flag, object message1 = null, object message2 = null, object message3 = null,
            object message4 = null, object message5 = null, object message6 = null, object message7 = null,
            object message8 = null, object message9 = null, object message10 = null, object message11 = null,
            object message12 = null, object message13 = null, object message14 = null, object message15 = null)
    {
        StringBuilder result = new StringBuilder();
        result.Append(NowTimeStr());
        result.Append(flag);

        if (message1 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message1));
        }
        if (message2 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message2));
        }
        if (message3 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message3));
        }
        if (message4 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message4));
        }
        if (message5 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message5));
        }
        if (message6 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message6));
        }
        if (message7 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message7));
        }
        if (message8 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message8));
        }
        if (message9 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message9));
        }
        if (message10 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message10));
        }
        if (message11 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message11));
        }
        if (message12 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message12));
        }
        if (message13 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message13));
        }
        if (message14 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message14));
        }
        if (message15 != null)
        {
            result.Append("  ");
            result.Append(ObjectToStr(message15));
        }
        return result.ToString();
    }

    [Conditional("ENABLE_LOG")]
    public static void D(object message0, object message1 = null, object message2 = null, object message3 = null,
        object message4 = null, object message5 = null, object message6 = null, object message7 = null,
        object message8 = null, object message9 = null, object message10 = null, object message11 = null,
        object message12 = null, object message13 = null, object message14 = null)
    {
        Profiler.BeginSample("Log.D");

#if LOG_STACK_LINE
        var frame = new System.Diagnostics.StackTrace(true).GetFrame(1);
        string currentFile = frame.GetFileName();
        if (!string.IsNullOrEmpty(currentFile))
            currentFile = currentFile.Substring(currentFile.LastIndexOf("Assets/"));
        int currentLine = frame.GetFileLineNumber();
        var prefix = $"[D {currentFile}:{currentLine}]:";
#else
        var prefix = "[D]:";
#endif

        var result = ObjectsToString(prefix, message0, message1, message2, message3,
                 message4, message5, message6, message7,
                 message8, message9, message10, message11,
                 message12, message13, message14);

#if UNITY_SERVER
        Console.WriteLine(result);
#else
        UnityEngine.Debug.Log(result);
#endif
        Profiler.EndSample();
    }

    [Conditional("ENABLE_LOG")]
    public static void W(object message0, object message1 = null, object message2 = null,
        object message3 = null,
        object message4 = null, object message5 = null, object message6 = null, object message7 = null,
        object message8 = null, object message9 = null, object message10 = null, object message11 = null,
        object message12 = null, object message13 = null, object message14 = null)
    {
        Profiler.BeginSample("Log.W");

#if LOG_STACK_LINE
        var frame = new System.Diagnostics.StackTrace(true).GetFrame(1);
        string currentFile = frame.GetFileName();
        if (!string.IsNullOrEmpty(currentFile))
            currentFile = currentFile.Substring(currentFile.LastIndexOf("Assets/"));
        int currentLine = frame.GetFileLineNumber();
        var prefix = $"[W {currentFile}:{currentLine}]:";
#else
        var prefix = "[W]:";
#endif

        var result = ObjectsToString(prefix, message0, message1, message2, message3,
                 message4, message5, message6, message7,
                 message8, message9, message10, message11,
                 message12, message13, message14);

#if UNITY_SERVER
        Console.WriteLine(result);
#else
        UnityEngine.Debug.LogWarning(result);
#endif
        Profiler.EndSample();
    }

    private static Action<string> s_ErrToast = null;
    public static void SetErrToast(Action<string> errToast)
    {
        s_ErrToast = errToast;
    }

    // 即使关闭日志也会输出错误
    public static void E(object message0, object message1 = null, object message2 = null, object message3 = null,
        object message4 = null, object message5 = null, object message6 = null, object message7 = null,
        object message8 = null, object message9 = null, object message10 = null, object message11 = null,
        object message12 = null, object message13 = null, object message14 = null)
    {
        Profiler.BeginSample("Log.E");

#if LOG_STACK_LINE
        var frame = new System.Diagnostics.StackTrace(true).GetFrame(1);
        string currentFile = frame.GetFileName();
        if (!string.IsNullOrEmpty(currentFile))
            currentFile = currentFile.Substring(currentFile.LastIndexOf("Assets/"));
        int currentLine = frame.GetFileLineNumber();
        var prefix = $"[E {currentFile}:{currentLine}]:";
#else
        var prefix = "[E]:";
#endif

        var result = ObjectsToString(prefix, message0, message1, message2, message3,
                 message4, message5, message6, message7,
                 message8, message9, message10, message11,
                 message12, message13, message14);

#if UNITY_SERVER
        Console.WriteLine(result);
#else
        UnityEngine.Debug.LogError(result);
#endif

        Profiler.EndSample();

        s_ErrToast?.Invoke(result);
    }


    // 即使关闭日志也会输出错误
    public static void AssertIsTrue(bool value, object message0 = null, object message1 = null, object message2 = null, object message3 = null,
            object message4 = null, object message5 = null, object message6 = null, object message7 = null,
            object message8 = null, object message9 = null, object message10 = null, object message11 = null,
            object message12 = null, object message13 = null, object message14 = null)
    {
        if (!value)
        {

#if LOG_STACK_LINE
            var frame = new System.Diagnostics.StackTrace(true).GetFrame(1);
            string currentFile = frame.GetFileName();
            if (!string.IsNullOrEmpty(currentFile))
                currentFile = currentFile.Substring(currentFile.LastIndexOf("Assets/"));
            int currentLine = frame.GetFileLineNumber();
            var methodBase = frame.GetMethod();
            var prefix = $"[A {currentFile}:{currentLine} {methodBase}]:";
#else
            var prefix = "[A]:";
#endif

            string result = string.Empty;

            if (message0 == null)
            {
                result = ObjectsToString(prefix, "IsTrue failed");
            }
            else
            {
                result = ObjectsToString(prefix, message0, message1, message2, message3,
                 message4, message5, message6, message7,
                 message8, message9, message10, message11,
                 message12, message13, message14);
            }

#if UNITY_SERVER
            Console.WriteLine(result);
#else
            UnityEngine.Debug.LogError(result);
#endif
        }
    }
}