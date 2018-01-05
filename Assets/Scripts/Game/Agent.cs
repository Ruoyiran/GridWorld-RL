using UnityEngine;
namespace GridWorld
{
    public enum Action
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public class Agent : MonoBehaviour
    {
        public delegate void ColliderEvent(GameObject other);
        public event ColliderEvent OnColliderEvent;

        private float _reward = 0f;

        public void MoveUp()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
        }

        public void MoveDown()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
        }

        public void MoveLeft()
        {
            transform.position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
        }

        public void MoveRight()
        {
            transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
        }

        public float CheckReward()
        {
            _reward = -0.1f;
            Collider[] colliders = Physics.OverlapBox(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(0.3f, 0.3f, 0.3f));
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject obj = colliders[i].gameObject;
                if (obj != gameObject)
                {
                    if (OnColliderEvent != null)
                        OnColliderEvent(obj);
                    if (obj.CompareTag("Goal"))
                    {
                        _reward = 1;
                        break;
                    }
                    else if (obj.CompareTag("Pit"))
                    {
                        _reward = -1;
                        break;
                    }
                }
            }
            return _reward;
        }
    }
}