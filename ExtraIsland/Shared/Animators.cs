using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ExtraIsland.Shared;

public static class Animators {
    public class ClockTransformControlAnimator {
        public ClockTransformControlAnimator(ContentControl targetLabel, double motionMultiple = 0.8) {
            _targetLabel = targetLabel;
            DoubleAnimationUsingKeyFrames swapFadeAnimation = new DoubleAnimationUsingKeyFrames {
                KeyFrames = [
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(0.5),
                        Value = 0,
                        EasingFunction = new CubicEase {
                            EasingMode = EasingMode.EaseInOut
                        }
                    },
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(1),
                        Value = 1,
                        EasingFunction = new CubicEase {
                            EasingMode = EasingMode.EaseIn
                        }
                    }
                ],
                Duration = new Duration(TimeSpan.FromMilliseconds(250))
            };
            Storyboard.SetTarget(swapFadeAnimation, targetLabel);
            Storyboard.SetTargetProperty(swapFadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            
            DoubleAnimationUsingKeyFrames fadeAnimation = new DoubleAnimationUsingKeyFrames {
                KeyFrames = [
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(0.5),
                        Value = 0.1
                    },
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(1),
                        Value = 1
                    }
                ],
                Duration = new Duration(TimeSpan.FromMilliseconds(250))
            };
            Storyboard.SetTarget(fadeAnimation, targetLabel);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            
            DoubleAnimationUsingKeyFrames swapAnimation = new DoubleAnimationUsingKeyFrames {
                KeyFrames = [
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(0.5),
                        Value = 40.0 * motionMultiple,
                        EasingFunction = new CubicEase {
                            EasingMode = EasingMode.EaseIn
                        }
                    },
                    new DiscreteDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(0.501),
                        Value = -40.0 * motionMultiple
                    },
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(1),
                        Value = 0,
                        EasingFunction = new CubicEase {
                            EasingMode = EasingMode.EaseIn
                        }
                    }
                ],
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                FillBehavior = FillBehavior.Stop
            };
            Storyboard.SetTarget(swapAnimation,targetLabel);
            Storyboard.SetTargetProperty(swapAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            
            _swapStoryboard = new Storyboard {
                Children = [swapAnimation,swapFadeAnimation]
            };
            _swapStoryboard.Completed += (_,_) => {
                _renderLock = false;
            };
            //Timeline.SetDesiredFrameRate(_swapStoryboard, 60);
            
            _fadeStoryboard = new Storyboard {
                Children = [fadeAnimation]
            };
            _fadeStoryboard.Completed += (_,_) => {
                _renderLock = false;
            };
            //Timeline.SetDesiredFrameRate(_fadeStoryboard, 60);
        }

        readonly Storyboard _swapStoryboard;

        readonly Storyboard _fadeStoryboard;

        bool _renderLock;

        readonly ContentControl _targetLabel;
        
        string _targetContent = string.Empty;
        public string TargetContent {
            get => _targetContent;
            set => Update(value);
        }

        public void Update(string targetContent, bool isAnimated = true, bool isSwapAnimEnabled = true, bool isForced = false)
        {
            if (!(targetContent != _targetContent | isForced)) return;
            if (_renderLock) return;
            _targetContent = targetContent;
            if (!isAnimated)
            {
                _targetLabel.Content = _targetContent;
                return;
            }

            _renderLock = true;
            if (isSwapAnimEnabled)
            {
                _swapStoryboard.Begin();
            }
            else
            {
                _fadeStoryboard.Begin();
            }

            Task.Run(() =>
            {
                Thread.Sleep(110);
                _targetLabel.Dispatcher.InvokeAsync(() => { _targetLabel.Content = _targetContent; });
            });
        }

        public void SilentUpdate(string targetContent) {
            _targetContent = targetContent;
            _targetLabel.Content = _targetContent;
        }
    }
    
    public class SeparatorControlAnimator {
        public SeparatorControlAnimator(Control targetControl) {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            var easeFunc = new SineEase();
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(60)),
                EasingFunction = easeFunc
            };
            Storyboard.SetTarget(fadeOutAnimation, targetControl);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
            _fadeOutStoryboard = new Storyboard {
                Children = [fadeOutAnimation]
            };

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(60)),
                EasingFunction = easeFunc
            };
            Storyboard.SetTarget(fadeInAnimation, targetControl);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            _fadeInStoryboard = new Storyboard {
                Children = [fadeInAnimation]
            };
        }

        readonly Storyboard _fadeOutStoryboard;
        readonly Storyboard _fadeInStoryboard;
        
        public void Update(bool isInverse = false) {
            if (isInverse) {
                _fadeOutStoryboard.Begin();
            } else {
                _fadeInStoryboard.Begin();
            }
        }
    }
    
    public class EmphasizeUiElementAnimator {
        public EmphasizeUiElementAnimator(UIElement targetControl, double timeMultiple = 1) {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            var easeFunc = new SineEase();
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(60 * timeMultiple)),
                EasingFunction = easeFunc
            };
            Storyboard.SetTarget(fadeOutAnimation, targetControl);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));

            DoubleAnimation fadeOutInAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(60 * timeMultiple)),
                BeginTime = TimeSpan.FromSeconds(3),
                EasingFunction = easeFunc
            };
            Storyboard.SetTarget(fadeOutInAnimation, targetControl);
            Storyboard.SetTargetProperty(fadeOutInAnimation, new PropertyPath(UIElement.OpacityProperty));
            
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(60 * timeMultiple)),
                EasingFunction = easeFunc
            };
            Storyboard.SetTarget(fadeInAnimation, targetControl);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

            _fadeOutInStoryboard = new Storyboard {
                Children = [fadeOutAnimation,fadeOutInAnimation]
            };
            
            _fadeOutStoryboard = new Storyboard {
                Children = [fadeOutAnimation]
            };
            
            _fadeInStoryboard = new Storyboard {
                Children = [fadeInAnimation]
            };
        }

        readonly Storyboard _fadeOutInStoryboard;
        readonly Storyboard _fadeInStoryboard;
        readonly Storyboard _fadeOutStoryboard;

        public void Update(bool? stat = null) {
            switch (stat) {
                case null:
                    _fadeOutInStoryboard.Begin();
                    break;
                case true:
                    _fadeInStoryboard.Begin();
                    break;
                case false:
                    _fadeOutStoryboard.Begin();
                    break;
            }
        }
    }

    public class IslandDriftAnimator {
        public IslandDriftAnimator(Window targetWindow, Color dockBackground, double moveAmount, double timeAmount = 700) {
            targetWindow.Background = new SolidColorBrush {
                Color = Color.FromArgb(0x00,0x00,0x00,0x00)
            };
            DoubleAnimationUsingKeyFrames expandAnimation = new DoubleAnimationUsingKeyFrames {
                KeyFrames = [
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(1),
                        Value = 0,
                        EasingFunction = new SineEase {
                            EasingMode = EasingMode.EaseOut
                        }
                    }
                ],
                Duration = new Duration(TimeSpan.FromMilliseconds(timeAmount))
            };
            Storyboard.SetTarget(expandAnimation, targetWindow);
            Storyboard.SetTargetProperty(expandAnimation, new PropertyPath("Content.RenderTransform.Children[0].Y"));
            
            DoubleAnimationUsingKeyFrames collapseAnimation = new DoubleAnimationUsingKeyFrames {
                KeyFrames = [
                    new EasingDoubleKeyFrame {
                        KeyTime = KeyTime.FromPercent(1),
                        Value = -moveAmount,
                        EasingFunction = new SineEase {
                            EasingMode = EasingMode.EaseOut
                        }
                    }
                ],
                Duration = new Duration(TimeSpan.FromMilliseconds(timeAmount))
            };
            Storyboard.SetTarget(collapseAnimation, targetWindow);
            Storyboard.SetTargetProperty(collapseAnimation, new PropertyPath("Content.RenderTransform.Children[0].Y"));

            ColorAnimation colorOutAnimation = new ColorAnimation {
                From = dockBackground,
                To = Colors.Transparent,
                Duration = new Duration(TimeSpan.FromMilliseconds(timeAmount))
            };
            Storyboard.SetTarget(colorOutAnimation, targetWindow);
            Storyboard.SetTargetProperty(colorOutAnimation, new PropertyPath("Background.Color"));
            
            ColorAnimation colorInAnimation = new ColorAnimation {
                From = Colors.Transparent,
                To = dockBackground,
                Duration = new Duration(TimeSpan.FromMilliseconds(timeAmount))
            };
            Storyboard.SetTarget(colorInAnimation, targetWindow);
            Storyboard.SetTargetProperty(colorInAnimation, new PropertyPath("Background.Color"));

            _expandStoryboard = new Storyboard {
                Children = [expandAnimation]
            };
            _collapseStoryboard = new Storyboard {
                Children = [collapseAnimation]
            };
            _colorOutStoryboard = new Storyboard {
                Children = [colorOutAnimation]
            };
            _colorInStoryboard = new Storyboard {
                Children = [colorInAnimation]
            };
            _colorOutStoryboard.Completed += (_,_) => {
                if (_backgroundTarget) {
                    _colorInStoryboard.Begin();
                } else _backgroundRenderLock = false;
            };
            _colorInStoryboard.Completed += (_,_) => {
                if (!_backgroundTarget) {
                    _colorOutStoryboard.Begin();
                } else _backgroundRenderLock = false;
            };
        }

        readonly Storyboard _expandStoryboard;
        readonly Storyboard _collapseStoryboard;
        readonly Storyboard _colorOutStoryboard;
        readonly Storyboard _colorInStoryboard;

        bool _backgroundTarget;
        bool _backgroundRenderLock;
        public void Background(bool stat = false) {
            _backgroundTarget = stat;
            if (_backgroundRenderLock) return;
            _backgroundRenderLock = true;
            if (stat) _colorInStoryboard.Begin();
            else _colorOutStoryboard.Begin();
        }

        bool _driftRenderLock;
        public void Expand(bool stat = false) {
            if (_driftRenderLock) return;
            _driftRenderLock = true;
            if (stat) _expandStoryboard.Begin();
            else _collapseStoryboard.Begin();
        }
    }
}