using UnityEngine;

public class CTool : MonoBehaviour {

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
}
