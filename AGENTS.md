# AGENTS 导航

本文件适用于整个目录树。开始工作先读必读入口，再在相关操作前读取对应主题文档；
涉及多个主题时读取所有相关条目，目标目录有更深层 AGENTS.md 时继续读取其局部入口。

## 开始工作

- 开始任务前，阅读 [当前上下文](Context/progress.md)。
- 所有变更，先阅读 [开发协作与变更约束](Context/项目约束/开发协作与变更约束.md)。

## Progress 记录

- 新增、改写、降级或清理 `Context/progress.md` 条目，或判断某段信息该不该进 progress 前，阅读
  [progress 信息保存约束](Context/项目约束/progress信息保存约束.md)。

## Project File Organization

- 新增、移动、拆分、合并或重命名项目、目录或文档，或调整 `src/` 生产与参考源码边界、
  `Shared/Server/Client` 端别拆分、`Test/` 与 `Build/` 落位前，阅读
  [项目文件组织设计约束](Context/架构约束/项目文件组织设计约束.md)。

## ECS File Organization

- 在 ECS 领域目录内新增、移动、拆分、合并或重命名组件、查询、系统、领域模型文件前，阅读
  [ECS 文件组织设计约束](Context/架构约束/ECS文件组织设计约束.md)。

## ECS Entity Organization

- 新增或调整实体身份、组件组合与存储、创建/销毁流程、实体查询或跨实体关系前，阅读
  [ECS Entity 组织设计约束](docs/architecture/ECSEntity组织设计约束.md)。

## ECS And Infrastructure Boundaries

- 设计 ECS 领域状态与持久化、文件或平台基础设施的职责/依赖边界时，阅读
  [ECS 领域层与基础设施六边形架构边界](Context/架构约束/ECS领域与基础设施架构边界.md)。

## Item And Inventory Design

- 新增或修改物品身份、物品堆、库存槽位、世界掉落物、物品使用配置，或调整物品载荷与
  实体态/非实体态的状态归属前，阅读
  [物品载荷与单一物品系统设计约束](docs/architecture/物品载荷与单一物品系统设计约束.md)；
  实现落位与迁移批次见
  [Item 系统设计：单域 · 双状态](docs/architecture/2026-11-25-item-single-domain-dual-state-design.md)。

## ECS System Input Boundary

- 新增系统、改变系统构造依赖，或让一个域影响另一个域的状态前，阅读
  [ECS 系统输入边界约束](Context/项目约束/ECS系统输入边界约束.md)。

## 组件命名

- 新增或重命名组件类型、字段、注册键或相关原型标识符前，阅读
  [组件命名设计约束](Context/架构约束/组件命名设计约束.md)。

## C# Code

- 新增或重构 C# 前，阅读 [C# 风格约束](Context/项目约束/Google-CSharp-Style-Guide-约束.md)。

## Side-Effect Isolation

- 修改命令式 C#、ECS 系统、后台任务或协议适配器前，阅读
  [副作用隔离规范（草案）](Context/项目约束/非函数式编码副作用隔离规范.md)

## Error Handling

- 新增失败路径、定义错误码或结果类型、在层与层之间转换失败、把错误送进协议或存档前，阅读
  [错误处理约束](Context/项目约束/错误处理约束.md)。

## Test 验证项目

- 新增、移动或重命名 `Test/` 下的验证项目，或要确定某个改动该跑哪些验证前，阅读
  [Test 验证项目组织](Test/README.md)。

## Build And Verification

- 执行构建、测试、运行、发布等可能触发编译的操作，或修改构建策略、生成输出、报告验证结果前，阅读
  [构建与验证约束](Context/项目约束/构建与验证约束.md)。

## 入口维护

- 修改本文件或目录级入口前，阅读
  [导航维护要求](Context/项目约束/开发协作与变更约束.md#导航维护)。
