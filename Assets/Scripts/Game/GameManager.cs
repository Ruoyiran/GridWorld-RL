using Utils;
using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : SingletonMono<GameManager>
{
    private const string ENVIRONMENT_PREFAB_PATH = "Prefabs/Environment";
    private const string AGENT_PREFAB_PATH = "Prefabs/Agent";
    private const string OBSTACLE_PREFAB_PATH = "Prefabs/Pit";
    private const string GOAL_PREFAB_PATH = "Prefabs/Goal";
    private AgentController _agentController;
    private EnvironmentManager _envManager;
    private List<Vector3> _allPositions;
    private List<GameObject> _obstacleObjs = new List<GameObject>();
    private List<GameObject> _goalObjs = new List<GameObject>();
    private GameObject _parentOfGoals;
    private GameObject _parentOfObstacles;

    private void Start ()
    {
        LoadEnvironment();
        LoadAgent();
        AddOnClliderEvent();
        InitAllPoints();
        PlaceObjects();
    }

    private void AddOnClliderEvent()
    {
        if (_agentController == null)
            return;
        _agentController.OnColliderEvent += OnColliderEventHandler;
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _agentController.Move(AgentAction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _agentController.Move(AgentAction.Right);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _agentController.Move(AgentAction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _agentController.Move(AgentAction.Down);
        }
#endif
    }

    private void InitAllPoints()
    {
        _allPositions = new List<Vector3>(_envManager.gridSize * _envManager.gridSize);
        for (int x = 0; x < _envManager.gridSize; ++x)
        {
            for (int y = 0; y < _envManager.gridSize; ++y)
            {
                _allPositions.Add(new Vector3(x, 0, y));
            }
        }
    }

    private void LoadEnvironment()
    {
        GameObject envObj = ResourceManager.Instance.InstantiateGameObjectFromPath(ENVIRONMENT_PREFAB_PATH, "Environment");
        _envManager = CTool.GetOrAddComponent<EnvironmentManager>(envObj);
    }

    private void LoadAgent()
    {
        GameObject agentObj = ResourceManager.Instance.InstantiateGameObjectFromPath(AGENT_PREFAB_PATH, "Agent");
        _agentController = CTool.GetOrAddComponent<AgentController>(agentObj);
    }

    public AgentController GetAgentController()
    {
        return _agentController;
    }

    public EnvironmentManager GetEnvironmentManager()
    {
        return _envManager;
    }

    private void PlaceObjects()
    {
        int oneAgent = 1;
        int totalPoints = _envManager.numObstacles + _envManager.numGoals + oneAgent;
        List<Vector3> points = Algorithm.RandomSample(_allPositions, totalPoints);
        PlaceAgent(points[0]);
        PlaceObstacles(points, oneAgent, oneAgent + _envManager.numObstacles);
        PlaceGoals(points, oneAgent + _envManager.numObstacles, points.Count);
    }

    private void PlaceAgent(Vector3 position)
    {
        if (_agentController == null)
            return;
        _agentController.transform.position = position;
    }

    private void PlaceObstacles(List<Vector3> points, int startIndex, int endIndex)
    {
        if(_parentOfObstacles == null)
            _parentOfObstacles = CTool.CreateEmptyGameObject("Pits");
        for (int i = startIndex; i < endIndex; i++)
        {
            int index = i - startIndex;
            if (_obstacleObjs.Count <= index)
            {
                GameObject obj = ResourceManager.Instance.InstantiateGameObjectFromPath(OBSTACLE_PREFAB_PATH);
                if (obj != null)
                {
                    obj.transform.SetParent(_parentOfObstacles.transform, false);
                    obj.transform.position = points[i];
                    _obstacleObjs.Add(obj);
                }
            }
            else
            {
                GameObject obj = _obstacleObjs[index];
                obj.transform.position = points[i];
            }
        }
    }

    private void PlaceGoals(List<Vector3> points, int startIndex, int endIndex)
    {
        if(_parentOfGoals == null)
            _parentOfGoals = CTool.CreateEmptyGameObject("Goals");
        for (int i = startIndex; i < endIndex; i++)
        {
            int index = i - startIndex;
            if (_goalObjs.Count <= index)
            {
                GameObject obj = ResourceManager.Instance.InstantiateGameObjectFromPath(GOAL_PREFAB_PATH);
                if (obj != null)
                {
                    obj.transform.SetParent(_parentOfGoals.transform, false);
                    obj.transform.position = points[i];
                    _goalObjs.Add(obj);
                }
            }
            else
            {
                GameObject obj = _goalObjs[index];
                obj.transform.position = points[i];
            }
        }
    }

    private void OnColliderEventHandler(GameObject other)
    {
        other.transform.position = GetNextAvailablePosition();
    }

    private Vector3 GetNextAvailablePosition()
    {
        HashSet<int> inavaiablePositions = GetInavaiablePositions();
        List<Vector3> availablePoints = new List<Vector3>();
        for (int i = 0; i < _allPositions.Count; i++)
        {
            if(!inavaiablePositions.Contains(i))
                availablePoints.Add(_allPositions[i]);
        }
        if (availablePoints.Count == 0)
            return Vector3.zero;
        List<Vector3> points = Algorithm.RandomSample(availablePoints, 1);
        return points[0];
    }

    private HashSet<int> GetInavaiablePositions()
    {
        HashSet<int> inavaiablePositions = new HashSet<int>();
        inavaiablePositions.Add(GetHashIndex(_agentController.transform.position));
        for (int i = 0; i < _goalObjs.Count; i++)
            inavaiablePositions.Add(GetHashIndex(_goalObjs[i].transform.position));
        for (int i = 0; i < _obstacleObjs.Count; i++)
            inavaiablePositions.Add(GetHashIndex(_obstacleObjs[i].transform.position));
        return inavaiablePositions;
    }

    private int GetHashIndex(Vector3 pos)
    {
        return (int)(pos.x * _envManager.gridSize + pos.z);
    }
}
