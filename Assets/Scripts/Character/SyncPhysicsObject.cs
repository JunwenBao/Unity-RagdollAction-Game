using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    private Rigidbody rigidbody3D;   // ��ǰ֫���Rigidbody����������ģ��
    private ConfigurableJoint joint; // ��ǰ֫���ConfigurableJoint������֫����Ը��ڵ���ת��Լ��

    [SerializeField]
    private Rigidbody animatedRigidBody3D; // ���ڶ�������Rigidbody

    [SerializeField]
    private bool syncAnimation = false;    // ���ݶ�������֫����ת

    private Quaternion startLocalRotation; // �洢��ʼ�ֲ���ת�����ں�����㶯����תƫ��

    private float startSlerpPositionSpring = 0.0f; // ����joint��ԭʼslerp�������ȣ�����Ragdoll/Active Ragdoll�л�

    private void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        // �洢��ʼֵ
        startLocalRotation = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    // ��������֫�嶯��
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

    // ����Ragdoll״̬������Active Ragdoll����ʼ��״̬���ָ���ʼֵ
    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
