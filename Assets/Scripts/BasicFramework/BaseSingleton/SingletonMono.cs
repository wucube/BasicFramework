using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手动挂载继承Mono的单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance => instance;

    protected virtual void Awake()
    {
        //若已存在一个对应的单例对象，就不需要另外的
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this as T;

        //挂载继承单例基类的脚本后，依附的对象过场景时不会被移除，偄保证在游戏的整个生命周期中都存在 
        DontDestroyOnLoad(instance);
    }
}