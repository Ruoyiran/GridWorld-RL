using UnityEngine;

public class AgentController : MonoBehaviour {

	void Start () {
		
	}
	
    public void Move(AgentAction action)
    {
        switch (action) 
        {
            case AgentAction.Up:
                MoveUp();
                break;
            case AgentAction.Down:
                MoveDown();
                break;
            case AgentAction.Left:
                MoveLeft();
                break;
            case AgentAction.Right:
                MoveRight();
                break;
            default:
                break;
        }
    }

    private void MoveUp()
    {
        EnvironmentManager envMgr = GameManager.Instance.GetEnvironmentManager();
        int gridSize = envMgr.gridSize;
        if(transform.position.z + 1 < envMgr.gridSize)
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
    }

    private void MoveDown()
    {
        if (transform.position.z - 1 >= 0)
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
    }

    private void MoveLeft()
    {
        if (transform.position.x - 1 >= 0)
            transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
    }

    private void MoveRight()
    {
        EnvironmentManager envMgr = GameManager.Instance.GetEnvironmentManager();
        int gridSize = envMgr.gridSize;
        if (transform.position.x + 1 < gridSize)
            transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
    }
}
