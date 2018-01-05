using Utils;
using UnityEngine;
using System;

namespace GridWorld
{
    public class GameManager : SingletonMono<GameManager>
    {
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
            float reward = 0f;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                reward = _env.Step(Action.Left);
                print("Reward: " + reward);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                reward = _env.Step(Action.Right);
                print("Reward: " + reward);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                reward = _env.Step(Action.Up);
                print("Reward: " + reward);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                reward = _env.Step(Action.Down);
                print("Reward: " + reward);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
                _env.Reset();
#endif
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