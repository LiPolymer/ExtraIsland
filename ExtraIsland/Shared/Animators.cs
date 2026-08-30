using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace ExtraIsland.Shared;

public static class Animators {

    public class GenericContentSwapAnimator {
        public GenericContentSwapAnimator(ContentControl targetLabel, double motionMultiple = 0.5) {
            _targetLabel = targetLabel;
            _swapOutAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(TranslateTransform.YProperty, 0.0),
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(TranslateTransform.YProperty, 40.0 * motionMultiple),
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(150),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseIn()
            };
            _swapInAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(TranslateTransform.YProperty, -40.0 * motionMultiple),
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(TranslateTransform.YProperty, 0.0),
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(150),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseOut()
            };
            _fadeOutAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseIn()
            };
            _fadeInAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseOut()
            };
        }
        readonly ContentControl _targetLabel;
        readonly Animation _swapOutAnimation;
        readonly Animation _swapInAnimation;
        readonly Animation _fadeOutAnimation;
        readonly Animation _fadeInAnimation;
        string _targetContent = string.Empty;

        public string TargetContent {
            get => _targetContent;
            set => Update(value);
        }

        bool _renderLock;
        int _generation;
        long _lockStartTimestamp;
        object? _pendingContent;
        bool _pendingAnimated;
        bool _pendingSwap;
        
        const long LockTimeoutMs = 3000;

        public bool IsRendering => _renderLock;

        public void Update(string content, bool isAnimated = true, bool isSwapAnimEnabled = true, bool isForced = false) {
            if (!(content != _targetContent | isForced)) return;
            _targetContent = content;
            Update((object)content, isAnimated, isSwapAnimEnabled);
        }

        public void Update(object content, bool isAnimated = true, bool isSwapAnimEnabled = true) {
            if (_renderLock) {
                if (Environment.TickCount64 - _lockStartTimestamp <= LockTimeoutMs) {
                    _pendingContent = content;
                    _pendingAnimated = isAnimated;
                    _pendingSwap = isSwapAnimEnabled;
                    return;
                }
                // 认为动画挂起,放弃旧动画并接管锁
                _pendingContent = null;
                _renderLock = false;
            }
            _renderLock = true;
            _lockStartTimestamp = Environment.TickCount64;
            int generation = ++_generation;
            // Use InvokeAsync only if needed to avoid overhead if already on UI thread
            if (Dispatcher.UIThread.CheckAccess()) {
                RunUpdate(content, isAnimated, isSwapAnimEnabled, generation);
            } else {
                Dispatcher.UIThread.InvokeAsync(() => RunUpdate(content, isAnimated, isSwapAnimEnabled, generation));
            }
        }

        private async void RunUpdate(object content, bool isAnimated, bool isSwapAnimEnabled, int generation) {
            try {
                if (!isAnimated) {
                    _targetLabel.Content = content;
                } else if (isSwapAnimEnabled) {
                    await _swapOutAnimation.RunAsync(_targetLabel);
                    _targetLabel.Content = content;
                    await _swapInAnimation.RunAsync(_targetLabel);
                } else {
                    await _fadeOutAnimation.RunAsync(_targetLabel);
                    _targetLabel.Content = content;
                    await _fadeInAnimation.RunAsync(_targetLabel);
                }
            } catch {
                // Ignore animation errors
                if (generation == _generation) {
                    _targetLabel.Content = content;
                }
            } finally {
                // 代际不匹配说明本次动画已被放弃,不再参与锁管理,避免干扰新的更新或写回旧内容
                if (generation == _generation) {
                    _renderLock = false;
                    object? pending = _pendingContent;
                    if (pending != null) {
                        _pendingContent = null;
                        Update(pending,_pendingAnimated,_pendingSwap);
                    }
                }
            }
        }

        public void SilentUpdate(string content) {
            _targetContent = content;
            SilentUpdate((object)content);
        }

        public void SilentUpdate(object content) {
             if (Dispatcher.UIThread.CheckAccess()) {
                _targetLabel.Content = content;
            } else {
                Dispatcher.UIThread.InvokeAsync(() => {
                    _targetLabel.Content = content;
                });
            }
        }
    }

    public class SeparatorVisualAnimator {
        // ReSharper disable once ConvertToPrimaryConstructor
        public SeparatorVisualAnimator(Visual target) {
            _target = target;
            _fadeOutAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseIn()
            };
            _fadeInAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125),
                FillMode = FillMode.Forward,
                Easing = new QuadraticEaseOut()
            };
        }
        readonly Visual _target;
        readonly Animation _fadeOutAnimation;
        readonly Animation _fadeInAnimation;
        string _targetContent = string.Empty;
        
        public void Update(bool isInvisible = false) {
            Dispatcher.UIThread.InvokeAsync(() => {
                if (isInvisible) {
                    _fadeOutAnimation.RunAsync(_target);
                } else {
                    _fadeInAnimation.RunAsync(_target);
                }
            });
        }
    }

    public class EmphasizerVisualAnimator {
        // ReSharper disable once ConvertToPrimaryConstructor
        public EmphasizerVisualAnimator(Visual target, double timeMultiple = 1) {
            _target = target;
            _fadeOutAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125 * timeMultiple),
                FillMode = FillMode.Forward,
                Easing = new SineEaseIn()
            };
            _fadeInAnimation = new Animation {
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0)
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0)
                        }
                    }
                },
                Duration = TimeSpan.FromMilliseconds(125 * timeMultiple),
                FillMode = FillMode.Forward,
                Easing = new SineEaseOut()
            };
        }
        readonly Visual _target;
        readonly Animation _fadeOutAnimation;
        readonly Animation _fadeInAnimation;
        string _targetContent = string.Empty;

        public void Update(bool? stat = null) {
            Dispatcher.UIThread.InvokeAsync(async () => {
                switch (stat) {
                    case null:
                        await _fadeInAnimation.RunAsync(_target);
                        await Task.Delay(TimeSpan.FromSeconds(3));
                        await _fadeOutAnimation.RunAsync(_target);
                        break;
                    case true:
                        await _fadeOutAnimation.RunAsync(_target);
                        break;
                    case false:
                        await _fadeInAnimation.RunAsync(_target);
                        break;
                }
            });
        }
    }
}