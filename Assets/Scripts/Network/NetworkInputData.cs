using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;       // 存储玩家在平面方向的输入
    public NetworkBool isJumpPressed;   // 存储玩家在当前帧是否按下跳跃键（瞬时输入）
    public NetworkBool isRevivePressed; // 存储玩家是否按下复活键
}