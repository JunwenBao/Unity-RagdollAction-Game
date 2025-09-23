using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;       // �洢�����ƽ�淽�������
    public NetworkBool isJumpPressed;   // �洢����ڵ�ǰ֡�Ƿ�����Ծ����˲ʱ���룩
    public NetworkBool isRevivePressed; // �洢����Ƿ��¸����
}