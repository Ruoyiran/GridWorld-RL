using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace GridWorld
{
    public class Environment : MonoBehaviour
    {
        private struct InnerState
        {
            public float reward;
            public bool isDone;
            public GameObject colliderObj;
            public InnerState(float reward = 0.0f, bool isDone = false, GameObject colliderObj = null)
            {
                this.reward = reward;
                this.isDone = isDone;
                this.colliderObj = colliderObj;
            }
        }
        private const string AGENT_PREFAB_PATH = "Prefabs/Agent";
        private const string OBSTACLE_PREFAB_PATH = "Prefabs/Pit";
        private const string GOAL_PREFAB_PATH = "Prefabs/Goal";

        public float TotalReward { get; set; }
        public static int gridSize = 7;
        public int numObstacles = 5;
        public int numGoals = 1;
        public int maxSteps = gridSize * gridSize;
        private int _currMoveSteps = 0;
        private int _envImageWidth = 84;
        private int _envImageHeight = 84;
        public Camera _envCamera;
        private GameObject _agentObj;
        private GameObject _planeObj;
        private GameObject _eastWallObj;
        private GameObject _westWallObj;
        private GameObject _northWallObj;
        private GameObject _southWallObj;
        private List<Vector3> _allPositions;
        private List<GameObject> _goalObjs;
        private List<GameObject> _obstacleObjs;
        private Agent _agent;
        private Vector3 _prevAgentPos;
        private TFModel _tfModel;
        private const string TF_MODEL_PATH = "Model/graph_def.bytes";

        void Start()
        {
            _tfModel = new TFModel(Path.Combine(Application.streamingAssetsPath, TF_MODEL_PATH));
            InitAllPositions();
            InitObjects();
            SetEnvironment();
            ResetEnv();
        }

        private void InitAllPositions()
        {
            _allPositions = new List<Vector3>(gridSize * gridSize);
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    _allPositions.Add(new Vector3(i, 0, j));
                }
            }
        }

        private void SetEnvironment()
        {
            Camera mainCamera = Camera.main;
            mainCamera.transform.position = new Vector3(-(gridSize - 1) / 2f, gridSize * 1.25f, -(gridSize - 1) / 2f);
            mainCamera.orthographicSize = (gridSize + 5f) / 2f;

            _planeObj.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
            _planeObj.transform.position = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);

            _eastWallObj.transform.position = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
            _eastWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

            _westWallObj.transform.position = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);
            _westWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

            _southWallObj.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
            _southWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);

            _northWallObj.transform.position = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
            _northWallObj.transform.localScale = new Vector3(1, 1, gridSize + 2);
        }

        private void InitObjects()
        {
            InitEnvironmentObjs();
            InitEnvCamera();
            LoadAgentObj();
            LoadObstacleObjs();
            LoadGoalObjs();
        }

        private void InitEnvironmentObjs()
        {
            _planeObj = CTool.FindGameObject(gameObject, "Plane");
            _eastWallObj = CTool.FindGameObject(gameObject, "East");
            _westWallObj = CTool.FindGameObject(gameObject, "West");
            _northWallObj = CTool.FindGameObject(gameObject, "North");
            _southWallObj = CTool.FindGameObject(gameObject, "South");
        }

        private void InitEnvCamera()
        {
            GameObject obj = CTool.FindGameObject(gameObject, "EnvCamera");
            _envCamera = obj.GetComponent<Camera>();
        }

        private void LoadAgentObj()
        {
            _agentObj = ResourceManager.Instance.InstantiateGameObjectFromPath(AGENT_PREFAB_PATH, "Agent", transform);
            CTool.ResetGameObjectTransform(_agentObj);
            _agent = CTool.GetOrAddComponent<Agent>(_agentObj);
        }

        private void LoadObstacleObjs()
        {
            _obstacleObjs = new List<GameObject>();
            for (int i = 0; i < numObstacles; i++)
            {
                GameObject obj = ResourceManager.Instance.InstantiateGameObjectFromPath(OBSTACLE_PREFAB_PATH, "Obstacle" + i, transform);
                CTool.ResetGameObjectTransform(obj);
                _obstacleObjs.Add(obj);
            }
        }

        private void LoadGoalObjs()
        {
            _goalObjs = new List<GameObject>();
            for (int i = 0; i < numGoals; i++)
            {
                GameObject obj = ResourceManager.Instance.InstantiateGameObjectFromPath(GOAL_PREFAB_PATH, "Goal" + i, transform);
                CTool.ResetGameObjectTransform(obj);
                _goalObjs.Add(obj);
            }
        }

        public byte[] Reset()
        {
            ResetEnv();
            return GetEnvironmentImageBytes();
        }

        public void ResetEnv()
        {
            TotalReward = 0;
            _currMoveSteps = 0;
            int oneAgent = 1;
            int totalPoints = numObstacles + numGoals + oneAgent;
            List<Vector3> points = Algorithm.RandomSample(_allPositions, totalPoints);
            PlaceObject(_agentObj, points[0]);
            PlaceObstacles(points, oneAgent, oneAgent + numObstacles);
            PlaceGoals(points, oneAgent + numObstacles, points.Count);
            _prevAgentPos = _agentObj.transform.position;
        }

        public AgentStepMessage Step(Action action)
        {
            if (_agent == null)
                return null;
            switch (action)
            {
                case Action.Up:
                    MoveUp();
                    break;
                case Action.Down:
                    MoveDown();
                    break;
                case Action.Left:
                    MoveLeft();
                    break;
                case Action.Right:
                    MoveRight();
                    break;
                default:
                    break;
            }
            _currMoveSteps += 1;
            InnerState state = GetCurrentState();
            AgentStepMessage msg = new AgentStepMessage
            {
                Reward = state.reward,
                IsDone = state.isDone
            };
            TotalReward += msg.Reward;
            _prevAgentPos = _agent.transform.position;
            ClearColliderObj(state.colliderObj);
            return msg;
        }

        public void AIStep()
        {
            if (_tfModel == null)
                return;
            int action = GetAIAction();
            Logger.Print("Action: {0}", action);
            Step((Action)action);
        }

        private int GetAIAction()
        {
            Texture2D tex = ImageTool.RenderToTex(_envCamera, _envImageWidth, _envImageHeight);
            byte[] bytes = tex.EncodeToPNG();
            float[,] qout = _tfModel.GetValue("Input", "Qout", bytes);
            int action = GetMaxValueIndex(qout);
            return action;
        }

        private int GetMaxValueIndex(float[,] input)
        {
            if (input == null)
                return 0;
            float max = input[0, 0];
            int index = 0;
            for (int i = 1; i < 4; i++)
            {
                if (input[0, i] > max)
                {
                    max = input[0, i];
                    index = i;
                }
            }
            return index;
        }

        private InnerState GetCurrentState()
        {
            InnerState state = new InnerState();
            Collider[] colliders = Physics.OverlapBox(new Vector3(_agentObj.transform.position.x, 0, _agentObj.transform.position.z), new Vector3(0.3f, 0.3f, 0.3f));
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject obj = colliders[i].gameObject;
                if (obj != _agentObj)
                {
                    state.colliderObj = obj;
                    if (obj.CompareTag("Goal"))
                    {
                        state.reward = 10.1f;
                        state.isDone = false;
                        _currMoveSteps = 0;
                        return state;
                    }
                    else if (obj.CompareTag("Pit"))
                    {
                        state.reward = -10.0f;
                        state.isDone = true;
                        return state;
                    }
                }
            }
            
            float prevDist = Vector3.Distance(_prevAgentPos, _goalObjs[0].transform.position);
            float currDist = Vector3.Distance(_agentObj.transform.position, _goalObjs[0].transform.position);
            state.reward = currDist < prevDist ? 0.1f : -0.1f;
            state.isDone = _currMoveSteps >= maxSteps ? true : false;
            state.colliderObj = null;
            return state;
        }

        private void ClearColliderObj(GameObject colliderObj)
        {
            if (colliderObj == null)
                return;
            colliderObj.transform.position = GetNextAvailablePosition();
        }

        public byte[] GetEnvironmentImageBytes()
        {
            Texture2D tex = ImageTool.RenderToTex(_envCamera, _envImageWidth, _envImageHeight);
            byte[] imageBytes = tex.EncodeToPNG();
            DestroyImmediate(tex);
            Resources.UnloadUnusedAssets();
            return imageBytes;
        }

        private void MoveUp()
        {
            if (_agent.transform.position.z + 1 < gridSize)
                _agent.MoveUp();
        }
        private void MoveDown()
        {
            if (_agent.transform.position.z - 1 >= 0)
                _agent.MoveDown();
        }

        private void MoveLeft()
        {
            if (_agent.transform.position.x - 1 >= 0)
                _agent.MoveLeft();
        }

        private void MoveRight()
        {
            if (_agent.transform.position.x + 1 < gridSize)
                _agent.MoveRight();
        }

        private void PlaceObject(GameObject obj, Vector3 position)
        {
            if (obj == null)
                return;
            obj.transform.position = position;
        }

        private void PlaceObstacles(List<Vector3> points, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                int index = i - startIndex;
                PlaceObject(_obstacleObjs[index], points[i]);
            }
        }

        private void PlaceGoals(List<Vector3> points, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                int index = i - startIndex;
                PlaceObject(_goalObjs[index], points[i]);
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
                if (!inavaiablePositions.Contains(i))
                    availablePoints.Add(_allPositions[i]);
            }
            if (availablePoints.Count == 0)
                return Vector3.zero;
            List<Vector3> points = Algorithm.RandomSample(availablePoints, 1);
            return points[0];
        }

        private HashSet<int> GetInavaiablePositions()
        {
            HashSet<int> inavaiablePositions = new HashSet<int>
        {
            GetHashIndex(_agentObj.transform.position)
        };
            for (int i = 0; i < _goalObjs.Count; i++)
                inavaiablePositions.Add(GetHashIndex(_goalObjs[i].transform.position));
            for (int i = 0; i < _obstacleObjs.Count; i++)
                inavaiablePositions.Add(GetHashIndex(_obstacleObjs[i].transform.position));
            return inavaiablePositions;
        }

        private int GetHashIndex(Vector3 pos)
        {
            return (int)(pos.x * gridSize + pos.z);
        }
    }
}