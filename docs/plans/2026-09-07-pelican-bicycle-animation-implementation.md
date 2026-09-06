# Pelican Bicycle SVG Animation Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 创建一个无需外部资源、打开即自动播放的 HTML，用内嵌 SVG 表现白色鹈鹕骑红色老式自行车穿过海边自行车道的循环 2D 动画。

**Architecture:** 单个 HTML 文件包含 SVG、内嵌 CSS 和一段只处理暂停状态的 JavaScript。SVG 按背景、地形、道路、自行车、鹈鹕和前景装饰分层；CSS keyframes 驱动车轮、脚蹬、骑行动作、围巾、翅膀和云朵，JavaScript 通过根节点 class 切换暂停/继续。

**Tech Stack:** HTML5, SVG 2D primitives/patterns/gradients, CSS keyframes, vanilla JavaScript.

---

### Task 1: 建立单文件 SVG 页面骨架

**Files:**

- Create: `pelican-bicycle.html`

**Step 1: 创建 HTML 文档与可访问性外壳**

加入标准 HTML5 文档头、响应式 viewport、页面标题、`role="img"` 的 SVG、描述性 `aria-label`，并为暂停状态准备 `aria-live` 文本和视觉按钮。

**Step 2: 加入全局样式与 SVG defs**

定义深色页面边框、响应式 SVG 容器、天空/海洋/道路渐变、碎石 pattern、滤镜和像素云所需的公共颜色变量。所有动画都提供 `prefers-reduced-motion: reduce` 覆盖规则。

**Step 3: 绘制天空和像素风格远景**

在 `background` 图层中绘制蓝色天空、暖色地平线、阶梯状方块落日和三组大小不同的矩形云朵。云朵只使用矩形组合，确保像素风格清晰可见。

**Step 4: 绘制海洋、道路和碎石人行道**

让海洋横跨中部地平线；使用梯形/多边形让道路从海洋旁的远处窄口向前景展开，并用虚线白色中心线和蓝色自行车图标标出自行车道。前景铺设带碎石纹理的暖灰人行道，再叠加少量不同大小的石子。

### Task 2: 绘制红色老式自行车与持续运动部件

**Files:**

- Modify: `pelican-bicycle.html`

**Step 1: 加入蓝色车轮与轮毂结构**

绘制前后两个蓝色外胎、浅蓝轮圈、深色轮毂和多根辐条；将每个完整车轮包装到独立的 `wheel-spin` group，避免轮胎与车架一起旋转。

**Step 2: 加入老式红色车架**

使用多段圆角线/路径绘制后轮—中轴—座管—头管的红色菱形车架，补充金属高光、旧漆斑点、挡泥板、车座、链条和黑色车把，确保车架样式明显区别于现代竞赛车。

**Step 3: 加入脚蹬与曲柄动画锚点**

在中轴处绘制黑色曲柄和两个脚蹬，把整个曲柄放在 `pedal-spin` group 中；为后续两只脚预留相反相位的连接点，使双脚始终位于脚蹬附近。

### Task 3: 绘制鹈鹕角色与服饰

**Files:**

- Modify: `pelican-bicycle.html`

**Step 1: 绘制白色鹈鹕身体、头部和嘴部**

使用暖白填充、深蓝灰描边和少量阴影绘制圆润身体、长颈、头部、眼睛与橙色长嘴；保持角色轮廓有足够对比度，避免与天空混在一起。

**Step 2: 绘制双翅并放置在黑色车把上**

绘制两只展开的白色翅膀，让翼根从身体两侧伸出、翼尖和前缘压在黑色车把附近；补充分层羽毛线。对翅膀只施加低幅 `wing-float`，表达迎风而不是飞行。

**Step 3: 加入红色圆帽和卡通红围巾**

在头部顶部绘制带帽檐的红色圆帽，在脖子处绘制结与两条飘动围巾尾端；围巾尾端使用 `scarf-wave`，但不遮挡嘴、眼睛或车把。

**Step 4: 加入腿、脚和骑行绑定**

绘制两条橙色腿与脚爪，并将脚爪端点放在曲柄两侧脚蹬处。角色身体置于 `ride-bob` group 中，双腿与脚蹬用短幅相位动画保持连接感，满足“脚放在脚蹬上不停地蹬着”。

### Task 4: 实现循环动画与暂停/继续交互

**Files:**

- Modify: `pelican-bicycle.html`

**Step 1: 定义 CSS keyframes**

加入 `wheelSpin`、`pedalSpin`、`rideBob`、`wingFloat`、`scarfWave`、`cloudDrift`、`waterShimmer` 和 `laneFlow`。车轮与曲柄使用线性无限循环，角色和装饰使用不同周期，避免所有元素机械同步。

**Step 2: 绑定 SVG 图层的 transform-origin**

为两个车轮中心、脚蹬中轴、角色身体、翅膀根部和围巾尾端设置明确的 SVG transform-origin，确保动画围绕正确锚点进行。

**Step 3: 加入暂停/继续脚本**

监听 SVG 或按钮点击，给页面根节点切换 `is-paused` class，并同步 `aria-pressed`、按钮文本和 `aria-live` 状态。脚本不引入外部依赖，不改变 SVG 几何形状。

**Step 4: 加入静态降级**

在减少动态效果模式下将动画持续时间改为极短或设为暂停，确保页面仍显示完整静态构图；无 JavaScript 时 SVG 仍可正常展示。

### Task 5: 人工核对交付物

**Files:**

- Verify: `pelican-bicycle.html`
- Verify: `docs/plans/2026-09-07-pelican-bicycle-animation-design.md`
- Verify: `docs/plans/2026-09-07-pelican-bicycle-animation-implementation.md`

**Step 1: 核对文件与关键结构**

运行 `Test-Path .\pelican-bicycle.html`，并用 `rg` 核对 `svg`、`pelican`、`wheel-spin`、`pedal-spin`、`prefers-reduced-motion`、红帽、围巾、海洋、碎石和自行车道相关标识均存在。

**Step 2: 检查 HTML/SVG 文本闭合与重复依赖**

确认页面只有一个自包含 HTML 文件、没有远程资源引用、没有新增测试或编译产物；使用编辑器或浏览器打开文件，确认画面完整、自动循环、点击暂停/继续有效。

**Step 3: 记录交付状态**

最终回复列出 HTML 文件链接、已实现的视觉和动画要求，以及人工核对范围；不声称运行了测试。

