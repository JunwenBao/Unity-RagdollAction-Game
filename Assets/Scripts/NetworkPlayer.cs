using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigidbody3D;

    [SerializeField]
    private ConfigurableJoint mainJoint;

    [SerializeField]
    private Animator animator;

    // 玩家指令输入
    private Vector2 moveInputVector = Vector2.zero;
    private bool isJumpButtonPressed = false;

    // 角色属性
    private float maxSpeed = 3f;

    // 角色状态
    private bool isGrounded = false;

    // Raycasts
    private RaycastHit[] raycastHits = new RaycastHit[10];

    // 同步动画
    private SyncPhysicsObject[] syncPhysicsObjects;

    private void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    private void Update()
    {
        // 移动按键输入
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        // 跳跃按键输入
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpButtonPressed = true;
        }
    }

    private void FixedUpdate()
    {
        // 通过射线检测，判断角色脚下0.5米范围是否有碰撞体
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.1f, transform.up * -1, raycastHits, 0.5f);

        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform) continue; // 跳过自身碰撞
            isGrounded = true; // 如果命中其他物体，则认为脚下有地面
            break;
        }

        // 如果角色在空中，则给刚体添加一个额外的重力
        if (!isGrounded)
        {
            rigidbody3D.AddForce(Vector3.down * 10);
        }

        // 控制角色移动与旋转
        float inputMagnityed = moveInputVector.magnitude;
        Vector3 localVelocityForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);
        float localForwardVelocity = localVelocityForward.magnitude;

        if (inputMagnityed != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInputVector.x * -1, 0, moveInputVector.y), transform.up);
            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.fixedDeltaTime * 300);
            
            if (localForwardVelocity < maxSpeed)
            {
                rigidbody3D.AddForce(transform.forward * inputMagnityed * 30);
            }
        }

        // 控制角色跳跃
        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * 20, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }

        // 更新动画参数
        animator.SetFloat("movementSpeed", localForwardVelocity * 0.4f);

        // 根据动画更新joints rotation，同步动画到物理
        for(int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}