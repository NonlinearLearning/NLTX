# AGENTS 导航

本文件适用于整个目录树。开始工作先读必读入口，再在相关操作前读取对应主题文档；
涉及多个主题时读取所有相关条目，目标目录有更深层 AGENTS.md 时继续读取其局部入口。

## 开始工作

- 开始任务前，阅读 [当前上下文](Context/progress.md)。
- 所有变更，先阅读 [开发协作与变更约束](Context/约束/开发协作与变更约束.md)。

## ECS File Organization

- 新增、移动、拆分、合并或重命名 ECS、领域模型、测试及相关文件前，阅读
  [ECS 文件组织设计约束](Context/架构设计/ECS文件组织设计约束.md)。

## 组件命名

- 新增或重命名组件类型、字段、注册键或相关原型标识符前，阅读
  [组件命名设计约束](Context/架构设计/组件命名设计约束.md)。

## C# Code

- 新增或重构 C# 前，阅读 [C# 风格约束](Context/约束/Google-CSharp-Style-Guide-约束.md)。

## Side-Effect Isolation

- 修改命令式 C#、ECS 系统、后台任务或协议适配器前，阅读
  [副作用隔离规范（草案）](Context/约束/非函数式编码副作用隔离规范.md)。

## Dotnet Build Concurrency Contract

- 执行构建、测试、运行、发布等可能触发编译的操作前，阅读
  [构建与验证约束](Context/约束/构建与验证约束.md)。

## Outputs And Verification

- 修改构建策略、生成输出或报告验证结果前，阅读
  [构建输出与验收要求](Context/约束/构建与验证约束.md#outputs-and-verification)。

## 入口维护

- 修改本文件或目录级入口前，阅读
  [导航维护要求](Context/约束/开发协作与变更约束.md#导航维护)。
