using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 池中的数据对象
/// </summary>
public class PoolData
{
    //用来存储抽屉中的对象
    private Stack<GameObject> dataStack = new Stack<GameObject>();
    //抽屉根对象 用来进行布局管理的对象
    private GameObject rootObj;

    //获取容器中是否有对象
    public int Count => dataStack.Count;

    /// <summary>
    /// 初始化构造函数
    /// </summary>
    /// <param name="root">柜子（缓存池）父对象</param>
    /// <param name="name">抽屉父对象的名字</param>
    public PoolData(GameObject root, string name)
    {
        //开启功能时 才会动态创建 建立父子关系
        if (PoolMgr.isOpenLayout)
        {
            //创建抽屉父对象
            rootObj = new GameObject(name);
            //和柜子父对象建立父子关系
            rootObj.transform.SetParent(root.transform);
        }

    }

    /// <summary>
    /// 从抽屉中弹出数据对象
    /// </summary>
    /// <returns>想要的对象数据</returns>
    public GameObject Pop()
    {
        //取出对象
        GameObject obj = dataStack.Pop();
        //激活对象
        obj.SetActive(true);
        //断开父子关系
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(null);

        return obj;
    }

    /// <summary>
    /// 将物体放入到抽屉对象中
    /// </summary>
    /// <param name="obj"></param>
    public void Push(GameObject obj)
    {
        //失活放入抽屉的对象
        obj.SetActive(false);
        //放入对应抽屉的根物体中 建立父子关系
        if (PoolMgr.isOpenLayout)
            obj.transform.SetParent(rootObj.transform);
        //通过栈记录对应的对象数据
        dataStack.Push(obj);
    }
}

/// <summary>
/// 对象池(缓存池
/// </summary>
public class PoolMgr : Singleton<PoolMgr>
{
    //缓存池容器
    public Dictionary<string, Stack<GameObject>> poolDict = new Dictionary<string, Stack<GameObject>>();

    //池子根对象
    private GameObject poolObj;

    /// <summary>
    /// 是否开启布局功能
    /// </summary>
    public static bool isOpenLayout = false;

    private PoolMgr() { }

    /// <summary>
    /// 往外拿东西
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetObj(string name)
    {
        GameObject obj;
        //有抽屉 并且 抽屉里 有对象 才去直接拿
        if (poolDict.ContainsKey(name) && poolDict[name].Count > 0)
        {
            //弹出栈中的对象 直接返回给外部使用
            obj = poolDict[name].Pop();
            //激活对象 再返回
            obj.SetActive(true);
        }
        //否则，就应该去创造
        else
        {
            //没有的时候 通过资源加载 去实例化出一个GameObject
            obj = GameObject.Instantiate(Resources.Load<GameObject>(name));
            //实例化出来的对象重命名过后,方便往里面放
            obj.name = name;
        }

        return obj;
    }

    /// <summary>
    /// 往缓存池中放入对象
    /// </summary>
    /// <param name="name">抽屉（对象）的名字</param>
    /// <param name="obj">希望放入的对象</param>
    public void PushObj(GameObject obj)
    {
        //目的是把对象隐藏起来
        //并不是直接移除对象 而是将对象失活,用的时候再激活,还可以把对象放倒屏幕外看不见的地方
        obj.SetActive(false);

        //没有抽屉 创建抽屉
        if (!poolDict.ContainsKey(obj.name))
            poolDict.Add(obj.name, new Stack<GameObject>());

        //往抽屉当中放对象
        poolDict[obj.name].Push(obj);
    }

    /// <summary>
    /// 清空缓存池的方法,主要用在场景切换时
    /// </summary>
    public void Clear()
    {
        poolDict.Clear();
    }
}
