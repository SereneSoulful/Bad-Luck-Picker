# 🎲 谁是倒霉蛋？—— 课堂随机抽人工具

> 一款** Apple 风格 × 原神抽卡动画 **的 WPF 桌面点名应用，专为教师课堂互动打造。  
> 32 人名单随机抽取，不重复、动画酷炫、操作丝滑。

---

## ✨ 功能特性

| 功能 | 说明 |
|------|------|
| 🎯 **随机抽取** | 从 32 人名单中无重复随机抽取，算法公平无偏 |
| 🔢 **人数可调** | 步进器支持 1~32 人自由选择，结果每行展示 3 人 |
| 🪟 **无边框窗口** | 自定义圆角窗口 + macOS 风格交通灯（红/黄/绿）标题栏 |
| ✨ **原神抽卡动画** | 深空星空遮罩 → 金色光柱扫过 → 卡片逐个弹出（缩放 + 发光） → 全屏白闪收尾 |
| ⏭️ **一键跳过** | 动画播放中点击任意位置跳过，直接出结果 |
| 📋 **结果动画** | 结果卡片淡入 + 上移弹性动画 |
| 🔔 **系统托盘** | 关闭/最小化缩至托盘，后台常驻 |
| ⚡ **性能优化** | 对象池复用卡片、预缓存笔刷/阴影/动画模板、冻结 Freezable 资源 |

---

## 🖥️ 界面预览

```
┌──────────────────────────────────┐
│  ●  ●  ●   谁是倒霉蛋？          │  ← macOS 交通灯 + 可拖拽标题栏
├──────────────────────────────────┤
│                                  │
│  ┌────────────────────────────┐  │
│  │ 🎲  本次倒霉蛋             │  │
│  │                            │  │
│  │     张三　李四　王五       │  │  ← 白卡结果区（淡入动画）
│  │     赵六　孙七             │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │ 抽取人数         [−] 3 [+] │  │  ← 步进器
│  │ 共 32 人可抽               │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │       🎰  随机抽取         │  │  ← 蓝色大按钮
│  └────────────────────────────┘  │
│                       [签名图]   │
└──────────────────────────────────┘
```

---

## 🎬 抽卡动画流程

```
点击「随机抽取」
    │
    ▼
┌──────────────┐
│ 深空遮罩淡入  │  渐变深蓝紫色背景 + 80 颗闪烁星星
└──────┬───────┘
       ▼
┌──────────────┐
│ 金色光柱扫过  │  半透明光束从左到右划过
└──────┬───────┘
       ▼
┌──────────────┐
│ 卡片逐个弹出  │  缩放(0.6→1.0) + 上移 + 淡入
│              │  发光边框 + 星级标注(★)
│              │  多人时扇形错开排列
└──────┬───────┘
       ▼
┌──────────────┐
│ 全屏白闪收尾  │  0.5s 白闪 → 遮罩淡出
└──────┬───────┘
       ▼
┌──────────────┐
│ 显示最终结果  │  结果卡片淡入，步进器恢复正常
└──────────────┘
```

> 💡 动画过程中 **点击任意处可跳过**，直接展示结果。

---

## 🛠️ 技术栈

| 层级 | 技术 |
|------|------|
| **框架** | .NET Framework 4.7.2 |
| **UI** | WPF (Windows Presentation Foundation) |
| **语言** | C# 7.3 + XAML |
| **目标平台** | Windows (x64, AnyCPU) |
| **IDE** | Visual Studio 2017+ |

### 核心依赖

- `PresentationFramework` — WPF 核心
- `System.Windows.Forms` — NotifyIcon 系统托盘
- `System.Drawing` — 图标/图像处理

---

## 📁 项目结构

```
Choose students/
├── App.xaml                    # 应用程序入口定义
├── App.xaml.cs                 # 应用程序后台代码
├── App.config                  # 运行时版本配置
├── MainWindow.xaml             # 主窗口 UI（XAML 布局 + 样式 + 动画资源）
├── MainWindow.xaml.cs          # 主窗口逻辑（抽取算法 + 动画控制 + 性能优化）
├── Choose students.csproj      # MSBuild 项目文件
├── Choose students.sln         # Visual Studio 解决方案
├── Properties/
│   ├── AssemblyInfo.cs         # 程序集元信息
│   ├── Resources.resx          # 资源文件
│   └── Settings.settings       # 应用设置
├── 优化界面.ico                # 应用图标
├──     # 签名图片资源
└── bin/                        # 编译输出目录
    ├── Debug/
    └── Release/
```

---

## 🚀 快速开始

### 环境要求

- Windows 7 SP1 或更高版本
- [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472)
- （开发）Visual Studio 2017+ 与 `.NET 桌面开发` 工作负载

### 编译运行

```bash
# 克隆项目
git clone <仓库地址>
cd "Choose students"

# 使用 MSBuild 编译
msbuild "Choose students.sln" /p:Configuration=Release /p:Platform="Any CPU"

# 或直接运行
start bin\Release\"Choose students.exe"
```

### Visual Studio

1. 双击 `Choose students.sln` 打开解决方案
2. 选择 `Release | Any CPU` 配置
3. 按 `F5` 调试运行 / `Ctrl+Shift+B` 编译

---

## 📝 配置说明

### 修改学生名单

编辑 `MainWindow.xaml.cs` 中的 `_students` 列表：

```csharp
private readonly List<string> _students = new List<string>
{
    "学生1","学生2","学生3", // ...
};
```

### 修改抽卡动画参数

| 变量 / 资源 | 位置 | 说明 |
|-------------|------|------|
| `StarCanvas` 星星数量 | `MainWindow.xaml.cs:GenerateStars()` | 默认 80 颗 |
| `LightBeamAnim` 光柱速度 | `MainWindow.xaml` → `LightBeamAnim` Storyboard | Duration `0:0:0.9` |
| `FadeInResult` 结果淡入速度 | `MainWindow.xaml` → `FadeInResult` Storyboard | Duration `0:0:0.4` |
| `_pickCount` 默认抽取人数 | `MainWindow.xaml.cs` | 默认 1 |

---

## 🎨 设计细节

### Apple 风格 UI

- **无边框窗口**：`WindowStyle="None"` + `AllowsTransparency="True"` + `CornerRadius="20"` 实现圆角
- **交通灯按钮**：红(#FF5F57) 关闭 / 黄(#FEBC2E) 最小化 / 绿(#28C840) 装饰
- **配色方案**：`#007AFF` 苹果蓝 + `#F2F2F7` 浅灰底 + `#1D1D1F` 深色文字
- **步进器**：圆形 ± 按钮，模仿 iOS Stepper 控件

### 原神抽卡动画

- **星空背景**：LinearGradientBrush 深蓝紫渐变 + 80 个 Ellipse 带独立闪烁动画
- **光柱**：透明 → 金色渐变 → 透明的 Rectangle，SineEase 扫光
- **卡片弹出**：BackEase 弹性缩放 + 上移 + 淡入，带 DropShadowEffect 外发光
- **星级标注**：首张 5★(金)、其余按稀有度分配 4★(紫) / 3★(蓝)

### 性能优化清单

| 优化项 | 实现方式 |
|--------|----------|
| **对象池** | `Queue<Border> _cardPool` 回收复用卡片 UI 元素 |
| **笔刷缓存** | `Dictionary<int, SolidColorBrush> _colorBrushes` 预创建 + `Freeze()` |
| **阴影缓存** | `Dictionary<int, DropShadowEffect> _shadowEffects` 复用以减少 GPU 开销 |
| **动画缓存** | `_fadeAnim` / `_scaleXAnim` / `_scaleYAnim` / `_moveAnim` 预创建 + `Freeze()` |
| **Storyboard 复用** | `_fadeInStoryboard` / `_beamStoryboard` / `_flashStoryboard` 一次性绑定事件 |
| **随机算法** | `HashSet` 判重 O(n) 无偏采样替代 `OrderBy(Random)` O(n log n) |
| **定时器追踪** | `List<DispatcherTimer> _activeTimers` 统一管理，`ClearAllTimers()` 防止泄漏 |

---

## 🐛 已知问题

- 托盘图标相关功能（`_notifyIcon`）在 `InitNotifyIcon()` 中未完整实现
- 窗口最小化后需通过托盘恢复，桌面任务栏不显示（Windows 新版本兼容性需验证）

---

## 📄 License

本项目为个人教学工具，仅供学习与课堂使用。

---

## 👩‍🏫 作者

**Lina** —— 课堂互动，从"谁是倒霉蛋"开始 🎉

---

*Built with ❤️ using WPF & .NET Framework 4.7.2*
