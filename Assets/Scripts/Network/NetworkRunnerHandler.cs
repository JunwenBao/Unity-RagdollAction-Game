using Fusion;
using Fusion.Sockets;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunner;

    private void Awake()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
    }

    private void Start()
    {
        // 如果当前场景中没有NetworkRunner，则从prefab中生成
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
        }

        // 初始化NetworkRunner，启动网络连接
        Task clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, "TestSession", NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);

        Utils.DebugLog("InitializeNetworkRunner Called");
    }

    /// <summary>
    /// 负责在联机时加载/同步Unity场景
    /// </summary>
    /// <param name="runner"></param>
    /// <returns></returns>
    INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        INetworkSceneManager sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if(sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    /// <summary>
    /// 配置Runner(输入、场景管理)并调用StartGame正式启动
    /// </summary>
    /// <param name="networkRunner">使用的Fusion网络核心对象</param>
    /// <param name="gameMode">运行模式：使用AutoHostOrClient，先进房间的作为Host，后进的作为Client</param>
    /// <param name="sessionName">房间名</param>
    /// <param name="address">网络地址</param>
    /// <param name="scene">场景引用</param>
    /// <param name="initialized">可选回调</param>
    /// <returns></returns>
    protected virtual Task InitializeNetworkRunner(NetworkRunner networkRunner, GameMode gameMode, string sessionName, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        INetworkSceneManager sceneManager = GetSceneManager(networkRunner); // 获取场景管理器：监听场景事件，确保每个客户端都加载同一个场景

        networkRunner.ProvideInput = true; // 启用输入同步

        // networkRunner.StartGame()：网络连接，创建/加入房间，同步场景，初始化帧同步
        return networkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            SceneManager = sceneManager
        });
    }
}