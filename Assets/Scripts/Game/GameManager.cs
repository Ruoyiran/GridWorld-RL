using Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GridWorld
{
    public class GameManager : SingletonMono<GameManager>
    {
        public Text scoreText;
        private const string ENVIRONMENT_PREFAB_PATH = "Prefabs/Environment";
        private Environment _env;
        private AgentObserver _angetObserver;
        private new void Awake()
        {
            base.Awake();
            LoadEnvironment();
            CreateAgentObserverObj();
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            AgentStepMessage msg = null;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                msg = _env.Step(Action.Left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                msg = _env.Step(Action.Right);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                msg = _env.Step(Action.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                msg = _env.Step(Action.Down);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
                _env.Reset();
            if(msg != null)
                Logger.Print("Reward: {0}", msg.Reward);
#endif
            scoreText.text = _env.TotalReward.ToString();
        }

        private void LoadEnvironment()
        {
            GameObject envObj = ResourceManager.Instance.InstantiateGameObjectFromPath(ENVIRONMENT_PREFAB_PATH, "Environment");
            _env = CTool.GetOrAddComponent<Environment>(envObj);
        }

        private void CreateAgentObserverObj()
        {
            GameObject obj = CTool.CreateEmptyGameObject("AgentObserver");
            _angetObserver = obj.AddComponent<AgentObserver>();
            _angetObserver.SetEnvironment(_env);
            _angetObserver.CreateCommunicator();
        }
    }
}