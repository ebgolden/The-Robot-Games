using System.Collections.Generic;
using UnityEngine;

public class ProjectileListener : MonoBehaviour
{
    private List<Collider> collidersTouching = new List<Collider>();
    public bool collided = false;

    public void OnCollisionEnter(Collision collision)
    {
        collided = true;
        collidersTouching.Add(collision.collider);
    }

    public void OnTriggerEnter(Collider other)
    {
        collided = true;
        if (!collidersTouching.Contains(other))
            collidersTouching.Add(other);
    }

    public List<Collider> getCollidersTouching()
    {
        return collidersTouching;
    }
}