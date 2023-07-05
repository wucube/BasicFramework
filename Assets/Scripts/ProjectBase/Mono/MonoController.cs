using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mono的管理者
/// 1.声明周期函数
/// 2.事件 
/// 3.协程
/// </summary>
public class MonoController : MonoBehaviour 
{
    /// <summary>
    /// 帧更新事件
    /// </summary>
    private Action updateEvent;
    private Action lateUpdateEvent;
    private Action fixedUpdateEvent;

	void Start () 
    {
        DontDestroyOnLoad(this.gameObject);
	}

    #region 生命周期函数
    /// <summary>
    /// 添加Update监听
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateListener(Action action)
    {
        updateEvent += action;
    }

    /// <summary>
    /// 移除Update监听
    /// </summary>
    /// <param name="action"></param>
    public  void RemoveUpdateListener(Action action)
    {
        updateEvent -= action;
    }

    /// <summary>
    /// 添加LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void AddLateUpdateListener(Action action)
    {
        lateUpdateEvent += action;
    }

    /// <summary>
    /// 移除LateUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void RemoveLateUpdateListener(Action action)
    {
        lateUpdateEvent -= action;
    }

    /// <summary>
    /// 添加FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public  void AddFixedUpdateListener(Action action)
    {
        fixedUpdateEvent += action;
    }

    /// <summary>
    /// 移除FixedUpdate监听
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdateListener(Action action)
    {
        fixedUpdateEvent -= action;
    }

    private void Update()
    {
        updateEvent?.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateEvent?.Invoke();
    }
    private void FixedUpdate()
    {
        fixedUpdateEvent?.Invoke();
    }

    #endregion


}
