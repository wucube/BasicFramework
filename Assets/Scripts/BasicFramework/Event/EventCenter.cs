using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//TODO:使用Plane老师教的事件写法来改造事件中心，并且将延迟执行的缓存事件字典用起来_《Unity动作游戏开发实战》

/// <summary>
/// 事件类型
/// </summary>
public enum EventType
{
    /// <summary>
    /// 怪物死亡事件 —— 参数：Monster
    /// </summary>
    Monster_Dead,
    /// <summary>
    /// 玩家获取奖励 —— 参数：int
    /// </summary>
    Player_GetReward,
    Input_KeyDown,
    Input_KeyUp,

    LoadingBar_Update,
    /// <summary>
    /// 测试用事件 —— 参数：无
    /// </summary>
    Test
}

/// <summary>
/// 事件抽象类，用于引用派生类的实例
/// </summary>
public abstract class EventBase { }

/// <summary>
/// 非泛型事件类，包裹无参的委托
/// </summary>
public class EventInfo : EventBase
{
    public UnityAction actions;
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 泛型事件类，包裹有参的委托
/// </summary>
/// <typeparam name="T"></typeparam>
public class EventInfo<T> : EventBase
{
    public UnityAction<T> actions;
    public EventInfo( UnityAction<T> action)
    {
        actions += action;
    }
}

/// <summary> 
/// 事件中心
/// </summary>
public class EventCenter : Singleton<EventCenter>
{
    /// <summary>
    /// 事件字典
    /// </summary>
    /// <remarks> 记录事件类型及对应事件处理器 </remarks>
    private Dictionary<EventType, EventBase> eventDict = new Dictionary<EventType, EventBase>();

    // 事件缓存，应对事件未注册但已触发的情况
    private Dictionary<EventType, EventBase> eventCacheDict = new Dictionary<EventType, EventBase>();
   
    private EventCenter() { }

    /// <summary>
    /// 添加带参数的事件监听
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public void AddListener<T>(EventType eventType, UnityAction<T> action)
    {
        //已有对应的事件监听，就追加监听
        if( eventDict.TryGetValue(eventType, out EventBase evt) )
            (evt as EventInfo<T>).actions += action;
        //没有对应事件监听，则添加新的事件监听
        else
            eventDict.Add(eventType, new EventInfo<T>(action));
    }

    /// <summary>
    /// 监听不需要参数传递的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddListener(EventType eventType, UnityAction action)
    {
        if( eventDict.TryGetValue(eventType, out EventBase evt) )
            (evt as EventInfo).actions += action;
        else
            eventDict.Add(eventType, new EventInfo(action));
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    public void RemoveListener<T>(EventType eventType, UnityAction<T> action)
    {
        if (eventDict.TryGetValue(eventType, out EventBase evt))
            (evt as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 移除不需要参数的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void RemoveListener(EventType eventType, UnityAction action)
    {
        if (eventDict.TryGetValue(eventType, out EventBase evt))
            (evt as EventInfo).actions -= action;
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    public void Trigger<T>(EventType eventType, T info)
    {
        if (eventDict.TryGetValue(eventType, out EventBase evt))
            (evt as EventInfo<T>).actions?.Invoke(info);
    }

    public void Trigger(EventType eventType)
    {
        if (eventDict.TryGetValue(eventType, out EventBase evt))
            (evt as EventInfo).actions?.Invoke();
    }

    /// <summary>
    /// 清除事件字典的记录
    /// </summary>
    public void Clear()
    {
        eventDict.Clear();
    }

    public void Clear(EventType eventType)
    {
        if (eventDict.ContainsKey(eventType))
            eventDict.Remove(eventType);
    }
}
