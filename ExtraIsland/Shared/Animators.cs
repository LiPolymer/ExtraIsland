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
        // ReSharper disable once ConvertToPrimaryConstructor
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
                Duration = TimeSpan.FromMilliseconds(125),
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
                Duration = TimeSpan.FromMilliseconds(125),
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
        public void Update(string content, bool isAnimated = true, bool isSwapAnimEnabled = true, bool isForced = false) {
            if (_renderLock) return;
            if (!(content != _targetContent | isForced)) return;
            _targetContent = content;
            Update((object)content, isAnimated, isSwapAnimEnabled);
        }

        public void Update(object content, bool isAnimated = true, bool isSwapAnimEnabled = true) {
            if (_renderLock) return;
            _renderLock = true;
            Dispatcher.UIThread.InvokeAsync(async () => {
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
                _renderLock = false;
            });
        }

        public void SilentUpdate(string content) {
            _targetContent = content;
            SilentUpdate((object)content);
        }
        
        public void SilentUpdate(object content) {
            Dispatcher.UIThread.InvokeAsync(() => {
                _targetLabel.Content = content;
            });
        }
    }

    public class SeparatorControlAnimator {
        
    }
}