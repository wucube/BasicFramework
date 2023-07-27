using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UnityEngine组件的拓展方法
/// </summary>
public static class ComponentExtensionMethods
{
    /// <summary>
    /// 通过名称查找子对象的 <see langword = "Transform"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// 只会返回查找到的第一个<see langword = "Transform"/>
    /// </para>
    /// </remarks>
    /// <param name="self"></param>
    /// <param name="childName">子对象的名称</param>
    /// <returns></returns>
    public static Transform FindChild(this Component self, string childName)
    {
        Transform child = self.transform.Find(childName);

        if (child == null)
        {
            for (int i = 0; i < self.transform.childCount; i++)
            {
                child = FindChild(self.transform.GetChild(i), childName);

                if (child != null) return child;
            }
        }

        return child;
    }

    /// <summary>
    /// 通过子对象名称获取子对象中的指定组件
    /// </summary>
    /// <remarks>
    /// <para>
    /// 先通过 <see langword = "Transform"/> 找到指定Transform，再获取指定的 <see langword = "Component"/>
    /// </para>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static T GetChildComponent<T>(this Component parent, string childName) where T : Component
    {
        Transform child = FindChild(parent, childName);

        if (child != null) return child.GetComponent<T>();

        else return default(T);
    }



    #region 通过 Transform 查找子对象的两种方式
    ////递归实现查找子对象
    //public static Transform FindChind(Transform parent, string name)
    //{
    //    for (int i = 0; i < parent.childCount; ++i)
    //    {
    //        Transform child = parent.GetChild(i);
    //        if (child.name == name)
    //        {
    //            return child;
    //        }

    //        child = FindChind(child, name);
    //        if (child)
    //        {
    //            return child;
    //        }
    //    }
    //    return null;
    //}

    ////非递归实现查找子对象
    //public static Transform FindChildByName(Transform parent, string name)
    //{
    //    Queue<Transform> queue = new Queue<Transform>();

    //    queue.Enqueue(parent);

    //    while (queue.Count > 0)
    //    {
    //        Transform current = queue.Dequeue();
    //        if (current.name == name)
    //        {
    //            return current;
    //        }

    //        int childCount = current.childCount;
    //        for (int i = 0; i < childCount; i++)
    //        {
    //            Transform child = current.GetChild(i);
    //            queue.Enqueue(child);
    //        }
    //    }

    //    return null;
    //}

    #endregion

}