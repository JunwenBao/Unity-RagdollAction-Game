using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField]
    private Collider thisCollider;

    [SerializeField]
    private Collider[] colliderToIgnore;

    private void Start()
    {
        foreach (Collider collider in colliderToIgnore)
        {
            Physics.IgnoreCollision(thisCollider, collider, true);
        }
    }
}