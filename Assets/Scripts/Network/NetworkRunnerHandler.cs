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
        // �����ǰ������û��NetworkRunner�����prefab������
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "Network Runner";
        }

        // ��ʼ��NetworkRunner��������������
        Task clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, "TestSession", NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);

        Utils.DebugLog("InitializeNetworkRunner Called");
    }

    /// <summary>
    /// ����������ʱ����/ͬ��Unity����
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
    /// ����Runner(���롢��������)������StartGame��ʽ����
    /// </summary>
    /// <param name="networkRunner">ʹ�õ�Fusion������Ķ���</param>
    /// <param name="gameMode">����ģʽ��ʹ��AutoHostOrClient���Ƚ��������ΪHost���������ΪClient</param>
    /// <param name="sessionName">������</param>
    /// <param name="address">�����ַ</param>
    /// <param name="scene">��������</param>
    /// <param name="initialized">��ѡ�ص�</param>
    /// <returns></returns>
    protected virtual Task InitializeNetworkRunner(NetworkRunner networkRunner, GameMode gameMode, string sessionName, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        INetworkSceneManager sceneManager = GetSceneManager(networkRunner); // ��ȡ���������������������¼���ȷ��ÿ���ͻ��˶�����ͬһ������

        networkRunner.ProvideInput = true; // ��������ͬ��

        // networkRunner.StartGame()���������ӣ�����/���뷿�䣬ͬ����������ʼ��֡ͬ��
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