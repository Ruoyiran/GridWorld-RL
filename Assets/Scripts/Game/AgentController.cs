using UnityEngine;

public class AgentController : MonoBehaviour {
    public delegate void ColliderEvent(GameObject other);
    public event ColliderEvent OnColliderEvent;

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
        CheckBlock();
    }

    private void MoveUp()
    {
        EnvironmentManager envMgr = GameManager.Instance.GetEnvironmentManager();
        int gridSize = envMgr.gridSize;
        if (transform.position.z + 1 < envMgr.gridSize)
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

    private void CheckBlock()
    {
        Collider[] blocks = Physics.OverlapBox(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(0.3f, 0.3f, 0.3f));
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].gameObject != gameObject)
            {
                if (OnColliderEvent != null)
                    OnColliderEvent(blocks[i].gameObject);
            }
        }
    }

}
