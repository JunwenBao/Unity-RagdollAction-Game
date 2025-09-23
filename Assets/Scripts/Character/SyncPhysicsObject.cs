using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    private Rigidbody rigidbody3D;
    private ConfigurableJoint joint;

    [SerializeField]
    private Rigidbody animatedRigidBody3D;

    [SerializeField]
    private bool syncAnimation = false;

    private Quaternion startLocalRotation;

    private float startSlerpPositionSpring = 0.0f;

    private void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        // �洢��ʼֵ
        startLocalRotation = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation) return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidBody3D.transform.localRotation, startLocalRotation);
    }

    // ����ɫ������ײʱ������Ragdoll״̬
    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1;
        joint.slerpDrive = jointDrive;
    }

    // ����Ragdoll״̬������Active Ragdoll����ʼ��״̬
    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
