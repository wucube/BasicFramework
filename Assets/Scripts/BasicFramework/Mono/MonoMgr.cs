using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共Mono管理器
/// </summary>
/// <remarks> 
/// 1.可以提供给外部添加帧更新事件的方法 2.可以提供给外部添加协程的方法
/// </remarks>
public class MonoMgr : SingletonAutoMono<MonoMgr>
{
    private Action updateEvent, fixedUpdateEvent, lateUpdateEvent;

    /// <summary>
    /// 添加Update帧更新事件
    /// </summary>
    public void AddUpdate(Action update)
    {
        updateEvent += update;
    }

    /// <summary>
    /// 移除Update帧更新事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdate(Action update)
    {
        updateEvent -= update;
    }

    /// <summary>
    /// 添加FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void AddFixedUpdate(Action fixedUpdate)
    {
         fixedUpdateEvent += fixedUpdate;
    }

    /// <summary>
    /// 移除FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdate(Action fixedUpdate)
    {
        fixedUpdateEvent -= fixedUpdate;
    }

    /// <summary>
    /// 添加LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public void AddLateUpdate(Action lateUpdate)
    {
        lateUpdateEvent += lateUpdate;
    }

    /// <summary>
    /// 移除LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public void RemoveLateUpdate(Action lateUpdate)
    {
        lateUpdateEvent -= lateUpdate;
    }

    void Update()
    {
        updateEvent?.Invoke();
    }

    void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }

    void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }

    //直接调用StartCoroutine() 启动外部协程
}
