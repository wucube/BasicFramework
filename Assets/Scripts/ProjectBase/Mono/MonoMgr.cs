using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 1.可以提供给外部添加帧更新事件的方法
/// 2.可以提供给外部添加 协程的方法
/// </summary>
public class MonoMgr : BaseManager<MonoMgr>
{
    private MonoController mono;

    public MonoMgr()
    {
        //保证了MonoController对象的唯一性
        GameObject obj = new GameObject("MonoController");
        mono = obj.AddComponent<MonoController>();
    }

    public void AddUpdateListener(Action action)
    {
        mono.AddFixedUpdateListener(action);
    }

    /// <summary>
    /// 移除Update监听
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdateListener(Action action)
    {
        mono.RemoveUpdateListener(action);
    }

    /// <summary>
    /// 添加LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void AddLateUpdateListener(Action action)
    {
        mono.AddLateUpdateListener(action);
    }

    /// <summary>
    /// 移除LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void RemoveLateUpdateListener(Action action)
    {
        mono.RemoveFixedUpdateListener(action);
    }

    /// <summary>
    /// 添加FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void AddFixedUpdateListener(Action action)
    {
        mono.AddFixedUpdateListener(action);
    }

    /// <summary>
    /// 移除FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdateListener(Action action)
    {
        mono.RemoveFixedUpdateListener(action);
    }

    //启用外部协程
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return mono.StartCoroutine(routine);
    }

    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return mono.StartCoroutine(methodName, value);
    }

    public Coroutine StartCoroutine(string methodName)
    {
        return mono.StartCoroutine(methodName);
    }
}
