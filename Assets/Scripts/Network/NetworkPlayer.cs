using Fusion;
using Fusion.Addons.Physics;
using Unity.Cinemachine;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    [SerializeField]
    private Rigidbody rigidbody3D;

    [SerializeField]
    private NetworkRigidbody3D networkRigidbody3D;

    [SerializeField]
    private ConfigurableJoint mainJoint;

    [SerializeField]
    private Animator animator;

    // 玩家指令输入
    private Vector2 moveInputVector = Vector2.zero;
    private bool isJumpButtonPressed = false;
    private bool isRevivedButtonPressed = false;

    // 角色属性
    private float maxSpeed = 3f;

    // 角色状态
    private bool isGrounded = false;
    private bool isActiveRagdoll = true;
    public bool IsActiveRagdoll => isActiveRagdoll;

    // Raycasts
    private RaycastHit[] raycastHits = new RaycastHit[10];

    // 同步动画
    private SyncPhysicsObject[] syncPhysicsObjects;

    // Cinemachine
    private CinemachineCamera cinemachineCamera;

    // 让客户端可以同步Ragdoll
    [Networked, Capacity(10)] public NetworkArray<Quaternion> networkPhysicsSyncedRotations { get; }

    // 保存原始数据
    private float startSlerpPositionSpring = 0.0f;

    private void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    private void Start()
    {
        startSlerpPositionSpring = mainJoint.slerpDrive.positionSpring; // 存储spring的原始状态，因为需要在玩家被击中后从ragdoll状态恢复
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

        // 复活按键输入
        if (Input.GetKeyDown(KeyCode.R))
        {
            isRevivedButtonPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        Vector3 localVelocityForward = Vector3.zero;
        float localForwardVelocity = 0;

        // 权威端(Client/Host)执行：即有权利修改网络对象的同步状态
        if (Object.HasStateAuthority)
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

            localVelocityForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);
            localForwardVelocity = localVelocityForward.magnitude;
        }

        // 处理玩家输入
        if (GetInput(out NetworkInputData networkInputData))
        {
            // 控制角色移动与旋转
            float inputMagnityed = networkInputData.movementInput.magnitude;
            
            if(isActiveRagdoll)
            {
                if (inputMagnityed != 0)
                {
                    Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(networkInputData.movementInput.x * -1, 0, networkInputData.movementInput.y), transform.up);
                    mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Runner.DeltaTime * 300);

                    if (localForwardVelocity < maxSpeed)
                    {
                        rigidbody3D.AddForce(transform.forward * inputMagnityed * 30);
                    }
                }

                // 控制角色跳跃
                if (isGrounded && networkInputData.isJumpPressed)
                {
                    rigidbody3D.AddForce(Vector3.up * 20, ForceMode.Impulse);
                    isJumpButtonPressed = false;
                }
            }
        }

        // 权威端更新动画和物理同步
        if (Object.HasStateAuthority)
        {
            // 更新动画参数
            animator.SetFloat("movementSpeed", localForwardVelocity * 0.4f);

            // 根据动画更新joints rotation，同步动画到物理
            for (int i = 0; i < syncPhysicsObjects.Length; i++)
            {
                syncPhysicsObjects[i].UpdateJointFromAnimation();
                networkPhysicsSyncedRotations.Set(i, syncPhysicsObjects[i].transform.localRotation);
            }

            if(transform.position.y < -10)
            {
                networkRigidbody3D.Teleport(Vector3.zero, Quaternion.identity);
            }
        }
    }

    // 物理？
    public override void Render()
    {
        if(!Object.HasStateAuthority)
        {
            var interpolated = new NetworkBehaviourBufferInterpolator(this);

            for(int i = 0; i < syncPhysicsObjects.Length;i++)
            {
                syncPhysicsObjects[i].transform.localRotation = Quaternion.Slerp(syncPhysicsObjects[i].transform.localRotation, networkPhysicsSyncedRotations.Get(i), interpolated.Alpha);
            }
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        // Move data
        networkInputData.movementInput = moveInputVector;

        if(isJumpButtonPressed)
        {
            networkInputData.isJumpPressed = true;
        }

        // Rest jump button
        isJumpButtonPressed = false;

        return networkInputData;
    }

    public void OnPlayerBodyPartHit()
    {
        if (!IsActiveRagdoll) return;

        MakeRagdoll();
    }

    // 当角色出现碰撞时，进入Ragdoll状态
    private void MakeRagdoll()
    {
        if (!Object.HasStateAuthority) return;

        // 更新mainJoint
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0;
        mainJoint.slerpDrive = jointDrive;

        // 更新所有的joint rotation值并发送给client
        for(int i = 0; i < syncPhysicsObjects.Length;i++)
        {
            syncPhysicsObjects[i].MakeRagdoll();
        }

        isActiveRagdoll = false;
    }

    // 结束Ragdoll状态，进入Active Ragdoll（初始）状态
    private void MakeActiveRagdoll()
    {
        if (!Object.HasStateAuthority) return;

        // 更新mainJoint
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;

        // 更新所有的joint rotation值并发送给client
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].MakeActiveRagdoll();
        }

        isActiveRagdoll = true;
    }
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("CauseDamage"))
        {
            MakeRagdoll();
        }
    }
    */
    public override void Spawned()
    {
        if(Object.HasInputAuthority)
        {
            Local = this;

            cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            cinemachineCamera.Follow = transform;

            Utils.DebugLog("Spawn player with input aythority");
        }
        else
        {
            Utils.DebugLog("Spawn player without input aythority");
        }

        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {

    }
}