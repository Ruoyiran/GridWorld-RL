using System;
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
    }
}