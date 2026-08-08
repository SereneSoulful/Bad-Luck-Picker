# 🎲 Bad Luck Picker — 课堂随机抽取器

> 一款 **像素风（8-bit 纸面桌游）** 的 WPF 桌面课堂点名工具。  
> 公平无重复随机抽取，搭配硬边像素动效，让课堂互动充满仪式感。

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet-framework/net472)
[![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![WPF](https://img.shields.io/badge/UI-WPF-5C2D91?logo=windows)](https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/)

---

## ✨ 功能特性

| 特性 | 说明 |
|---------|-------------|
| 🎯 **随机抽取** | 洗牌式伪随机（Fisher-Yates 洗牌袋）：一轮内每人最多被抽中一次，抽完自动重洗，跨轮避免紧邻重复 |
| 🔢 **人数可调** | Stepper 控件支持 1~N 人，每行最多展示 2 个结果，上下行按最长姓名对齐 |
| 🪟 **无边框窗口** | 方角像素窗框 + 偏移实心硬阴影 + 像素方块标题栏按钮 |
| 🎴 **扑克牌点名动画** | 纸面遮罩 → 同尺寸牌完全重叠 → 左右斜上方交替插牌 → 底部厚度条逐层增高 → 内外双环收尾 |
| ⏭️ **跳过动画** | 动画期间点击任意位置即可直接跳到结果 |
| 📋 **结果动画** | 像素结果卡硬切淡入 + 高光内框 |
| 🔔 **系统托盘** | 最小化到托盘，常驻后台 |
| ⚡ **性能优化** | 卡牌对象池复用、画刷/阴影/动画预缓存冻结、Storyboard 复用 |

---

## 🖥️ 界面预览

```
┌──────────────────────────────────┐
│  ▣  ▣  ▣   Bad Luck Picker      │  ← 像素方块按钮 + 可拖拽标题栏
├──────────────────────────────────┤
│                                  │
│  ┌────────────────────────────┐  │
│  │ T H I S   R O U N D      │  │
│  │                            │  │
│  │     小明   小红            │  │  ← 像素卡片结果区（每行 2 个）
│  │     小刚   小丽            │  │  ← 双字名补全角空格对齐
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │ 抽取人数         [−] 3 [+]│  │  ← Stepper 步进器
│  │ 共 8 名学生可选           │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │       🎰  随机抽取         │  │  ← 暖红强调像素按钮
│  └────────────────────────────┘  │
│                          By Lina  │
└──────────────────────────────────┘
```

---

## 🎬 扑克牌点名动画序列

```
点击"随机抽取"
    │
    ▼
┌──────────────┐
│ 遮罩淡入      │  纸面像素遮罩 + 点阵纹理
└──────┬───────┘
       ▼
┌──────────────┐
│ 名字浮现      │  同尺寸牌从下方 / 左右斜上方交替插入
│               │  后一张完全盖住前一张
└──────┬───────┘
       ▼
┌──────────────┐
│ 脉冲收尾      │  内圈深绿呼吸 + 外圈暖红脉冲 → 遮罩淡出
└──────┬───────┘
       ▼
┌──────────────┐
│ 展示结果      │  结果卡牌硬切淡入，Stepper 恢复
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
├── 优化界面.ico                # 像素风应用图标
├── Fonts/
│   ├── FusionPixel12.ttf       # 嵌入的开源像素中文字体（SIL OFL 1.1）
│   └── OFL.txt                 # 字体许可证
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
| 强调条闪烁 | `MainWindow.xaml.cs` → `TickAmbient()` | 两档明暗切换，周期 280ms |
| 呼吸方框节奏 | `MainWindow.xaml` → `BreathRingAnim` Storyboard | 时长 `0:0:1.2` |
| 结果淡入速度 | `MainWindow.xaml` → `FadeInResult` Storyboard | 时长 `0:0:0.16` |
| 默认抽取人数 | `MainWindow.xaml.cs:_pickCount` | 默认 1 |

---

## 🎨 设计细节

### 像素风设计（暖色纸面 + 硬边景深）

- **纸面背景**：奶油纸色 `#F2E6CD` + 8px 点阵纹理，控件底色比背景略深（面板 `#E3D3B2`、卡片 `#FFF7E8`）
- **硬边景深**：所有圆角与模糊阴影改为直角、偏移实心硬阴影和 1–2px 明暗斜面边框
- **无边框窗口**：`WindowStyle="None"` + `AllowsTransparency="True"` + 方角窗框 + 偏移硬阴影
- **标题栏按钮**：红(#D94F30)关闭 / 琥珀(#E8A33D)最小化 / 绿(#6FA86B)装饰，均为方形像素块
- **强调色**：主按钮使用暖红 `#D94F30` + 琥珀内框，成为界面最突出的层级
- **字体**：嵌入开源 Fusion Pixel 12px 简体像素字体（SIL OFL 1.1），中文姓名保持像素观感
- **无障碍**：插牌与收尾动画始终播放；系统“关闭动画效果”时仅跳过待机闪烁等环境动画

### 扑克牌点名动画

- **遮罩**：纸面像素遮罩 + 点阵纹理，聚焦中心
- **同尺寸**：所有名字牌大小一致，后一张完全重叠覆盖前一张
- **插牌**：第一张从下方弹出，后续牌从斜上方交替插入（奇数张左上、偶数张右上），三档硬切滑入
- **厚度**：牌堆底部厚度条每落一张牌增高 3px，`Canvas.ZIndex` 递增，牌堆越堆越厚
- **收尾**：内圈深绿呼吸环与外圈暖红脉冲环成对出现，放大扩散后遮罩淡出

### 性能优化

| 优化项 | 实现方式 |
|-------------|----------------|
| **对象池** | `Queue<Border> _cardPool` 回收卡牌 UI 元素 |
| **画刷缓存** | `_cardFillBrush` / `_cardBorderBrush` / `_inkBrush` 等预创建 + `Freeze()` 冻结 |
| **动画缓存** | `_quickFade` 预创建 + `Freeze()` 冻结，卡牌动画使用离散关键帧 |
| **Storyboard 复用** | `_overlayIn` / `_overlayOut` / `_pulse` 一次性事件绑定 |
| **随机算法** | Fisher-Yates 洗牌袋 O(n) 预洗牌，抽取 O(k) 顺序取，公平且不重复 |
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
