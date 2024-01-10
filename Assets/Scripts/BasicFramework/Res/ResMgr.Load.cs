using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// <see cref="Resources"/> 加载行为
/// </summary>
/// <remarks> 引用实现类的实例 </remarks>
public interface IResourceLoad { }

/// <summary>
/// 资源信息
/// </summary>
/// <typeparam name="T">资源类型</typeparam>
public class ResInfo<T> : IResourceLoad
{
    //资源
    public T asset;
    //异步加载结束后资源传递到外部时的委托
    public UnityAction<T> callback;
    //存储异步加载时开启的协同程序
    public Coroutine coroutine;
    //是否要移除
    public bool isDel;
}

public partial class ResMgr : Singleton<ResMgr>
{
    /// <summary>
    /// 记录加载过的或加载中的资源
    /// </summary>
    private Dictionary<string,IResourceLoad> resDict = new Dictionary<string, IResourceLoad>();
    private ResMgr() { }

    /// <summary>
    /// 同步加载<see cref="Resources"/>资源
    /// </summary>        
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    public T Load<T>(string path) where T: UnityEngine. Object
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成  TODO：能否用HashCode表示资源的唯一ID，或者使用StringBuilder拼接字符串节省性能
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> resInfo;

        //字典中存在指定资源时直接取出使用
        if(resDict.TryGetValue(resName, out IResourceLoad resource))
        {
            resInfo = resource as ResInfo<T>;
            //存在异步加载，资源还在加载中。异步加载最少下帧完成，所以当前帧的资源变量为空就表示异步加载还在进行中。
            if (resInfo.asset == null)
            {
                MonoMgr.Instance.StopCoroutine(resInfo.coroutine);//停止异步加载 
                T res = Resources.Load<T>(path);//直接采用同步加载
                resInfo.asset = res; //记录 
                resInfo.callback?.Invoke(res);//执行等待着异步加载结束的回调委托

                //回调结束，异步加载停止，清除无用的引用
                resInfo.callback = null;
                resInfo.coroutine = null;

                return res;
            }
            //如果已加载结束，直接用
            else
            {
                return resInfo.asset;
            }

        }
        //字典中不存在指定资源时，直接同步加载并且记录资源信息，便于下次直接取出使用
        else
        {
            T res = Resources.Load<T>(path);
            resInfo = new ResInfo<T>() { asset = res };
            resDict.Add(resName, resInfo);
            return res;
        }
    }

    /// <summary>
    /// 异步加载<see cref="Resources"/>资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <param name="callback">加载结束的回调</param>
    public void LoadAsync<T>(string path,UnityAction<T> callback) where T : UnityEngine.Object
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> resInfo;

        //若字典中记录了加载的资源信息
        if (resDict.TryGetValue(resName, out IResourceLoad resource))
        {
            resInfo = resource as ResInfo<T>;
            //如果资源还没有加载完，就表示还在进行异步加载
            if (resInfo.asset == null)
                resInfo.callback += callback;
            else
                callback.Invoke(resInfo.asset);
        }
        else
        {
            resInfo = new ResInfo<T>();
            //记录资源（资源还没有加载成功）
            resDict.Add(resName, resInfo);
            //记录传入的委托，资源加载完成后使用
            resInfo.callback += callback;
            //开启协程进行异步加载，记录协同程序（用于之后可能的停止协程）
            resInfo.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }

    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        //异步加载资源
        ResourceRequest request = Resources.LoadAsync<T>(path);
        //异步资源加载结束后，才继续执行yield return后的代码
        yield return request;

        string resName = path +"_"+typeof(T).Name;
        //资源加载结束后，将资源传到外部的委托函数中使用
        if (resDict.TryGetValue(resName,out IResourceLoad resource))
        {
            //取出资源信息 并且记录加载完成的资源
            ResInfo<T> resInfo = resource as ResInfo<T>;
            resInfo.asset = request.asset as T;
            //若资源需要移除，就直接移除
            if (resInfo.isDel)
                UnloadAsset<T>(path);
            else
            {
                //将加载完成的资源传递出去
                resInfo.callback?.Invoke(resInfo.asset);

                //加载完成后，清空引用，避免可能潜在的内存泄漏问题
                resInfo.callback = null;
                resInfo.coroutine = null;
            }

        }
    }

    /// <summary>
    /// 异步加载<see cref="Resources"/>资源
    /// </summary>
    [Obsolete("建议使用异步泛型加载。若使用 Type 参数加载，就不能与泛型加载混用来加载同类同名的资源")]
    public void LoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callback)
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成的
        string resName = path + "_" + type.Name;
        ResInfo<UnityEngine.Object> resInfo;

        if(resDict.TryGetValue(resName,out IResourceLoad resource))
        {
            resInfo = resource as ResInfo<UnityEngine.Object>;
            if (resInfo.asset == null)
                resInfo.callback += callback;
            else
                callback?.Invoke(resInfo.asset);
        }
        else
        {
            resInfo = new ResInfo<UnityEngine.Object>();
            resDict.Add(resName, resInfo);
            resInfo.callback += callback;
            resInfo.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path, type));
        }
    }

    private IEnumerator ReallyLoadAsync(string path, Type type)
    {
        ResourceRequest request = Resources.LoadAsync(path, type);
        yield return request;

        string resName = path + "_" + type.Name;
        if(resDict.TryGetValue(resName, out IResourceLoad resource))
        {
            ResInfo<UnityEngine.Object> resInfo = resource as ResInfo<UnityEngine.Object>;
            resInfo.asset = request.asset;

            if (resInfo.isDel)
                UnloadAsset(path, type);
            else
            {
                //将加载完成的资源传递出去
                resInfo.callback?.Invoke(resInfo.asset);

                resInfo.callback = null;
                resInfo.coroutine = null;
            }
        }

    }
    
}


