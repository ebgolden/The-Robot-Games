using System.Collections.Generic;
using UnityEngine;

public class ObstacleListener : MonoBehaviour
{
    private List<Collider> collidersTouching = new List<Collider>();

    public void OnTriggerEnter(Collider other)
    {
        collidersTouching.Add(other);
    }

    public void OnTriggerExit(Collider other)
    {
        collidersTouching.Remove(other);
    }

    public void Update()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public List<Collider> getCollidersTouching()
    {
        return collidersTouching;
    }
}