using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//TODO:尝试使用ScriptalbeObject来写Unity中的单例
/// <summary>
/// 单例基类
/// </summary>
/// <remarks> 通过抽象类和反射调用私有构造函数，保证对象的唯一性 </remarks>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> where T : class//, new()
{
    private static T instance;

    //用于加锁的对象
    protected static readonly object lockObj = new object();
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                //线程锁保证线程安全
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //instance = new T();

                        Type type = typeof(T);
                        //利用反射得到无参私有构造函数来实例化对象
                        ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, //私有成员方法
                                                                            null,                                         //没有绑定对象
                                                                            Type.EmptyTypes,                              //没有参数
                                                                            null);                                        //没有参数修饰符

                        instance = constructor?.Invoke(null) as T ?? throw new InvalidOperationException("没有得到对应的无参构造函数");

                    }
                }
            }

            return instance;
                
        }
    }
}
