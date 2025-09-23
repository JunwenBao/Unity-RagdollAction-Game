using Fusion;
using UnityEngine;

public class NetworkRotateObject : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rigidbody3D;

    [SerializeField]
    private Vector3 rotationAmount;

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            Vector3 rotateBy = transform.rotation.eulerAngles + rotationAmount * Runner.DeltaTime;

            if(rigidbody3D != null)
            {
                rigidbody3D.MoveRotation(Quaternion.Euler(rotateBy));
            }
            else
            {
                transform.rotation = Quaternion.Euler(rotateBy);
            }
        }
    }
}