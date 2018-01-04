using Utils;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    private const string ENVIRONMENT_PREFAB_PATH = "Prefabs/Environment";
    private const string AGENT_PREFAB_PATH = "Prefabs/Agent";
    private AgentController _agentController;
    private EnvironmentManager _envManager;

    void Start ()
    {
        LoadEnvironment();
        LoadAgent();
        //StartCoroutine(Move());
    }

    private void Update()
    {
    }

    private System.Collections.IEnumerator Move()
    {
        while (true)
        {
            for (int i = 0; i < 10; i++)
            {
                _agentController.Move(AgentAction.Right);
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < 10; i++)
            {
                _agentController.Move(AgentAction.Up);
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < 10; i++)
            {
                _agentController.Move(AgentAction.Left);
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < 10; i++)
            {
                _agentController.Move(AgentAction.Down);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void LoadEnvironment()
    {
        GameObject envObj = ResourceManager.Instance.LoadPrefab(ENVIRONMENT_PREFAB_PATH, "Environment");
        _envManager = CTool.GetOrAddComponent<EnvironmentManager>(envObj);
    }

    private void LoadAgent()
    {
        GameObject agentObj = ResourceManager.Instance.LoadPrefab(AGENT_PREFAB_PATH, "Agent");
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
}
