# Tools：协议源码生成工具

[返回总览](../README.md) · [Protocol](../Protocol/README.md)

## 作用与边界

当前工具入口是 [WireCompiler/Program.cs](WireCompiler/Program.cs)，项目为
[MinimalImplementation.WireCompiler.csproj](WireCompiler/MinimalImplementation.WireCompiler.csproj)。
它把版本化包定义交给 PacketDesignCompiler，生成 Protocol 编译所需的 C# 编解码源码。
该工具在构建时运行，不是服务器每收到一个包就运行一次。

## 生成部件如何协作

| 部件 | 作用 |
| --- | --- |
| PacketAllDefinitions.CreateProtocol | 创建包定义的协议清单 |
| PacketDesignCompiler.Compile | 校验清单并产生源码 artifact 和诊断 |
| GeneratedArtifact.HintName | 指定单个输出文件名；工具要求其为不带目录的 `.g.cs` 名称 |
| Version4GeneratedWireFacts.g.cs | 写入用于标识生成输入的指纹常量 |
| version4-input.sha256 | 保存同一生成指纹的文本形式 |
| Protocol 的生成 Target | 调用工具并把生成 `.g.cs` 加入编译 |

## 构建调用顺序

[Protocol.csproj](../Protocol/MinimalImplementation.Protocol.csproj) 的
`GenerateVersion4WireCode` 位于 CoreCompile 之前，依赖 ResolveProjectReferences：

1. 先取得 WireCompiler 的构建输出。
2. 传入 `--repository-root <仓库根>` 与 `--output-directory <生成目录>`。
3. 工具初始化编译期 schema 绑定所需输入，构造清单并执行编译。
4. 检查 artifact 文件名、重名和输出路径，再以 UTF-8 无 BOM 写入源码。
5. 清理该输出目录中已经不在本次产物列表里的旧 `.g.cs`，写入指纹。
6. Protocol 项目将生成文件编译到程序集。

默认生成目录位于仓库
`Build/generated/MinimalImplementation.Protocol/<Configuration>/<TargetFramework>/`，
工具程序集位于 `Build/bin/MinimalImplementation.WireCompiler/` 下对应配置与目标框架目录。
生成目录由 csproj 计算；这里的路径说明不表示本次已执行构建。

## 编译期事实与运行时事实的区别

`InitializeCompilerOnlySchemaInputs` 只为生成器提供 schema 绑定需要的模型。
生成代码在运行时读取 ProtocolInputs；生产事实由
[Version4ProtocolFactsFactory](../Protocol/Version4ProtocolFactsFactory.cs) 和 Host bootstrap
装配。不能把生成器中的占位回调当成当前服务器真实 NPC、弹幕或模块内容表。

## 路径和修改入口

工具要求输出目录位于仓库 `Build/generated/` 内，拒绝逃逸目录、带路径的 HintName 和重复
文件名。旧文件清理仅针对明确生成目录的 `.g.cs`。因此输出目录属于可重新生成的产物范围，
不应混放手写业务代码或需要保留的资料。

修改包布局时，应从正式包定义、schema 或运行时事实入口开始，再核对 Protocol 的读取和
Host 的领域路由；单独编辑生成 `.g.cs` 会被下次生成覆盖，也不能补齐领域处理。
工具退出码 0 表示生成流程完成，2 表示参数错误，1 表示生成异常；这与服务器协议行为
验证通过属于不同结论。

需要构建或运行验证时先读 [构建与验证约束](../../../Context/约束/构建与验证约束.md)，
再按改动范围选择受影响项目。本次文档任务没有调用生成器。
