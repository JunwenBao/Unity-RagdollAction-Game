using UnityEngine;

public class HandGrabHandler : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private FixedJoint fixedJoint;

    private Rigidbody rigidbody3D;

    private NetworkPlayer networkPlayer;

    private void Awake()
    {
        networkPlayer = transform.root.GetComponent<NetworkPlayer>();
        rigidbody3D = GetComponent<Rigidbody>();

        rigidbody3D.solverIterations = 100;
    }

    public void UpdateState()
    {
        if(networkPlayer.IsGrabbingActive)
        {
            animator.SetBool("isGrabing", true);
        }
        else
        {
            if(fixedJoint != null)
            {
                if(fixedJoint.connectedBody != null)
                {
                    float forceAmountMultipler = 0.1f;

                    if(fixedJoint.connectedBody.transform.root.TryGetComponent(out NetworkPlayer otherPlayerNetworkPlayer))
                    {
                        if (otherPlayerNetworkPlayer.IsActiveRagdoll) forceAmountMultipler = 10;
                        else forceAmountMultipler = 15;
                    }

                    fixedJoint.connectedBody.AddForce((networkPlayer.transform.forward + Vector3.up * 0.25f) * forceAmountMultipler, ForceMode.Impulse);
                }

                Destroy(fixedJoint);
            }
        }

        animator.SetBool("isCarring", false);
        animator.SetBool("isGrabing", false);
    }

    private bool TryCarryObject(Collision collision)
    {
        if (!networkPlayer.Object.HasStateAuthority) return false;

        if (!networkPlayer.IsActiveRagdoll) return false;

        // 如果当前正在抓某个物品，则不执行
        if (!networkPlayer.IsGrabbingActive) return false;

        if (fixedJoint != null) return false;

        if (collision.transform.root == networkPlayer.transform) return false;

        if (!collision.collider.TryGetComponent(out Rigidbody otherObjectRigidbody)) return false;

        fixedJoint = transform.gameObject.AddComponent<FixedJoint>();

        fixedJoint.connectedBody = otherObjectRigidbody;

        fixedJoint.autoConfigureConnectedAnchor = false;

        // 将碰撞点从世界空间转换为局部空间
        fixedJoint.connectedAnchor = collision.transform.InverseTransformPoint(collision.GetContact(0).point);

        // 播放动画
        animator.SetBool("isCarrying", true);

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCarryObject(collision);
    }
}
