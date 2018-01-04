using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<string, GameObject> _gameObjPool = new Dictionary<string, GameObject>();

    public GameObject InstantiateGameObjectFromPath(string path, string name = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            Logger.Error("ResourceManager.LoadPrefab - path can not be empty.");
            return null;
        }
        if (_gameObjPool.ContainsKey(path))
        {
            return Object.Instantiate(_gameObjPool[path]);
        }
        GameObject prefab = Resources.Load<GameObject>(path);
        if(prefab == null)
        {
            Logger.Error("ResourceManager.LoadPrefab - failed to load prefab from path '{0}'.", path);
            return null;
        }
        GameObject newObj = Object.Instantiate(prefab);
        if (!string.IsNullOrEmpty(name))
            newObj.name = name;
        _gameObjPool.Add(path, prefab);
        return newObj;
    }
}
