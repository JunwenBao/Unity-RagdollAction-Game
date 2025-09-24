# 基于Unity引擎的Ragdoll Active游戏
<p align="center">
  <img src="https://github.com/user-attachments/assets/00d66f04-c319-49ff-a923-fa52c9c0fc1a" 
       alt="Active Ragdoll Example" 
       width="600"/>
</p>

## 简介
本项目是一款基于Unity引擎和Photon Fusion网络框架开发的Ragdoll Active类游戏，旨在为游戏角色实现软体+布偶的动作感觉与运动效果。

## 玩法实现介绍
从游戏开发角度来讲，主动布娃娃移动(Active Ragdoll Locomotion)指的是通过在布娃娃式骨骼系统上施加力和旋转约束，使角色在玩家输入下产生可控但依然受物理影响的移动。
<br><br>
这类游戏的特点在于：面对玩家的指令输入并不是通过直接设置位置/旋转，而是基于物理力/扭矩的作用实现。角色的姿态受物理世界约束，从而实现较为“自然”的肢体摆动或摔倒动作，而这些并非是不可控的，玩家依旧可以对角色的动作做出控制指令。

### 实现原理
- 为角色的身体部位添加Rigidbody + Collider：让每一个肢体都可以在物理世界自由运动，并和环境发生碰撞。
- 为角色的关键肢体添加Configurable Joint：将各个肢体连接起来，同时限制运动范围，并提供弹性约束。
- 动画驱动肢体：对于角色的四肢，会在每一帧获取动画目标的旋转，即animatedRigidBody3D.transform.localRotation，然后根据
Configurable Joint的初始旋转startLocalRotation计算旋转偏移，最终设置Configurable Joint的目标旋转。

### Configurable Joint组件
Configurable Joint是Unity最灵活的关节组件，可以把两个Rigidbody连接起来，并控制它们的线性和平移、旋转的运动范围和响应方式。
<br><br>
细化到具体的参数设置上，首先Configurable Joint组件可以控制两种自由度：Linear Motion（平移自由度）：物体在世界坐标或父对象局部坐标上的平移自由度。Angular Motion（旋转自由度）：物体绕各轴旋转自由度。对于角色的四肢，我会锁定住它们的X/Y/Z Linear Motion，从而让四肢不会脱离躯干的相对位置，只能绕旋转自由度摆动。  
然后，将参数Rotation Drive Mode设置为Slerp，它负责控制关节以球面插值方式平滑旋转，这会让其看起来更加自然和柔和，减少僵硬感，更加适合布娃娃效果。

## 网络实现介绍
网路架构使用了Photon Fusion的典型模式：Host/Client + Client Prediction + Server Authority，类似于帧同步。
客户端通过OnInput在每个Tick上传封装好的NetworkInputData，Host在收到各个客户端的输入后进行权威计算（物理/移动/碰撞），然后把结果状态写入NetworkObject的Networked变量中，最后把这些变量按Tick广播给所有客户端，实现网络同步。

### 整体网络架构流程
1. 每个客户端玩家作为输入端，只负责上传自己的输入指令到Host。
2. 权威端Host（也可能是其中一位玩家）接收所有输入指令，在函数FixedUpdateNetwork()中计算新的物理状态（包括Rigidbody和ConfigurableJoint），同步到NetworkRigidbody3D。
3. Host广播最新的网络对象状态到各个客户端。
4. 所有客户端（包括 Host 自己）插值渲染，确保画面同步且流畅。

### Photon Fution中的一些关键概念
- 输入端(Input Authority)：产生本地玩家的原始输入数据（本游戏种为移动、跳跃、抓取等），并通过Fusion的Input系统上传。
- 权威端(State Authority)：通过Object.HasStateAuthority来判断，表示当前端拥有修改网络状态的权利，通常是服务器或Host。通俗来讲就是根据所有玩家的输入来真正修改网络对象的状态（位置、速度、动画参数等），并把结果同步给所有人。它是唯一能改写网络对象的真实状态，执行物理运算、动画同步、防作弊逻辑。

### Photon Fution的输入回调机制
- 结构体NetworkInputData实现了接口INetworkInput，里面存储了玩家角色的输入状态（移动/跳跃等）。
- 通过Photon Fusion框架提供的接口INetworkRunnerCallbacks的入口函数INetworkRunnerCallbacks.OnInput()，在每一帧获取到玩家的输入数据NetworkInputData，然后将数据上传给Host/Server。
- 具体流程可以理解为：客户端本地输入采集 -> Fusion请求输入 -> 将数据发送给Host -> Host接收并应用

### 如何实现角色四肢的物理同步？
- 根骨骼挂载的脚本中拥有NetworkRigidbody3D变量，用于在网络上同步根骨骼的位移+旋转。
- 通过NetworkArray + SyncPhysicsObject管理角色四肢刚体+关节的旋转：
