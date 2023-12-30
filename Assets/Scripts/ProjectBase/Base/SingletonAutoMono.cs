using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自动挂载继承Mono的单例
/// </summary>
/// <remarks> 
/// <para>推荐使用</para>
/// <para>无需手动挂载,无需动态添加,无需关心切场景带来的问题</para> 
/// </remarks>
/// <typeparam name="T"></typeparam>
public class SingletonAutoMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //在场景中创建空物体，用于自动挂载单例脚本
                GameObject obj = new GameObject();
                obj.name = typeof(T).ToString();
                //动态挂载对应的单例模式脚本
                instance = obj.AddComponent<T>();
                //过场景不移除对象，保证在整个游戏生命周期中都存在
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
}
