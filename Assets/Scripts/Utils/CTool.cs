﻿using UnityEngine;

public class CTool {

    public static GameObject FindGameObject(GameObject parentObj, string path)
    {
        if (parentObj == null || string.IsNullOrEmpty(path))
            return null;
        Transform trans = parentObj.transform.Find(path);
        if (trans == null)
            return null;
        return trans.gameObject;
    }

    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        if (obj == null)
            return null;
        T comp = obj.GetComponent<T>();
        if(comp == null)
            comp = obj.AddComponent<T>();
        return comp;
    }

    public static GameObject CreateEmptyGameObject(string name = null)
    {
        GameObject obj = new GameObject();
        if (!string.IsNullOrEmpty(name))
            obj.name = name;
        ResetGameObjectTransform(obj);
        return obj;
    }

    public static void ResetGameObjectTransform(GameObject obj)
    {
        if (obj == null)
            return;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
    }
}
