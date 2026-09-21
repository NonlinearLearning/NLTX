# Google C# Style Guide 约束清单（项目内落地版）

> 来源：<https://google.github.io/styleguide/csharp-style.html>
>
> 说明：本文档为**面向本仓库执行的中文约束化整理 / 意译总结**，用于代码实现与评审，不是原文逐字拷贝。
> 官方页面同时引用 Microsoft C# 命名指南和 CoreFX C# 编码指南；下文在不改变官方
> 意图的前提下，补充了本仓库的分层与增量改动要求。

## 1. 适用范围

- 本仓库新增代码、重构代码、批量格式调整代码，默认遵循本约束。
- 若与仓库既有强制架构约束冲突，优先满足架构、安全、分层、测试约束；风格层面再尽量贴齐 Google C# Style Guide。
- 做增量修改时，优先保证**改动块内部**符合规范；不要为了追求全量格式统一而大面积污染无关 diff。

## 2. 命名规则

### 2.1 代码元素命名

- 类、方法、枚举、公共字段、公共属性、命名空间：`PascalCase`
- 局部变量、方法参数：`camelCase`
- `private` / `protected` / `internal` / `protected internal` 字段与属性：`_camelCase`
- `const` / `static` / `readonly` 等修饰符**不改变命名规则**
- 缩写按单词处理，例如：`MyRpc`，不要写成 `MyRPC`
- 接口名称以 `I` 开头，例如：`IWorkspaceRepository`

### 2.2 文件与目录命名

- 文件名与目录名使用 `PascalCase`
- 一个核心公开类型对应一个同名 PascalCase 文件。

## 3. 组织规则

### 3.1 修饰符顺序

修饰符顺序统一为：

```csharp
public protected internal private new abstract virtual override sealed static readonly extern unsafe volatile async
```

### 3.2 using 与 namespace

- `using` 放在文件顶部，位于任何 `namespace` 之前
- `using` 按字母序排列
- `System` 相关 `using` 永远优先于其他命名空间
- 一般不要为了缩短长类型名而滥用 `using` alias
- 要意识到 `using alias` 仅文件级可见，可复用价值有限

### 3.3 类成员顺序

类成员按以下大组顺序排列：

1. 嵌套类 / 枚举 / 委托 / 事件
2. `static` / `const` / `readonly` 字段
3. 字段与属性
4. 构造函数与析构函数
5. 方法

每组内的可见性顺序：

1. `public`
2. `internal`
3. `protected internal`
4. `protected`
5. `private`

补充要求：

- 同一接口实现尽量聚在一起
- 保持同类成员相邻，避免在类里“跳来跳去”

## 4. 空白与格式

### 4.1 基础规则

- 一行最多一个语句
- 一个语句里最多一个赋值动作
- 缩进使用 **2 个空格**，禁止 tab
- 列宽上限：**100**
- 左大括号 `{` 不单独换行
- `}` 和 `else` 之间不换行
- 即使语法允许省略，也**始终保留 braces**

### 4.2 空格规则

- `if` / `for` / `while` 等关键字后保留一个空格
- 逗号后保留一个空格
- 左括号后、右括号前不留空格
- 一元运算符与操作数之间不加空格
- 其他运算符与两侧操作数之间留一个空格

### 4.3 换行与对齐

- 普通续行默认缩进 4 个空格。该规则源自 Google C++ 风格，并针对 Microsoft C# 格式化工具做了适配。
- 带 braces 的续行块（对象初始化器、集合初始化器、lambda 等）按块体规则缩进，不额外算续行层级
- 方法定义或调用一行放不下时：
  - 优先将后续参数与第一个参数对齐；
  - 如果这种对齐不清晰或放不下，则所有参数改为新行 + 4 空格缩进。
- 闭包右括号与包含左括号的那一行首字符对齐
- 空块在非常短小且可读时可以写成单行

## 5. 常量、字段与字段初始化器

- 能写 `const` 的必须写 `const`
- 不能 `const` 时，优先考虑 `readonly`
- 优先使用具名常量，不要散落 magic numbers
- 字段初始化器通常是鼓励的；它是独立于 `const` / `readonly` 选择的初始化方式。

## 6. 集合接口选择

### 6.1 输入参数

- 输入参数优先使用**最严格**且能表达意图的只读集合类型：
  - `IReadOnlyCollection<T>`
  - `IReadOnlyList<T>`
  - `IEnumerable<T>`
- 如果调用方不应修改输入，签名上就不要暴露可变集合

### 6.2 返回值

- 如果要把容器所有权交给调用方，可以优先 `IList<T>`
- 如果不转移所有权，返回更严格、更只读的接口

## 7. 生成器与容器

- 生成器写法不一定更易读；能直接构造容器时不要机械改成生成器
- 若结果会被懒处理（例如不需要全部结果），生成器可能更高效
- 生成器结果马上 `ToList()`，通常不如直接填充容器高效
- 需要多次枚举时，容器通常比重复运行生成器更快；生成器每次调用都会重新执行其逻辑

## 8. 属性与表达式体

### 8.1 属性风格

- 单行只读属性，优先使用表达式体属性：`=>`
- 其他属性继续使用传统 `get; set;` 语法

### 8.2 表达式体使用边界

- 可以审慎用于：
  - 简单 lambda
  - 简单只读属性
- 不要对方法定义广泛使用表达式体
- 对于有块体的表达式 / lambda，闭合位置按普通块规则对齐

## 9. class 与 struct 选择

- 默认几乎总是优先 `class`
- 仅当类型具有典型值语义时再考虑 `struct`，例如：
  - 很小
  - 生命周期短
  - 常被嵌入其他对象
- 需要牢记 struct 的值拷贝语义，不要误以为修改返回 struct 的成员会改到原对象
- 特定团队或性能场景可以有例外，但必须明确理由

## 10. lambda 与命名方法

- 如果 lambda：
  - 不止一两条核心语句
  - 在多个地方复用
  - 阅读负担明显上升
  就应改为命名方法

## 11. 扩展方法

仅在下列条件同时满足时才使用扩展方法：

- 原类型源码不可改，或修改成本不现实
- 新增行为属于该类型的“核心通用能力”
- 扩展方法会被放在全局可用的核心库中，而不是只在局部子模块可见

额外约束：

- 如果你能改原类，而维护者也允许，优先改原类
- 扩展方法天然会弱化可读性，因此宁可少用

## 12. ref / out

- 仅输出不用作输入的值，使用 `out`
- `out` 参数放在其他参数后面
- `ref` 只在“必须修改输入本身”时少量使用
- 不要把 `ref` 当成传 struct 的性能优化捷径
- 不要用 `ref` 传一个可变容器，除非你需要把整个容器实例替换掉

## 13. LINQ

- 优先单行、短链、可读的 LINQ
- 当命令式写法更清晰时，优先命令式写法
- 避免又长又重的 LINQ 链和命令式代码混杂
- 优先使用扩展方法风格，而不是 SQL 查询语法风格
- 对于超过单语句的逻辑，不要使用 `Container.ForEach(...)`

## 14. Array vs List

- 对外字段 / 属性 / 返回值一般优先 `List<T>`，再结合只读接口约束做收口
- 容量会变化时，优先 `List<T>`
- 容量固定且构造时已知时，可优先数组
- 多维数组优先数组而不是 `List<T>` 嵌套模拟
- 数组和 `List<T>` 都是线性、连续的容器；数组容量固定，`List<T>` 可以继续添加元素。
- 某些场景数组性能更好，但通常 `List<T>` 更灵活。

## 15. 文件夹与文件位置

- 与项目既有结构保持一致
- 能扁平就尽量扁平，不要过度嵌套

## 16. Tuple 返回值

- 一般优先具名类型，而不是 `Tuple<>`
- 尤其是复杂返回结果，应建清晰命名的类/结构体

## 17. 字符串插值、`String.Format`、`String.Concat` 与 `operator+`

- 默认优先选择**最易读**的方式，尤其在日志和断言信息中；不要求为风格而机械替换为某一种 API。
- 连续 `operator+` 拼接可能更慢并造成明显的内存抖动。
- 关注性能时，多段字符串拼接优先考虑 `StringBuilder`。

## 18. 对象初始化器

- 对“Plain Old Data”类型可使用对象初始化器
- 对带构造函数的类或 `struct`，避免使用对象初始化器，以免绕过构造语义
- 多行初始化器缩进一个块级

## 19. 命名空间

- 通常不要超过 2 层深度
- 不强制文件夹布局与命名空间一一对应
- 共享库 / 模块代码使用命名空间
- 叶子应用代码（例如 Unity 应用）可以不使用命名空间
- 新顶层命名空间名称必须全局可识别、避免冲突

## 20. struct 默认值 / null 返回

- 对 struct 返回失败场景，优先：`bool success + out value`
- 若性能不是问题且可读性收益明显（例如可空条件运算符链比深层嵌套 `if` 更清晰），可使用 nullable struct
- 但要意识到 nullable struct 会强化“`null` 表示失败”的模式；Google 风格总体不鼓励把 `null` 当作通用失败语义

## 21. 遍历时删除容器元素

- 仅按条件删除时，优先 `RemoveAll(...)`
- 若删除同时还要做其他复杂处理，可采用：
  - 新建容器
  - 遍历原容器把保留项放进去
  - 最后交换容器引用

## 22. 调用委托

- 调用委托使用 `Invoke()`
- 优先空条件调用：`someDelegate?.Invoke()`
- 这样更清晰，也更能规避竞态下的空引用问题

## 23. `var` 关键字使用规则

### 23.1 鼓励使用 var 的场景

- 类型非常明显：`var apple = new Apple();`
- 工厂返回泛型类型且右侧已充分表达类型，例如：`var request = Factory.Create<HttpRequest>();`
- 只作为临时中转并马上交给其他方法处理的变量

### 23.2 不鼓励使用 var 的场景

- 基础类型：如 `bool`、`int` 等明显但语义重要时
- 编译器推断的内置数值类型，容易误判精度/类型，例如：`var number = 12 * ReturnsFloat();`
- 调用方明显需要一眼知道真实类型的变量，例如：`var listOfItems = GetList();`

## 24. Attributes

- 特性放在其所修饰字段、属性或方法的上一行；特性与成员之间保留换行
- 多个特性各占一行，便于搜索、增删、评审

## 25. 参数可读性

当方法参数含义不明显时，优先做这些改进：

- 对重复使用的字面量提炼具名常量
- 用 `enum` 替代语义不清晰的 `bool`
- 将复杂表达式拆成具名变量
- 适度使用 named arguments 提高调用点可读性
- 当配置项过多时，定义一个 options 类 / struct 统一传参

## 26. 本仓库落地补充规则

结合本仓库现有 DDD 与分层约束，额外要求如下：

- Domain / Logic / Application / Infrastructure 的分层边界优先于风格偏好
- 新增方法签名优先选择更严格的输入类型，例如 `IReadOnlyCollection<T>`
- 新增代码中保留 braces，不写单行裸 `if`
- 新增 `using` 保持 `System` 优先 + 字母序
- 新增私有字段统一 `_camelCase`
- 不为追求风格一致而重命名已稳定公开 API，除非任务本身要求
- 重构时优先缩小 diff：先保证当前改动块符合规范，再考虑周边文件整理

## 27. 执行清单（提交前）

- [ ] 命名是否符合 `PascalCase` / `camelCase` / `_camelCase`
- [ ] `using` 是否在顶部且 `System` 优先
- [ ] braces 是否完整保留
- [ ] 缩进是否统一，避免 tab
- [ ] 公共 API 是否选择了合适的集合接口
- [ ] 长参数列表是否需要 options 对象或 named arguments
- [ ] 复杂 lambda 是否该提取为命名方法
- [ ] 能 `const` / `readonly` 的是否已收紧
- [ ] diff 是否只覆盖本次任务必要范围

## 28. 参考链接

- Google C# Style Guide: <https://google.github.io/styleguide/csharp-style.html>
- Microsoft C# naming guidelines: <https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names>
- CoreFX / .NET runtime coding guidelines（Google 页内提及）: <https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md>

## 29. 公共拆分入口

涉及模块拆分、ECS 组件/系统/查询、适配器或投影设计时，必须同时遵循：

- [公共拆分约束](./公共拆分约束.md)

