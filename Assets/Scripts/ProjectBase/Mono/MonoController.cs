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
    private event UnityAction updateEvent;

	void Start () 
    {
        DontDestroyOnLoad(this.gameObject);
	}
	
	void Update () 
    {
        //帧更新事件不为空则每帧运行
        if (updateEvent != null)
            updateEvent();
    }

    /// <summary>
    /// 供外部使用的添加帧更新事件函数
    /// </summary>
    /// <param name="func"></param>
    public void AddUpdateListener(UnityAction func)
    {
        updateEvent += func;
    }

    /// <summary>
    /// 提供给外部 用于移除帧更新事件函数
    /// </summary>
    /// <param name="func"></param>
    public void RemoveUpdateListener(UnityAction func)
    {
        updateEvent -= func;
    }
}
