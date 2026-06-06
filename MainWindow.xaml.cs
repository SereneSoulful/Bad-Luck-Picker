using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.ComponentModel;
namespace Choose_students
{
    public partial class MainWindow : Window
    {
        // ===== 学生名单 =====
        // 演示用测试名单（发布时替换为实际学生姓名）
        private readonly List<string> _students = new List<string>
        {
            "张小萌","李小宇","王小乐","赵小文",
            "孙小艺","周小博","吴小雅","郑小奇"
        };
        private readonly Random _rnd = new Random();
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private int _pickCount = 1;
        private List<string> _pendingResults;   // 等待动画展示的结果
        private bool _animating = false;         // 是否正在播放动画
        private bool _skipped = false;           // 是否已跳过

        // 用于追踪和清理定时器，防止内存泄漏
        private List<System.Windows.Threading.DispatcherTimer> _activeTimers = new List<System.Windows.Threading.DispatcherTimer>();

        // 原神稀有度颜色（抽人时随机分配让画面丰富）
        private static readonly Color[] StarColors = {
            Color.FromRgb(0xF5, 0xD6, 0x7A),   // 金色 (5星)
            Color.FromRgb(0xAA, 0x88, 0xFF),   // 紫色 (4星)
            Color.FromRgb(0x6A, 0xC4, 0xFF),   // 蓝色 (3星)
        };

        // === 性能优化：缓存常用资源，避免重复创建 ===
        // 缓存的 Brush，避免每次 new SolidColorBrush
        private readonly Dictionary<int, SolidColorBrush> _colorBrushes = new Dictionary<int, SolidColorBrush>();
        // 缓存的 DropShadowEffect，避免每次 new Effect（GPU 资源较重）
        private readonly Dictionary<int, DropShadowEffect> _shadowEffects = new Dictionary<int, DropShadowEffect>();
        // UI 对象池：回收用过的卡片，避免频繁 GC
        private readonly Queue<Border> _cardPool = new Queue<Border>();
        // 缓存的动画参数，避免重复创建动画对象
        private readonly DoubleAnimation _fadeAnim;
        private readonly DoubleAnimation _scaleXAnim;
        private readonly DoubleAnimation _scaleYAnim;
        private readonly DoubleAnimation _moveAnim;

        // 缓存的Storyboard引用，避免每次FindResource
        private Storyboard _fadeInStoryboard;
        private Storyboard _beamStoryboard;
        private Storyboard _flashStoryboard;

        public MainWindow()
        {
            InitializeComponent();
            UpdateCountLabel();
            TotalHint.Text = $"共 {_students.Count} 人可抽";

            // === 预初始化缓存资源 ===
            // 1. 预加载颜色笔刷
            for (int i = 0; i < StarColors.Length; i++)
            {
                var c = StarColors[i];
                _colorBrushes[i] = new SolidColorBrush(c);
                _colorBrushes[i].Freeze(); // 冻结，提升渲染性能

                // 2. 预加载阴影效果（这玩意很耗 GPU，复用实例）
                var effect = new DropShadowEffect
                {
                    Color = c,
                    BlurRadius = 28,
                    ShadowDepth = 0,
                    Opacity = 0.7
                };
                effect.Freeze();
                _shadowEffects[i] = effect;
            }

            // 3. 预加载动画模板，避免每次抽卡都 new 一堆动画
            var dur = TimeSpan.FromMilliseconds(380);
            var easeOut = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 };
            var easeLinear = new CubicEase { EasingMode = EasingMode.EaseOut };

            _fadeAnim = new DoubleAnimation(0, 1, dur) { EasingFunction = easeLinear };
            _fadeAnim.Freeze();
            _scaleXAnim = new DoubleAnimation(0.6, 1.0, dur) { EasingFunction = easeOut };
            _scaleXAnim.Freeze();
            _scaleYAnim = new DoubleAnimation(0.6, 1.0, dur) { EasingFunction = easeOut };
            _scaleYAnim.Freeze();
            _moveAnim = new DoubleAnimation(40, 0, dur) { EasingFunction = easeOut };
            _moveAnim.Freeze();

            InitNotifyIcon();

            // 预加载Storyboard并绑定事件（只绑定一次！）
            _fadeInStoryboard = (Storyboard)FindResource("GenshinOverlayIn");
            _beamStoryboard = (Storyboard)FindResource("LightBeamAnim");
            _flashStoryboard = (Storyboard)FindResource("WhiteFlash");

            _fadeInStoryboard.Completed += FadeIn_Completed;
            _beamStoryboard.Completed += Beam_Completed;
            _flashStoryboard.Completed += Flash_Completed;

            Loaded += MainWindow_Loaded;
        }

        private void FadeIn_Completed(object sender, EventArgs e)
        {
            if (!_skipped) PlayLightBeamInternal();
        }

        private void Beam_Completed(object sender, EventArgs e)
        {
            if (!_skipped) StartShowingCards(0);
        }

        private void Flash_Completed(object sender, EventArgs e)
        {
            if (!_skipped)
            {
                // 停留1.2秒让用户看清结果
                var holdTimer = CreateTimer(TimeSpan.FromMilliseconds(1200), () =>
                {
                    FadeOutOverlay();
                });
                holdTimer.Start();
            }
        }

        // ===== 窗口加载后撒星星 =====
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 星星只生成一次！
            GenerateStars();
        }

        // ===== 生成背景星点 =====
        private void GenerateStars()
        {
            // 如果已经生成过了，就不要再生成了
            if (StarCanvas.Children.Count > 0) return;

            StarCanvas.Children.Clear();
            double w = ActualWidth > 0 ? ActualWidth : 480;
            double h = ActualHeight > 0 ? ActualHeight : 700;
            int count = 80;
            for (int i = 0; i < count; i++)
            {
                double size = _rnd.NextDouble() * 2.5 + 0.5;
                double opacity = _rnd.NextDouble() * 0.7 + 0.15;
                double x = _rnd.NextDouble() * w;
                double y = _rnd.NextDouble() * h;
                var star = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = Brushes.White, // 用系统缓存的笔刷
                    Opacity = opacity
                };
                Canvas.SetLeft(star, x);
                Canvas.SetTop(star, y);
                StarCanvas.Children.Add(star);
                // 给每颗星星加闪烁动画
                var anim = new DoubleAnimation
                {
                    From = opacity,
                    To = opacity * 0.2,
                    Duration = TimeSpan.FromSeconds(1.2 + _rnd.NextDouble() * 2.5),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(_rnd.NextDouble() * 3)
                };
                star.BeginAnimation(UIElement.OpacityProperty, anim);
            }
        }

        // 辅助方法：创建并追踪定时器
        private System.Windows.Threading.DispatcherTimer CreateTimer(TimeSpan interval, Action callback)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = interval
            };
            timer.Tick += (s, _) =>
            {
                timer.Stop();
                _activeTimers.Remove(timer); // 执行完移除追踪
                callback?.Invoke();
            };
            _activeTimers.Add(timer);
            return timer;
        }

        // 清理所有活动的定时器
        private void ClearAllTimers()
        {
            foreach (var timer in _activeTimers)
            {
                timer.Stop();
            }
            _activeTimers.Clear();
        }

        // === 性能优化：高效随机抽取 ===
        // 原来的 OrderBy(_ => rnd.Next()) 是 O(n log n) 排序，且有轻微偏差
        // 现在使用 O(n) 无偏采样，直接抽取不重复的索引
        private List<string> RandomPick(int count)
        {
            if (count >= _students.Count) return _students.ToList();

            var result = new List<string>(count);
            var picked = new HashSet<int>();

            while (result.Count < count)
            {
                int idx = _rnd.Next(_students.Count);
                if (picked.Add(idx)) // 保证不重复
                {
                    result.Add(_students[idx]);
                }
            }
            return result;
        }

        // === 对象池：获取卡片 ===
        private Border GetCardFromPool()
        {
            if (_cardPool.Count > 0)
            {
                // 池子里有回收的，直接复用
                return _cardPool.Dequeue();
            }
            // 没有的话才新建
            return new Border();
        }

        // === 对象池：回收卡片 ===
        private void RecycleCard(Border card)
        {
            // 重置状态，放回池子
            card.Child = null;
            card.ClearValue(Border.BackgroundProperty);
            card.ClearValue(Border.BorderBrushProperty);
            card.ClearValue(Border.EffectProperty);
            card.ClearValue(UIElement.OpacityProperty);
            card.ClearValue(UIElement.RenderTransformProperty);

            // 停止卡片上的所有动画
            card.BeginAnimation(UIElement.OpacityProperty, null);
            if (card.RenderTransform is TransformGroup tg)
            {
                if (tg.Children[0] is ScaleTransform scale)
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                }
                if (tg.Children[1] is TranslateTransform trans)
                {
                    trans.BeginAnimation(TranslateTransform.YProperty, null);
                }
            }

            _cardPool.Enqueue(card);
        }

        // ===== 标题栏拖动 =====
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return;
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Visible = false;
            // 退出时清理对象池
            _cardPool.Clear();
            Application.Current.Shutdown();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // ===== 步进器 =====
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            if (_pickCount > 1) { _pickCount--; UpdateCountLabel(); }
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (_pickCount < _students.Count) { _pickCount++; UpdateCountLabel(); }
        }

        private void UpdateCountLabel()
        {
            CountLabel.Text = _pickCount.ToString();
            BtnMinus.IsEnabled = _pickCount > 1;
            BtnPlus.IsEnabled = _pickCount < _students.Count;
        }

        // ===== 主按钮点击 =====
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_animating) return;
            if (_students.Count == 0)
            {
                SetResult("名单为空，无法抽取。");
                return;
            }

            // === 优化：使用高效随机抽取 ===
            _pendingResults = RandomPick(_pickCount);

            // 启动原神动画
            StartGenshinAnimation();
        }

        // ===== 跳过层点击 =====
        private void SkipLayer_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_animating) return;
            SkipAnimation();
        }

        // =====================================================
        //  原神抽卡动画主流程
        // =====================================================
        private void StartGenshinAnimation()
        {
            _animating = true;
            _skipped = false;
            BtnRun.IsEnabled = false;

            // 清理上一次可能残留的定时器
            ClearAllTimers();

            // 回收上一次的卡片到对象池
            foreach (var child in CardCanvas.Children)
            {
                if (child is Border b)
                {
                    RecycleCard(b);
                }
            }
            CardCanvas.Children.Clear();

            // 显示跳过层
            SkipLayer.Visibility = Visibility.Visible;
            GenshinOverlay.IsHitTestVisible = true;

            // 播放遮罩淡入
            _fadeInStoryboard.Begin();
        }

        // 1. 光柱扫过
        private void PlayLightBeamInternal()
        {
            if (_skipped) return;
            _beamStoryboard.Begin();
        }

        // 2. 逐个显示卡片
        private void StartShowingCards(int index)
        {
            if (_skipped) return;
            if (index >= _pendingResults.Count)
            {
                // 所有卡片出完 → 白闪 → 收尾
                PlayWhiteFlashAndEnd();
                return;
            }
            ShowOneCard(index, () => StartShowingCards(index + 1));
        }

        private void ShowOneCard(int index, Action onComplete)
        {
            if (_skipped) return;
            double canvasW = ActualWidth > 0 ? ActualWidth : 480;
            double canvasH = ActualHeight > 0 ? ActualHeight : 700;
            string name = _pendingResults[index];
            int colorIndex = _rnd.Next(StarColors.Length);
            Color color = StarColors[colorIndex];

            // === 从对象池拿卡片 ===
            var card = GetCardFromPool();
            card.Width = 300;
            card.Height = 160;
            card.CornerRadius = new CornerRadius(16);
            card.Opacity = 0;
            card.RenderTransformOrigin = new Point(0.5, 0.5);

            // === 使用缓存的笔刷和效果 ===
            // 渐变背景
            var grad = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            var darkColor = Color.Multiply(color, 0.25f);
            darkColor.A = 255;
            grad.GradientStops.Add(new GradientStop(Color.FromArgb(230, darkColor.R, darkColor.G, darkColor.B), 0));
            grad.GradientStops.Add(new GradientStop(Color.FromArgb(200, 15, 15, 40), 1));
            grad.Freeze();
            card.Background = grad;

            // 发光边框
            card.BorderThickness = new Thickness(1.5);
            card.BorderBrush = _colorBrushes[colorIndex]; // 缓存的笔刷

            // 外发光
            card.Effect = _shadowEffects[colorIndex]; // 缓存的Effect！

            // ---- 卡片内容 ----
            var inner = new Grid();
            // 背景纹理圆圈（装饰）
            var deco = new Ellipse
            {
                Width = 200,
                Height = 200,
                Opacity = 0.06,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, -40, 0),
                Fill = _colorBrushes[colorIndex] // 缓存的笔刷
            };
            inner.Children.Add(deco);

            // 星星图标
            int starCount = index == 0 ? 5 : (colorIndex == 1 ? 4 : 3);
            var starRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 16, 0, 0)
            };
            for (int s = 0; s < starCount; s++)
            {
                starRow.Children.Add(new TextBlock
                {
                    Text = "★",
                    FontSize = 14,
                    Foreground = _colorBrushes[colorIndex], // 缓存的笔刷
                    Margin = new Thickness(0, 0, 2, 0)
                });
            }
            inner.Children.Add(starRow);

            // 名字
            var nameBlock = new TextBlock
            {
                Text = name,
                FontFamily = new FontFamily("Microsoft YaHei, Segoe UI"),
                FontSize = 44,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White, // 系统缓存
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                // 名字的阴影，这里也可以缓存，但因为只有一个，影响不大
                Effect = new DropShadowEffect
                {
                    Color = color,
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };
            inner.Children.Add(nameBlock);

            // 底部序号
            var indexBlock = new TextBlock
            {
                Text = $"No.{index + 1}",
                FontFamily = new FontFamily("Segoe UI, Microsoft YaHei"),
                FontSize = 12,
                Foreground = Brushes.White, // 系统缓存
                Opacity = 0.55,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 16, 12)
            };
            inner.Children.Add(indexBlock);

            card.Child = inner;

            // ---- 布局位置 ----
            double cardX = (canvasW - 300) / 2.0;
            double cardY = (canvasH - 160) / 2.0 - 30;
            if (_pendingResults.Count > 1)
            {
                double spread = Math.Min(80.0, 320.0 / _pendingResults.Count);
                cardX = (canvasW - 300) / 2.0 + (index - (_pendingResults.Count - 1) / 2.0) * spread;
                cardY = canvasH / 2.0 - 80 - Math.Abs(index - (_pendingResults.Count - 1) / 2.0) * 12;
            }
            Canvas.SetLeft(card, cardX);
            Canvas.SetTop(card, cardY);

            var tg = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new ScaleTransform(0.6, 0.6),
                    new TranslateTransform(0, 40)
                }
            };
            card.RenderTransform = tg;

            CardCanvas.Children.Add(card);

            // === 使用缓存的动画 ===
            // 透明度
            card.BeginAnimation(UIElement.OpacityProperty, _fadeAnim);
            // 缩放
            var scale = (ScaleTransform)tg.Children[0];
            var trans = (TranslateTransform)tg.Children[1];
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, _scaleXAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, _scaleYAnim);
            // 上移
            trans.BeginAnimation(TranslateTransform.YProperty, _moveAnim);

            // 间隔后出下一张
            double interval = _pendingResults.Count == 1 ? 600 : 450;
            var timer = CreateTimer(TimeSpan.FromMilliseconds(interval), onComplete);
            timer.Start();
        }

        // 3. 白闪 + 结束
        private void PlayWhiteFlashAndEnd()
        {
            if (_skipped) return;
            FlashOverlay.Opacity = 0.9;
            _flashStoryboard.Begin();
        }

        private void FadeOutOverlay()
        {
            var fadeOut = (Storyboard)FindResource("GenshinOverlayOut");
            fadeOut.Begin();
        }

        // 动画结束 → 更新结果卡片
        private void GenshinOverlayOut_Completed(object sender, EventArgs e)
        {
            FinishAnimation();
        }

        private void FinishAnimation()
        {
            _animating = false;
            _skipped = false;
            GenshinOverlay.Opacity = 0;
            GenshinOverlay.IsHitTestVisible = false;
            SkipLayer.Visibility = Visibility.Collapsed;

            // 回收卡片到对象池，而不是直接丢弃
            foreach (var child in CardCanvas.Children)
            {
                if (child is Border b)
                {
                    RecycleCard(b);
                }
            }
            CardCanvas.Children.Clear();

            // 重置光柱位置
            LightBeamTranslate.X = -600;
            LightBeam.Opacity = 0;
            FlashOverlay.Opacity = 0;

            // 更新结果区
            var lines = new List<string>();
            for (int i = 0; i < _pendingResults.Count; i += 3)
                lines.Add(string.Join("　", _pendingResults.Skip(i).Take(3)));
            SetResult(string.Join("\n", lines));

            BtnRun.IsEnabled = true;
        }

        // ===== 跳过动画 =====
        private void SkipAnimation()
        {
            _skipped = true;

            // 停止所有正在播放的 Storyboard
            _fadeInStoryboard.Stop();
            _beamStoryboard.Stop();
            _flashStoryboard.Stop();
            ((Storyboard)FindResource("GenshinOverlayOut")).Stop();

            // 清理所有定时器
            ClearAllTimers();

            FinishAnimation();
        }

        // ===== 更新结果区（带淡入动画） =====
        private void SetResult(string text)
        {
            txtBadLuck.Text = text;
            var fadeIn = (Storyboard)FindResource("FadeInResult");
            fadeIn.Begin();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // 什么都不做，避免重建星星
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // 退出时清理所有定时器
            ClearAllTimers();
            _notifyIcon.Dispose();
        }

        private void InitNotifyIcon()
        {
            // 托盘图标初始化
        }
    }
}
