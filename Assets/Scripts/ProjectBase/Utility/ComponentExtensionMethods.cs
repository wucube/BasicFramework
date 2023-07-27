using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UnityEngine组件的拓展方法
/// </summary>
public static class ComponentExtensionMethods
{
    /// <summary>
    /// 通过名称查找对象及子对象中的指定Transform
    /// </summary>
    /// <param name="self"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static Transform FindChildFromAllChild(this Component self, string childName)
    {
        Transform child = self.transform.Find(childName);
        if (child == null)
        {
            for (int i = 0; i < self.transform.childCount; i++)
            {
                child = FindChildFromAllChild(self.transform.GetChild(i), childName);

                if (child != null) return child;
            }
        }

        return child;
    }

    /// <summary>
    /// 通过名称获取子对象中的指定组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
    public static T GetChildComponentFromAllChild<T>(this Component parent, string childName) where T : Component
    {
        Transform child = FindChildFromAllChild(parent, childName);

        if (child != null) return child.GetComponent<T>();

        else return default(T);
    }

}