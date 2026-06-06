# 🎲 Bad Luck Picker — Classroom Random Picker

> A **Apple-style × Genshin Impact gacha animation** WPF desktop classroom picker app.  
> Randomly picks from the student list with non-repeating selection, stunning animations, and smooth interaction.

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet-framework/net472)
[![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![WPF](https://img.shields.io/badge/UI-WPF-5C2D91?logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🎯 **Random Pick** | Fair non-repeating random selection (HashSet O(n) unbiased sampling) |
| 🔢 **Adjustable Count** | Stepper control supports 1~N people, 3 results per row |
| 🪟 **Borderless Window** | Custom rounded-corner window + macOS-style traffic light (red/yellow/green) title bar |
| ✨ **Genshin Gacha Anim** | Deep-space star overlay → golden light beam sweep → cards pop out one by one (scale + glow) → fullscreen white flash finale |
| ⏭️ **Skip Animation** | Click anywhere during animation to skip right to results |
| 📋 **Result Animation** | Fade-in + slide-up elastic animation for result cards |
| 🔔 **System Tray** | Minimize to tray, stay resident in background |
| ⚡ **Performance** | Object pool for card reuse, pre-cached brushes/shadows/animations, frozen Freezable resources |

---

## 🖥️ Screenshot

```
┌──────────────────────────────────┐
│  ●  ●  ●   Bad Luck Picker      │  ← macOS traffic lights + draggable title bar
├──────────────────────────────────┤
│                                  │
│  ┌────────────────────────────┐  │
│  │ 🎲  This Time's Pick       │  │
│  │                            │  │
│  │     Alice   Bob   Carol    │  │  ← White card result area (fade-in)
│  │     Dave   Eve             │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │ Pick Count        [−] 3 [+]│  │  ← Stepper
│  │ 8 students available      │  │
│  └────────────────────────────┘  │
│                                  │
│  ┌────────────────────────────┐  │
│  │       🎰  Pick Randomly    │  │  ← Blue action button
│  └────────────────────────────┘  │
│                       [signature]│
└──────────────────────────────────┘
```

---

## 🎬 Gacha Animation Sequence

```
Click "Pick Randomly"
    │
    ▼
┌──────────────┐
│ Overlay Fade  │  Deep blue-purple gradient + 80 twinkling stars
└──────┬───────┘
       ▼
┌──────────────┐
│ Light Beam    │  Semi-transparent golden beam sweeps left → right
└──────┬───────┘
       ▼
┌──────────────┐
│ Cards Pop Out │  Scale(0.6→1.0) + slide-up + fade-in
│               │  Glowing border + star rating (★)
│               │  Fan-out layout for multiple picks
└──────┬───────┘
       ▼
┌──────────────┐
│ White Flash   │  0.5s white flash → overlay fade out
└──────┬───────┘
       ▼
┌──────────────┐
│ Show Results  │  Result cards fade in, stepper restored
└──────────────┘
```

> 💡 Click anywhere during the animation to **skip** directly to results.

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| **Framework** | .NET Framework 4.7.2 |
| **UI** | WPF (Windows Presentation Foundation) |
| **Language** | C# 7.3 + XAML |
| **Target** | Windows (x64, AnyCPU) |
| **IDE** | Visual Studio 2017+ |

### Core Dependencies

- `PresentationFramework` — WPF core
- `System.Windows.Forms` — NotifyIcon system tray
- `System.Drawing` — Icon / image processing

---

## 📁 Project Structure

```
Bad-Luck-Picker/
├── App.xaml                    # Application entry definition
├── App.xaml.cs                 # Application code-behind
├── App.config                  # Runtime version config
├── MainWindow.xaml             # Main window UI (XAML layout + styles + animation resources)
├── MainWindow.xaml.cs          # Main window logic (pick algorithm + animation control + perf optimization)
├── Choose students.csproj      # MSBuild project file
├── Choose students.sln         # Visual Studio solution
├── Properties/
│   ├── AssemblyInfo.cs         # Assembly metadata
│   ├── Resources.resx          # Resource file
│   └── Settings.settings       # Application settings
├── 优化界面.ico                # App icon
├──     # Signature image resource
└── .gitignore
```

---

## 🚀 Quick Start

### Prerequisites

- Windows 7 SP1 or later
- [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472)
- (Development) Visual Studio 2017+ with `.NET Desktop Development` workload

### Build & Run

```bash
# Clone the repo
git clone https://github.com/YOUR_USERNAME/Bad-Luck-Picker.git
cd Bad-Luck-Picker

# Build with MSBuild
msbuild "Choose students.sln" /p:Configuration=Release /p:Platform="Any CPU"

# Run directly
start bin\Release\"Choose students.exe"
```

### Visual Studio

1. Double-click `Choose students.sln` to open the solution
2. Select `Release | Any CPU` configuration
3. Press `F5` to debug / `Ctrl+Shift+B` to build

---

## 📝 Configuration

### Customize Student List

Edit the `_students` list in `MainWindow.xaml.cs`:

```csharp
private readonly List<string> _students = new List<string>
{
    "Alice", "Bob", "Carol", "Dave",
    "Eve", "Frank", "Grace", "Hank"
};
```

### Tweak Animation Parameters

| Variable / Resource | Location | Description |
|---------------------|----------|-------------|
| Star count | `MainWindow.xaml.cs:GenerateStars()` | Default 80 |
| Light beam speed | `MainWindow.xaml` → `LightBeamAnim` Storyboard | Duration `0:0:0.9` |
| Result fade-in speed | `MainWindow.xaml` → `FadeInResult` Storyboard | Duration `0:0:0.4` |
| Default pick count | `MainWindow.xaml.cs:_pickCount` | Default 1 |

---

## 🎨 Design Details

### Apple-style UI

- **Borderless window**: `WindowStyle="None"` + `AllowsTransparency="True"` + `CornerRadius="20"`
- **Traffic lights**: Red (#FF5F57) close / Yellow (#FEBC2E) minimize / Green (#28C840) decorative
- **Color scheme**: `#007AFF` Apple Blue + `#F2F2F7` light gray + `#1D1D1F` dark text
- **Stepper**: Circular ± buttons mimicking iOS Stepper control

### Genshin Gacha Animation

- **Star background**: LinearGradientBrush deep blue-purple + 80 Ellipses with independent twinkle anims
- **Light beam**: Transparent → gold gradient → transparent Rectangle, SineEase sweep
- **Card pop-out**: BackEase elastic scale + slide-up + fade-in, DropShadowEffect outer glow
- **Star ratings**: First card gets 5★ (gold), others get 4★ (purple) / 3★ (blue) random rarity

### Performance Optimizations

| Optimization | Implementation |
|-------------|----------------|
| **Object Pool** | `Queue<Border> _cardPool` recycles card UI elements |
| **Brush Cache** | `Dictionary<int, SolidColorBrush> _colorBrushes` pre-created + `Freeze()` |
| **Shadow Cache** | `Dictionary<int, DropShadowEffect> _shadowEffects` reuse reduces GPU overhead |
| **Animation Cache** | `_fadeAnim` / `_scaleXAnim` / `_scaleYAnim` / `_moveAnim` pre-created + `Freeze()` |
| **Storyboard Reuse** | `_fadeInStoryboard` / `_beamStoryboard` / `_flashStoryboard` one-time event binding |
| **Random Algorithm** | `HashSet` O(n) unbiased sampling replacing `OrderBy(Random)` O(n log n) |
| **Timer Tracking** | `List<DispatcherTimer> _activeTimers` unified management, `ClearAllTimers()` prevents leaks |

---

## 🐛 Known Issues

- System tray icon functionality (`_notifyIcon`) not fully implemented in `InitNotifyIcon()`
- Window may not appear in taskbar after minimizing (needs verification on newer Windows versions)

---

## 🤝 Contributing

Contributions, issues and feature requests are welcome! Feel free to check the [issues page](https://github.com/YOUR_USERNAME/Bad-Luck-Picker/issues).

### How to Contribute

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the [MIT License](LICENSE) — see the LICENSE file for details.

---

## 👩‍🏫 Author

**Lina** — Making classroom interaction fun, starting with "Who's the unlucky one?" 🎉

> Qingdao, Shandong, China

---

*Built with ❤️ using WPF & .NET Framework 4.7.2*
