# 🎲 Bad Luck Picker — 课堂随机抽取器

> 一款 **Apple 风格 × 原神抽卡动画** 的 WPF 桌面课堂点名工具。  
> 公平无重复随机抽取，搭配惊艳视觉动效，让课堂互动充满仪式感。

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet-framework/net472)
[![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![WPF](https://img.shields.io/badge/UI-WPF-5C2D91?logo=windows)](https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/)

---

## ✨ 功能特性

| 特性 | 说明 |
|---------|-------------|
| 🎯 **随机抽取** | 公平无重复随机算法（HashSet O(n) 无偏采样） |
| 🔢 **人数可调** | Stepper 控件支持 1~N 人，每行展示 3 个结果 |
| 🪟 **无边框窗口** | 自定义圆角窗口 + macOS 风格红绿灯标题栏 |
| ✨ **原神抽卡动画** | 深空星空叠加 → 金光扫过 → 卡牌依次弹出（缩放+光晕）→ 全屏白光炸裂收尾 |
| ⏭️ **跳过动画** | 动画期间点击任意位置即可直接跳到结果 |
| 📋 **结果动画** | 结果卡牌淡入 + 弹性上滑动画 |
| 🔔 **系统托盘** | 最小化到托盘，常驻后台 |
| ⚡ **性能优化** | 卡牌对象池复用、画刷/阴影/动画预缓存冻结、Storyboard 复用 |

---

## 🖥️ 界面预览

```
┌──────────────────────────────────┐
│  ●  ●  ●   Bad Luck Picker      │  ← macOS 红绿灯 + 可拖拽标题栏
├──────────────────────────────────┤
│                                  │
│  ┌────────────────────────────┐  │
│  │ 🎲  本次抽取              │  │
│  │                            │  │
│  │     小明   小红   小华     │  │  ← 白色卡片结果区（淡入）
│  │     小刚   小丽             │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │ 抽取人数         [−] 3 [+]│  │  ← Stepper 步进器
│  │ 共 8 名学生可选           │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │       🎰  随机抽取         │  │  ← 蓝色操作按钮
│  └────────────────────────────┘  │
│                       [艺术签名]  │
└──────────────────────────────────┘
```

---

## 🎬 抽卡动画序列

```
点击"随机抽取"
    │
    ▼
┌──────────────┐
│ 遮罩淡入      │  深蓝紫渐变底 + 80 颗闪烁星星
└──────┬───────┘
       ▼
┌──────────────┐
│ 金光扫过      │  半透明金色光束从左 → 右扫过
└──────┬───────┘
       ▼
┌──────────────┐
│ 卡牌弹出      │  Scale(0.6→1.0) + 上滑 + 淡入
│               │  发光边框 + 星级评价（★）
│               │  多人时扇形展开布局
└──────┬───────┘
       ▼
┌──────────────┐
│ 白光炸裂      │  0.5s 全屏白光闪烁 → 遮罩淡出
└──────┬───────┘
       ▼
┌──────────────┐
│ 展示结果      │  结果卡牌淡入，Stepper 恢复
└──────────────┘
```

> 💡 动画期间点击任意位置可**跳过**动画直接查看结果。

---

## 🛠️ 技术栈

| 层级 | 技术 |
|-------|------------|
| **框架** | .NET Framework 4.7.2 |
| **界面** | WPF（Windows Presentation Foundation） |
| **语言** | C# 7.3 + XAML |
| **目标平台** | Windows（x64，AnyCPU） |
| **开发工具** | Visual Studio 2017+ |

### 核心依赖

- `PresentationFramework` — WPF 核心
- `System.Windows.Forms` — NotifyIcon 系统托盘
- `System.Drawing` — 图标/图像处理

---

## 📁 项目结构

```
Bad-Luck-Picker/
├── App.xaml                    # 应用程序入口定义
├── App.xaml.cs                 # 应用程序代码逻辑
├── App.config                  # 运行时版本配置
├── MainWindow.xaml             # 主窗口界面（XAML 布局 + 样式 + 动画资源）
├── MainWindow.xaml.cs          # 主窗口逻辑（抽取算法 + 动画控制 + 性能优化）
├── Choose students.csproj      # MSBuild 项目文件
├── Choose students.sln         # Visual Studio 解决方案
├── Properties/
│   ├── AssemblyInfo.cs         # 程序集元数据
│   ├── Resources.resx          # 资源文件
│   └── Settings.settings       # 应用程序设置
├── 优化界面.ico                # 应用图标
├──     # 签名图片资源
└── .gitignore
```

---

## 🚀 快速开始

### 环境要求

- Windows 7 SP1 及以上
- [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472)
- （开发）Visual Studio 2017+，安装 `.NET 桌面开发` 工作负载

### 构建与运行

```bash
# 克隆仓库
git clone https://github.com/SereneSoulful/Bad-Luck-Picker.git
cd Bad-Luck-Picker

# 使用 MSBuild 构建
msbuild "Choose students.sln" /p:Configuration=Release /p:Platform="Any CPU"

# 直接运行
start bin\Release\"Choose students.exe"
```

### Visual Studio

1. 双击 `Choose students.sln` 打开解决方案
2. 选择 `Release | Any CPU` 配置
3. 按 `F5` 调试 / `Ctrl+Shift+B` 构建

---

## 📝 自定义配置

### 修改学生名单

编辑 `MainWindow.xaml.cs` 中的 `_students` 列表：

```csharp
private readonly List<string> _students = new List<string>
{
    "张三", "李四", "王五", "赵六",
    "孙七", "周八", "吴九", "郑十"
};
```

### 调整动画参数

| 参数/资源 | 位置 | 说明 |
|---------------------|----------|-------------|
| 星星数量 | `MainWindow.xaml.cs:GenerateStars()` | 默认 80 |
| 光束扫过速度 | `MainWindow.xaml` → `LightBeamAnim` Storyboard | 时长 `0:0:0.9` |
| 结果淡入速度 | `MainWindow.xaml` → `FadeInResult` Storyboard | 时长 `0:0:0.4` |
| 默认抽取人数 | `MainWindow.xaml.cs:_pickCount` | 默认 1 |

---

## 🎨 设计细节

### Apple 风格 UI

- **无边框窗口**：`WindowStyle="None"` + `AllowsTransparency="True"` + `CornerRadius="20"`
- **红绿灯**：红色(#FF5F57)关闭 / 黄色(#FEBC2E)最小化 / 绿色(#28C840)装饰
- **配色方案**：`#007AFF` Apple 蓝 + `#F2F2F7` 浅灰 + `#1D1D1F` 深色文字
- **Stepper**：圆形 ± 按钮，模仿 iOS Stepper 控件

### 原神抽卡动画

- **星空背景**：LinearGradientBrush 深蓝紫渐变 + 80 个 Ellipse 各自独立闪烁动画
- **金光扫过**：透明 → 金色渐变 → 透明 Rectangle，SineEase 缓动扫过
- **卡牌弹出**：BackEase 弹性缩放 + 上滑 + 淡入，DropShadowEffect 外发光
- **星级评级**：首张卡 5★（金色），其余 4★（紫色）/ 3★（蓝色）随机稀有度

### 性能优化

| 优化项 | 实现方式 |
|-------------|----------------|
| **对象池** | `Queue<Border> _cardPool` 回收卡牌 UI 元素 |
| **画刷缓存** | `Dictionary<int, SolidColorBrush> _colorBrushes` 预创建 + `Freeze()` 冻结 |
| **阴影缓存** | `Dictionary<int, DropShadowEffect> _shadowEffects` 复用，减少 GPU 开销 |
| **动画缓存** | `_fadeAnim` / `_scaleXAnim` / `_scaleYAnim` / `_moveAnim` 预创建 + `Freeze()` 冻结 |
| **Storyboard 复用** | `_fadeInStoryboard` / `_beamStoryboard` / `_flashStoryboard` 一次性事件绑定 |
| **随机算法** | `HashSet` O(n) 无偏采样，替代 `OrderBy(Random)` 的 O(n log n) |
| **定时器管理** | `List<DispatcherTimer> _activeTimers` 统一管理，`ClearAllTimers()` 防泄漏 |

---

## 🐛 已知问题

- 系统托盘图标功能（`_notifyIcon`）在 `InitNotifyIcon()` 中尚未完全实现
- 最小化后窗口可能不在任务栏显示（需在较新 Windows 版本验证）

---

## 🤝 参与贡献

欢迎提交 Issue、Feature Request 和 Pull Request！请访问 [Issues 页面](https://github.com/SereneSoulful/Bad-Luck-Picker/issues)。

### 贡献流程

1. Fork 本仓库
2. 创建功能分支（`git checkout -b feature/amazing-feature`）
3. 提交更改（`git commit -m '添加某某功能'`）
4. 推送到分支（`git push origin feature/amazing-feature`）
5. 发起 Pull Request

---

## 📄 开源协议

本项目基于 [MIT License](LICENSE) 开源 —— 详见 LICENSE 文件。

---

## 👩‍🏫 作者

**Lina** — 让课堂互动更有趣，从"谁是倒霉蛋？"开始 🎉

> 

---

*用 WPF & .NET Framework 4.7.2 倾心打造*
