﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public interface IEventInfo { }

public class EventInfo : IEventInfo
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;

    public EventInfo( UnityAction<T> action)
    {
        actions += action;
    }
}


/// <summary> 
/// 事件中心 单例模式对象
/// 1.Dictionary
/// 2.委托
/// 3.观察者设计模式
/// 4.泛型
/// </summary>
public class EventCenter : Singleton<EventCenter>
{
    //key —— 事件的名字（比如：怪物死亡，玩家死亡，通关 等等）
    //value —— 对应的是 监听这个事件 对应的委托函数们
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    // 事件缓存，应对事件未注册但已触发的情况
    private Dictionary<string, IEventInfo> eventCacheDic = new Dictionary<string, IEventInfo>();


    /// <summary>
    /// 监听带有参数的事件
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">准备用来处理事件 的委托函数</param>
    public void AddListener<T>(string name, UnityAction<T> action)
    {
        //已有对应的事件监听，就追加监听
        if( eventDic.ContainsKey(name) )
        {
            (eventDic[name] as EventInfo<T>).actions += action;
        }
        //没有对应事件监听，则添加新的事件监听
        else
        {
            eventDic.Add(name, new EventInfo<T>( action ));
        }
    }

    /// <summary>
    /// 监听不需要参数传递的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions += action;
        }

        else
        {
            eventDic.Add(name, new EventInfo(action));
        }
    }


    /// <summary>
    /// 移除对应的事件监听
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">对应之前添加的委托函数</param>
    public void RemoveListener<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }

    /// <summary>
    /// 移除不需要参数的事件
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void RemoveListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions -= action;
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="name">哪一个名字的事件触发了</param>
    public void Trigger<T>(string name, T info, bool isDealyTrigger = false)
    {
        //有没有对应的事件监听
        //有的情况
        if (eventDic.ContainsKey(name))
        {
            //eventDic[name]();
            if((eventDic[name] as EventInfo<T>).actions != null)
                (eventDic[name] as EventInfo<T>).actions.Invoke(info);
            //eventDic[name].Invoke(info);
        }
    }

    /// <summary>
    /// 事件触发（不需要参数的）
    /// </summary>
    /// <param name="name"></param>
    public void Trigger(string name)
    {
        //有没有对应的事件监听
        //有的情况
        if (eventDic.ContainsKey(name))
        {
            //eventDic[name]();
            if ((eventDic[name] as EventInfo).actions != null)
                (eventDic[name] as EventInfo).actions.Invoke();
            //eventDic[name].Invoke(info);
        }
    }

    /// <summary>
    /// 清空事件中心
    /// 主要用在 场景切换时
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}
