using Utils;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : SingletonMono<GameManager>
{
    private const string ENVIRONMENT_PREFAB_PATH = "Prefabs/Environment";
    private const string AGENT_PREFAB_PATH = "Prefabs/Agent";
    private const string OBSTACLE_PREFAB_PATH = "Prefabs/Pit";
    private const string GOAL_PREFAB_PATH = "Prefabs/Goal";
    private AgentController _agentController;
    private EnvironmentManager _envManager;
    private List<Vector2> _allPoints;
    private List<GameObject> _obstacleObjs = new List<GameObject>();
    private List<GameObject> _goalObjs = new List<GameObject>();
    private GameObject _parentOfGoals;
    private GameObject _parentOfObstacles;
    private void Start ()
    {
        LoadEnvironment();
        LoadAgent();
        InitAllPoints();
        PlaceObjects();
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
        _allPoints = new List<Vector2>(_envManager.gridSize * _envManager.gridSize);
        for (int x = 0; x < _envManager.gridSize; ++x)
        {
            for (int y = 0; y < _envManager.gridSize; ++y)
            {
                _allPoints.Add(new Vector2(x, y));
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
        List<Vector2> points = Algorithm.RandomSample(_allPoints, totalPoints);
        PlaceAgent(points[0]);
        PlaceObstacles(points, oneAgent, oneAgent + _envManager.numObstacles);
        PlaceGoals(points, oneAgent + _envManager.numObstacles, points.Count);
    }

    private void PlaceAgent(Vector2 position)
    {
        if (_agentController == null)
            return;
        _agentController.transform.position = new Vector3(position.x, 0, position.y);
    }

    private void PlaceObstacles(List<Vector2> points, int startIndex, int endIndex)
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
                    obj.transform.localPosition = new Vector3(points[i].x, 0, points[i].y);
                    _obstacleObjs.Add(obj);
                }
            }
            else
            {
                GameObject obj = _obstacleObjs[index];
                obj.transform.localPosition = new Vector3(points[i].x, 0, points[i].y);
            }
        }
    }

    private void PlaceGoals(List<Vector2> points, int startIndex, int endIndex)
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
                    obj.transform.localPosition = new Vector3(points[i].x, 0, points[i].y);
                    _goalObjs.Add(obj);
                }
            }
            else
            {
                GameObject obj = _goalObjs[index];
                obj.transform.localPosition = new Vector3(points[i].x, 0, points[i].y);
            }
        }
    }
}
