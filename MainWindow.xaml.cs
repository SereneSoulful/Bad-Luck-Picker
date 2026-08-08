using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Choose_students
{
    public partial class MainWindow : Window
    {
        private readonly List<string> _students = new List<string>
        {
            "同学01","同学02","同学03","同学04","同学05","同学06","同学07","同学08",
            "同学09","同学10","同学11","同学12","同学13","同学14","同学15","同学16",
            "同学17","同学18","同学19","同学20","同学21","同学22","同学23","同学24",
            "同学25","同学26","同学27","同学28","同学29","同学30","同学31","同学32"
        };
        private readonly Random _rnd = new Random();
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private int _pickCount = 1;

        // 洗牌袋（伪随机）：一轮内每人最多被抽中一次，抽完重新洗牌
        private readonly List<int> _bag = new List<int>();
        private int _bagCursor;
        private int _lastPicked = -1;

        private List<string> _pendingResults;
        private bool _animating = false;
        private bool _skipped = false;

        private List<System.Windows.Threading.DispatcherTimer> _activeTimers =
            new List<System.Windows.Threading.DispatcherTimer>();

        // 像素风色板缓存
        private SolidColorBrush _cardFillBrush;
        private SolidColorBrush _cardBorderBrush;
        private SolidColorBrush _innerBevelBrush;
        private SolidColorBrush _inkBrush;
        private FontFamily _pixelFont;

        // 无障碍：跟随系统“关闭动画效果”
        private bool _reducedMotion;

        // 对象池
        private readonly Queue<Border> _cardPool = new Queue<Border>();

        // 快速淡入（无障碍模式）
        private readonly DoubleAnimation _quickFade;

        // Storyboard 引用
        private Storyboard _overlayIn;
        private Storyboard _overlayOut;
        private Storyboard _pulse;

        public MainWindow()
        {
            InitializeComponent();
            UpdateCountLabel();
            TotalHint.Text = $"共 {_students.Count} 人可抽";

            ApplyAccessibilitySettings();
            BuildPixelBrushes();
            _pixelFont = (FontFamily)FindResource("PixelFont");

            _quickFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            _quickFade.Freeze();

            InitNotifyIcon();

            // 缓存 Storyboard
            _overlayIn = (Storyboard)FindResource("OceanOverlayIn");
            _overlayOut = (Storyboard)FindResource("OceanOverlayOut");
            _pulse = (Storyboard)FindResource("SoftPulse");

            _overlayIn.Completed += OverlayIn_Completed;
            _overlayOut.Completed += OceanOverlayOut_Completed;
            _pulse.Completed += Pulse_Completed;

            Loaded += MainWindow_Loaded;
        }

        // ===== 无障碍 =====
        private void ApplyAccessibilitySettings()
        {
            // 跟随 Windows“关闭动画效果”设置
            _reducedMotion = !SystemParameters.ClientAreaAnimation;
        }

        private void BuildPixelBrushes()
        {
            _cardFillBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xF7, 0xE8));
            _cardFillBrush.Freeze();
            _cardBorderBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x38, 0x27));
            _cardBorderBrush.Freeze();
            _innerBevelBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xF9, 0xEC));
            _innerBevelBrush.Freeze();
            _inkBrush = new SolidColorBrush(Color.FromRgb(0x3B, 0x2E, 0x23));
            _inkBrush.Freeze();
        }

        private void OverlayIn_Completed(object sender, EventArgs e)
        {
            if (!_skipped) StartReveal(0);
        }

        private void Pulse_Completed(object sender, EventArgs e)
        {
            if (!_skipped)
            {
                var holdTimer = CreateTimer(TimeSpan.FromMilliseconds(1200), () =>
                {
                    FadeOutOverlay();
                });
                holdTimer.Start();
            }
        }

        // ===== Window loaded =====
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_reducedMotion) StartIdleAmbient();
        }

        // ===== 待机环境动画（强调条两档硬切换闪烁） =====
        private System.Windows.Threading.DispatcherTimer _ambientTimer;
        private bool _accentBlink;

        private void StartIdleAmbient()
        {
            if (_reducedMotion) return;

            AccentBar.Opacity = 1;
            _accentBlink = false;
            _ambientTimer = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(280),
                System.Windows.Threading.DispatcherPriority.Render,
                (s, e) => TickAmbient(),
                Dispatcher);
            _ambientTimer.Start();
        }

        private void TickAmbient()
        {
            _accentBlink = !_accentBlink;
            AccentBar.Opacity = _accentBlink ? 1.0 : 0.55;
        }

        private void StopIdleAmbient()
        {
            _ambientTimer?.Stop();
            AccentBar.Opacity = 1;
        }

        // Timer helper
        private System.Windows.Threading.DispatcherTimer CreateTimer(TimeSpan interval, Action callback)
        {
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = interval };
            timer.Tick += (s, _) =>
            {
                timer.Stop();
                _activeTimers.Remove(timer);
                callback?.Invoke();
            };
            _activeTimers.Add(timer);
            return timer;
        }

        private void ClearAllTimers()
        {
            foreach (var t in _activeTimers) t.Stop();
            _activeTimers.Clear();
        }

        // ===== Pseudo-random pick（洗牌袋） =====
        private void RebuildBag()
        {
            _bag.Clear();
            for (int i = 0; i < _students.Count; i++) _bag.Add(i);

            // Fisher-Yates 洗牌
            for (int i = _bag.Count - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                int tmp = _bag[i];
                _bag[i] = _bag[j];
                _bag[j] = tmp;
            }
            _bagCursor = 0;

            // 避免换轮时与上一轮最后一个人紧邻重复
            if (_lastPicked >= 0 && _bag.Count > 1 && _bag[0] == _lastPicked)
            {
                int j = _rnd.Next(1, _bag.Count);
                int tmp = _bag[0];
                _bag[0] = _bag[j];
                _bag[j] = tmp;
            }
        }

        private List<string> RandomPick(int count)
        {
            if (count >= _students.Count) return _students.ToList();

            var result = new List<string>(count);
            while (result.Count < count)
            {
                // 剩余不够一次抽取时先重新洗牌，保证单次抽取内不重复
                if (_bag.Count - _bagCursor < count - result.Count) RebuildBag();

                int idx = _bag[_bagCursor++];
                _lastPicked = idx;
                result.Add(_students[idx]);
            }
            return result;
        }

        // Object pool
        private Border GetCardFromPool()
        {
            if (_cardPool.Count > 0) return _cardPool.Dequeue();
            return new Border();
        }

        private void RecycleCard(Border card)
        {
            card.Child = null;
            card.ClearValue(Border.BackgroundProperty);
            card.ClearValue(Border.BorderBrushProperty);
            card.ClearValue(Border.EffectProperty);
            card.ClearValue(UIElement.OpacityProperty);
            card.ClearValue(UIElement.RenderTransformProperty);
            card.BeginAnimation(UIElement.OpacityProperty, null);
            card.BeginAnimation(Canvas.LeftProperty, null);
            card.BeginAnimation(Canvas.TopProperty, null);
            _cardPool.Enqueue(card);
        }

        // ===== Title bar =====
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return;
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // ===== Stepper =====
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

            // 像素风：三档硬切弹跳
            var scale = (ScaleTransform)CountLabel.RenderTransform;
            var bounce = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(240)
            };
            bounce.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            bounce.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1.18, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            bounce.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240))));
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, bounce);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
        }

        // ===== Main button =====
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_animating) return;
            if (_students.Count == 0)
            {
                SetResult("名单为空，无法抽取。");
                return;
            }

            _pendingResults = RandomPick(_pickCount);
            StartOceanAnimation();
        }

        // ===== Skip =====
        private void SkipLayer_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_animating) return;
            SkipAnimation();
        }

        // ===== PIXEL REVEAL ANIMATION =====
        private void StartOceanAnimation()
        {
            _animating = true;
            _skipped = false;
            BtnRun.IsEnabled = false;
            ClearAllTimers();
            StopIdleAmbient();

            foreach (var child in NameCanvas.Children)
                if (child is Border b) RecycleCard(b);
            NameCanvas.Children.Clear();

            SkipLayer.Visibility = Visibility.Visible;
            // 点击层位于遮罩之上（ZIndex 200），遮罩本身不拦截输入
            OceanOverlay.IsHitTestVisible = false;

            // Start breathing square
            if (!_reducedMotion)
            {
                var breathRing = (Storyboard)FindResource("BreathRingAnim");
                breathRing.Begin();
            }

            _overlayIn.Begin();
        }

        private void StartReveal(int index)
        {
            if (_skipped) return;
            if (index >= _pendingResults.Count)
            {
                PlayPulseAndEnd();
                return;
            }
            ShowOneName(index, () => StartReveal(index + 1));
        }

        private void ShowOneName(int index, Action onComplete)
        {
            if (_skipped) return;
            double canvasW = NameCanvas.ActualWidth > 0
                ? NameCanvas.ActualWidth
                : ActualWidth - 32;
            double canvasH = NameCanvas.ActualHeight > 0
                ? NameCanvas.ActualHeight
                : ActualHeight - 32;
            string name = _pendingResults[index];
            int total = _pendingResults.Count;

            // 自适应字号：人数越多，卡片越小
            double fontSize;
            if (total <= 5)       { fontSize = 46; }
            else if (total <= 10) { fontSize = 34; }
            else if (total <= 18) { fontSize = 26; }
            else                  { fontSize = 21; }

            // 第一张是居中的大主牌，后续插入的牌彼此同尺寸
            bool isLead = index == 0;
            if (isLead) fontSize += 8;

            var text = new TextBlock
            {
                Text = name,
                FontFamily = _pixelFont,
                FontSize = fontSize,
                Foreground = _inkBrush,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textW = text.DesiredSize.Width;
            double textH = text.DesiredSize.Height;

            // 直角像素卡：深色外框 + 高光内框
            double chipW = isLead
                ? Math.Min(textW + 72, canvasW - 48)
                : Math.Min(textW + 56, canvasW - 56);
            double chipH = isLead
                ? textH + 40
                : textH + 30;

            var chip = GetCardFromPool();
            chip.Width = chipW;
            chip.Height = chipH;
            chip.Background = _cardFillBrush;
            chip.BorderBrush = _cardBorderBrush;
            chip.BorderThickness = new Thickness(3);
            chip.CornerRadius = new CornerRadius(0);
            chip.Opacity = 0;
            chip.RenderTransformOrigin = new Point(0.5, 0.5);
            chip.RenderTransform = new ScaleTransform(0.8, 0.8);
            chip.Child = new Border
            {
                BorderBrush = _innerBevelBrush,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(3),
                Child = text
            };

            // 牌堆叠放：主牌居中；后续牌从斜上方交替插入，
            // 左一张右一张地错位覆盖，牌堆逐层变厚（ZIndex 递增）
            double anchorX = (canvasW - chipW) / 2.0;
            double anchorY = canvasH / 2.0 - chipH / 2.0 - 14;
            double chipX = anchorX;
            double chipY = anchorY;
            double startX = chipX;
            double startY = chipY;
            bool fromLeft = true;
            if (!isLead)
            {
                // 奇数张从左上插入，偶数张从右上插入
                fromLeft = index % 2 == 1;
                double offset = Math.Min(34, 12 + index * 3);
                double offsetY = Math.Min(26, 8 + index * 2);
                chipX = anchorX + (fromLeft ? -offset : offset);
                chipY = anchorY - offsetY;
                startX = chipX + (fromLeft ? -110 : 110);
                startY = chipY - 90;
            }
            else
            {
                // 主牌从下方三档硬切弹出
                startY = chipY + 40;
            }

            Canvas.SetLeft(chip, chipX);
            Canvas.SetZIndex(chip, index);
            NameCanvas.Children.Add(chip);

            if (_reducedMotion)
            {
                Canvas.SetTop(chip, chipY);
                chip.BeginAnimation(UIElement.OpacityProperty, _quickFade);
            }
            else
            {
                // 三档硬切步进：主牌下方弹出；副牌从对应斜上方滑入
                Canvas.SetTop(chip, startY);
                chip.BeginAnimation(UIElement.OpacityProperty, BuildFadeKeyframes());
                if (isLead)
                {
                    chip.BeginAnimation(Canvas.TopProperty, BuildMoveKeyframes(chipY));
                }
                else
                {
                    chip.BeginAnimation(Canvas.LeftProperty,
                        BuildInsertKeyframes(startX, chipX));
                    chip.BeginAnimation(Canvas.TopProperty,
                        BuildInsertKeyframes(startY, chipY));
                }

                var scale = (ScaleTransform)chip.RenderTransform;
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, BuildScaleKeyframes());
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, BuildScaleKeyframes());
            }

            double interval = total == 1 ? 520 : 360;
            var timer = CreateTimer(TimeSpan.FromMilliseconds(interval), onComplete);
            timer.Start();
        }

        // ===== 像素硬切动画辅助 =====
        private static DoubleAnimationUsingKeyFrames BuildFadeKeyframes()
        {
            var kf = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(260)
            };
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(70))));
            return kf;
        }

        private static DoubleAnimationUsingKeyFrames BuildMoveKeyframes(double targetY)
        {
            var kf = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                targetY + 40, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                targetY + 12, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                targetY + 3, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                targetY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            return kf;
        }

        private static DoubleAnimationUsingKeyFrames BuildInsertKeyframes(
            double from, double to)
        {
            var kf = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                from, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                from + (to - from) * 0.35,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(100))));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                from + (to - from) * 0.72,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                to, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            return kf;
        }

        private static DoubleAnimationUsingKeyFrames BuildScaleKeyframes()
        {
            var kf = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                0.7, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1.04, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(150))));
            kf.KeyFrames.Add(new DiscreteDoubleKeyFrame(
                1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
            return kf;
        }

        private void PlayPulseAndEnd()
        {
            if (_skipped) return;

            if (_reducedMotion)
            {
                var holdTimer = CreateTimer(TimeSpan.FromMilliseconds(500), FadeOutOverlay);
                holdTimer.Start();
                return;
            }

            PulseRing.Opacity = 0.9;
            PulseRingScale.ScaleX = 0.6;
            PulseRingScale.ScaleY = 0.6;
            _pulse.Begin();
        }

        private void FadeOutOverlay()
        {
            _overlayOut.Begin();
        }

        private void OceanOverlayOut_Completed(object sender, EventArgs e)
        {
            FinishAnimation();
        }

        private void FinishAnimation()
        {
            _animating = false;
            _skipped = false;
            OceanOverlay.Opacity = 0;
            OceanOverlay.IsHitTestVisible = false;
            SkipLayer.Visibility = Visibility.Collapsed;

            foreach (var child in NameCanvas.Children)
                if (child is Border b) RecycleCard(b);
            NameCanvas.Children.Clear();

            PulseRing.Opacity = 0;
            BreathRing.Opacity = 0;
            ((Storyboard)FindResource("BreathRingAnim")).Stop();

            var lines = new List<string>();
            for (int i = 0; i < _pendingResults.Count; i += 3)
                lines.Add(string.Join("　", _pendingResults.Skip(i).Take(3)));
            SetResult(string.Join("\n", lines));

            BtnRun.IsEnabled = true;
            StartIdleAmbient();
        }

        // ===== Skip =====
        private void SkipAnimation()
        {
            _skipped = true;
            _overlayIn.Stop();
            _overlayOut.Stop();
            _pulse.Stop();
            ((Storyboard)FindResource("BreathRingAnim")).Stop();
            ClearAllTimers();
            FinishAnimation();
        }

        // ===== Set result =====
        private void SetResult(string text)
        {
            txtBadLuck.Text = text;

            if (_reducedMotion)
            {
                ResultCard.BeginAnimation(UIElement.OpacityProperty, _quickFade);
                return;
            }

            var fadeIn = (Storyboard)FindResource("FadeInResult");
            fadeIn.Begin();
        }

        private void Window_StateChanged(object sender, EventArgs e) { }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ClearAllTimers();
            _notifyIcon?.Dispose();
        }

        private void InitNotifyIcon() { }
    }
}
