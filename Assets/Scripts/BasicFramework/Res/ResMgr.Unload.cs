using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class ResMgr
{
    /// <summary>
    /// 卸载一个指定的资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    public void UnloadAsset<T>(string path)
    {
        //资源的唯一ID，是通过 路径名_资源类型 拼接而成  TODO：能否用HashCode表示资源的唯一ID，或者使用StringBuilder拼接字符串节省性能
        string resName = path + "_" + typeof(T).Name;

        //若存在对应资源
        if(m_ResDict.TryGetValue(resName,out IResourceLoad resource))
        {
            ResInfo<T> resInfo = resource as ResInfo<T>;

            //资源已加载结束
            if(resInfo.asset != null)
            {
                m_ResDict.Remove(resName);
                //通过Resources API卸载资源
                Resources.UnloadAsset(resInfo.asset as UnityEngine.Object);
            }
            else//资源正在异步加载中
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //保险起见一定要移除资源
                resInfo.isDel = true;//改变表示 待删除

            }
        }
    }

    public void UnloadAsset(string path,Type type)
    {
        string resName = path + "_" + type.Name;
        if(m_ResDict.TryGetValue (resName,out IResourceLoad resource))
        {
            ResInfo<UnityEngine.Object> resInfo = resource as ResInfo<UnityEngine.Object>;
            //资源已经加载结束 
            if (resInfo.asset != null)
            {
                m_ResDict.Remove(resName);
                Resources.UnloadAsset(resInfo.asset);
            }
            else//资源正在异步加载中
            {
                //MonoMgr.Instance.StopCoroutine(resInfo.coroutine);
                //resDic.Remove(resName);
                //为了保险起见 一定要让资源移除了
                //改变表示 待删除
                resInfo.isDel = true;
            }
        }
    }

    public void UnloadUnusedAssets(UnityAction callback)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback = null)
    {
        AsyncOperation operation = Resources.UnloadUnusedAssets();
        yield return operation;
        //卸载完毕后 通知外部
        callback?.Invoke();
    }
}
