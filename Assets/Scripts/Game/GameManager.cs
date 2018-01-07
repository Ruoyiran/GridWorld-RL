using Utils;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GridWorld
{
    public class GameManager : SingletonMono<GameManager>
    {
        enum PlayType
        {
            ManualPlay,
            AIPlay,
            NetworkPlay
        }
        public Text scoreText;
        public Toggle toggleManual;
        public Toggle toggleAI;
        public Toggle toggleNetwork;
        public Button btnUp;
        public Button btnDown;
        public Button btnLeft;
        public Button btnRight;
        public Button btnReset;
        private const string ENVIRONMENT_PREFAB_PATH = "Prefabs/Environment";
        private Environment _env;
        private AgentObserver _angetObserver;
        private Coroutine _aiPlayCoroutine;
        private PlayType _playType = PlayType.ManualPlay;
        private new void Awake()
        {
            Application.runInBackground = true;
            base.Awake();
            LoadEnvironment();
            CreateAgentObserverObj();
            AddToggleValueChangedListeners();
            AddButtonClickedListeners();
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
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                _env.ResetEnv();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                _env.AIStep();
            }
            if (msg != null)
            {
                Logger.Print("Reward: {0} IsDone: {1}", msg.Reward, msg.IsDone);
                if (msg.IsDone)
                    _env.ResetEnv();
            }
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
        }

        private void AddToggleValueChangedListeners()
        {
            toggleManual.onValueChanged.AddListener(OnManualPlayToggleValueChanged);
            toggleAI.onValueChanged.AddListener(OnAIPlayToggleValueChanged);
            toggleNetwork.onValueChanged.AddListener(OnNetworkPlayToggleValueChanged);
        }

        private void AddButtonClickedListeners()
        {
            btnUp.onClick.AddListener(() => { OnManualControlButtonClicked(Action.Up); });
            btnDown.onClick.AddListener(() => { OnManualControlButtonClicked(Action.Down); });
            btnLeft.onClick.AddListener(() => { OnManualControlButtonClicked(Action.Left); });
            btnRight.onClick.AddListener(() => { OnManualControlButtonClicked(Action.Right); });
            btnReset.onClick.AddListener(() => { _env.ResetEnv(); });
        }

        private void OnManualControlButtonClicked(Action action)
        {
            if (_playType != PlayType.ManualPlay)
                return;
            _env.Step(action);
        }

        private void OnManualPlayToggleValueChanged(bool isOn)
        {
            if (_playType == PlayType.ManualPlay)
                return;
            _playType = PlayType.ManualPlay;
        }

        private void OnAIPlayToggleValueChanged(bool isOn)
        {
            if (_playType == PlayType.AIPlay)
                return;
            _playType = PlayType.AIPlay;
            if (isOn)
            {
                print("AI Play");
                if (_aiPlayCoroutine != null)
                    StopCoroutine(_aiPlayCoroutine);
                _aiPlayCoroutine = StartCoroutine(AIPlay());
            }
            else
            {
                print("Stop AI Play");
                if (_aiPlayCoroutine != null)
                    StopCoroutine(_aiPlayCoroutine);
            }
        }

        private IEnumerator AIPlay()
        {
            while (true)
            {
                _env.AIStep();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnNetworkPlayToggleValueChanged(bool isOn)
        {
            if (_playType == PlayType.NetworkPlay)
                return;
            _playType = PlayType.NetworkPlay;
            if (isOn)
            {
                print("Network Play");
                if (_angetObserver.IsConnected)
                    return;
                _angetObserver.CreateCommunicator();
            }
            else
            {
                print("Stop Network Play");
                _angetObserver.Disconnect();
            }
        }
    }
}