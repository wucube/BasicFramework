
/*
 * 注销事件类封装事件类中的 注销事件 函数（命令）
 * 注销事件类实例再被封装到Mono类中，借由Mono的销毁生命周期函数来注销事件
 * 这一封装过程本质是构造Mono管理器，将事件的生命末期（注销）与Mono的生命末期同步，也利用了命令模式
 * 进一步抽象：借由命令模式的封装，可同步两个具有不同生命周期的对象
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 注销事件接口
/// </summary>
public interface IUnRegister
{
    /// <summary>
    /// 注销事件
    /// </summary>
    void UnRegister();
}
/// <summary>
/// 注销事件列表接口
/// </summary>
public interface IUnRegisterList
{
    /// <summary>
    /// 注销事件列表
    /// </summary>
    /// <value></value>
    List<IUnRegister> UnRegisterList { get; }
}
/// <summary>
/// 注销事件接口列的扩展
/// </summary>
public static class IUnRegisterListExtension
{
    /// <summary>
    /// 将注销事件添加到注销事件列表
    /// </summary>
    /// <param name="self"></param>
    /// <param name="unRegisterList"></param>
    public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList)
    {
        unRegisterList.UnRegisterList.Add(self);
    }

    /// <summary>
    /// 注销所有事件
    /// </summary>
    /// <param name="self"></param>
    public static void UnRegisterAll(this IUnRegisterList self)
    {
        foreach (var unRegister in self.UnRegisterList)
        {
            unRegister.UnRegister();
        }

        self.UnRegisterList.Clear();
    }
}

/// <summary>
/// 自定义可注销事件的类
/// </summary>
public struct CustomUnRegister : IUnRegister
{
    /// <summary>
    /// 接受 注册事件的函数
    /// </summary>
    private Action mOnUnRegister { get; set; }

    /// <summary>
    /// 带参构造函数
    /// </summary>
    /// <param name="onDispose"></param>
    public CustomUnRegister(Action onUnRegsiter)
    {
        mOnUnRegister = onUnRegsiter;
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void UnRegister()
    {
        mOnUnRegister.Invoke();
        mOnUnRegister = null;
    }
}


/// <summary>
/// GameObject销毁时自动注销注册的消息或事件
/// </summary>
public class UnRegisterOnDestroyTrigger : MonoBehaviour
{
    private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

    public void AddUnRegister(IUnRegister unRegister)
    {
        mUnRegisters.Add(unRegister);
    }

    public void RemoveUnRegister(IUnRegister unRegister)
    {
        mUnRegisters.Remove(unRegister);
    }

    private void OnDestroy()
    {
        foreach (var unRegister in mUnRegisters)
        {
            unRegister.UnRegister();
        }

        mUnRegisters.Clear();
    }
}

public static class UnRegisterExtension
{
    public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister, GameObject gameObject)
    {
        var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();

        if (!trigger)
        {
            trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
        }

        trigger.AddUnRegister(unRegister);

        return unRegister;
    }

    public static IUnRegister UnRegisterWhenGameObjectDestroyed<T>(this IUnRegister self, T component)
        where T : Component
    {
        return self.UnRegisterWhenGameObjectDestroyed(component.gameObject);
    }
}
