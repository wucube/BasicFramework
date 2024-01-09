
using System;
using UnityEngine;
using Object = UnityEngine.Object;

/*
*有两种debug，第一种Logging.Log("信息")，第二种Logging.Log(this,"信息")
*第二种点击debug，直接在hierarchy显示是哪个物体发出的log
*/
public static class Logging
{
    private static string _defaultColor = "FFFFFF";
    private static string _successColor = "03FF00";
    private static string _warningColor = "FDFF00";
    private static string _errorColor = "FF1F00";

    private static string Color(this string logSign, string color)
    {
        return $"<color=#{color}>{logSign}</color>";
    }

    private static void DoLog(Action<string> logFunction, string prefix, string color, params object[] msg)
    {
#if UNITY_EDITOR
        prefix = prefix.Color(color);
        var arrow = "----->".Color(color);
        logFunction($"[{prefix}]{arrow} {String.Join("; ", msg).Color(color)}\n ");
#endif
    }

    private static void DoLog(Action<string, Object> logFunction, string prefix, string color, Object logObj, params object[] msg)
    {
#if UNITY_EDITOR
        var name = (logObj ? logObj.name : "NullObject").Color(color);
        prefix = prefix.Color(color);
        var arrow = "----->".Color(color);
        logFunction($"[{prefix}][{name}]{arrow} {String.Join("; ", msg).Color(color)}\n ", logObj);
#endif
    }

    public static void Log(params object[] msg)
    {
        DoLog(Debug.Log, "Default", _defaultColor, msg);
    }

    public static void Log(this Object logObj, params object[] msg)
    {
        DoLog(Debug.Log, "Default", _defaultColor, logObj, msg);
    }

    public static void LogSuccess(params object[] msg)
    {
        DoLog(Debug.Log, "Success", _successColor, msg);
    }

    public static void LogSuccess(this Object logObj, params object[] msg)
    {
        DoLog(Debug.Log, "Success", _successColor, logObj, msg);
    }

    public static void LogWarning(params object[] msg)
    {
        DoLog(Debug.LogWarning, "Warning", _warningColor, msg);
    }

    public static void LogWarning(this Object logObj, params object[] msg)
    {
        DoLog(Debug.LogWarning, "Warning", _warningColor, logObj, msg);
    }

    public static void LogError(params object[] msg)
    {
        DoLog(Debug.LogError, "Error", _errorColor, msg);
    }

    public static void LogError(this Object logObj, params object[] msg)
    {
        DoLog(Debug.LogError, "Error", _errorColor, logObj, msg);
    }
}


