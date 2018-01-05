using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<string, GameObject> _gameObjPool = new Dictionary<string, GameObject>();

    public GameObject InstantiateGameObjectFromPath(string path, string name = null, Transform parent = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            Logger.Error("ResourceManager.LoadPrefab - path can not be empty.");
            return null;
        }
        GameObject newObj = null;
        if (_gameObjPool.ContainsKey(path))
        {
            newObj = Object.Instantiate(_gameObjPool[path]);
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Logger.Error("ResourceManager.LoadPrefab - failed to load prefab from path '{0}'.", path);
                return null;
            }
            _gameObjPool.Add(path, prefab);
            newObj = Object.Instantiate(prefab);
        }
        if (parent != null)
            newObj.transform.SetParent(parent, false);
        if (!string.IsNullOrEmpty(name))
            newObj.name = name;
        return newObj;
    }
}
