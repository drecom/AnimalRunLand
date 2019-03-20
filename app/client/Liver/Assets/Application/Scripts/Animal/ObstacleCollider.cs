using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObstacleCollider : MonoBehaviour
{
    public event Action<GameObject> OnCollision = (obj) => { };
    public event Action<GameObject> OnWall = (obj) => { };

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Obstacle")
        {
            OnCollision(other.gameObject);
            //Debug.Log("Obstacle!!!");
        }
        else if (other.tag == "Wall")
        {
            OnWall(other.gameObject);
        }
    }
}
