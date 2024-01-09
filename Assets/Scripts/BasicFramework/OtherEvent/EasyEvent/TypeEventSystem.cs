using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//基于EasyEvent重构的TypeEventSystem
public class TypeEventSystem
{
    private readonly EasyEvents mEvents = new EasyEvents();

    //便于使用TypeEventSystem对象
    public static readonly TypeEventSystem Global = new TypeEventSystem();

    public void Send<T>() where T : new()
    {
        mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());
    }

    public void Send<T>(T e)
    {
        mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);
    }

    public IUnRegister Register<T>(Action<T> onEvent)
    {
        var e = mEvents.GetOrAddEvent<EasyEvent<T>>();

        return e.Register(onEvent);
    }

    public void UnRegister<T>(Action<T> onEvent)
    {
        var e = mEvents.GetEvent<EasyEvent<T>>();

        if (e != null)
        {
            e.UnRegister(onEvent);
        }
    }
}
