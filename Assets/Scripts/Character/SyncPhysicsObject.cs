using UnityEngine;

public class SyncPhysicsObject : MonoBehaviour
{
    private Rigidbody rigidbody3D;   // 当前肢体的Rigidbody，用于物理模拟
    private ConfigurableJoint joint; // 当前肢体的ConfigurableJoint，控制肢体相对父节点旋转和约束

    [SerializeField]
    private Rigidbody animatedRigidBody3D; // 用于动画驱动Rigidbody

    [SerializeField]
    private bool syncAnimation = false;    // 根据动画更新肢体旋转

    private Quaternion startLocalRotation; // 存储初始局部旋转，用于后面计算动画旋转偏移

    private float startSlerpPositionSpring = 0.0f; // 保存joint的原始slerp驱动力度，用于Ragdoll/Active Ragdoll切换

    private void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();

        // 存储初始值
        startLocalRotation = transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    // 动画驱动肢体动作
    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation) return;

        ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRigidBody3D.transform.localRotation, startLocalRotation);
    }

    // 当角色出现碰撞时，进入Ragdoll状态
    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1;
        joint.slerpDrive = jointDrive;
    }

    // 结束Ragdoll状态，进入Active Ragdoll（初始）状态，恢复初始值
    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}
