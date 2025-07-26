using System.Numerics;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ExtraIsland.Shared;

public static class Animators {
    public class GenericContentSwapAnimator {
        // ReSharper disable once ConvertToPrimaryConstructor
        public GenericContentSwapAnimator(ContentControl target, double motionMultiple = 0.5) {
            _target = target;
            if (_target.IsAttachedToVisualTree()) {
                InitializeComposition();
            } else {
                _target.AttachedToVisualTree += (_, _) => InitializeComposition();
            }

            void InitializeComposition() {
                _visual = ElementComposition.GetElementVisual(target)!;
                Compositor compositor = _visual.Compositor;
                
                _swapAnimationGroup = compositor.CreateAnimationGroup();
                
                Vector3DKeyFrameAnimation swapOutAnimation = compositor.CreateVector3DKeyFrameAnimation();
                swapOutAnimation.Target = "Offset";
                swapOutAnimation.InsertKeyFrame(0.0f, _visual.Offset with { Y = 0 } , new QuadraticEaseIn());
                swapOutAnimation.InsertKeyFrame(1.0f, _visual.Offset with { Y = 40 * motionMultiple } , new QuadraticEaseIn());
                swapOutAnimation.Duration = TimeSpan.FromMilliseconds(125);
                _swapAnimationGroup.Add(swapOutAnimation);

                Vector3DKeyFrameAnimation swapInAnimation = compositor.CreateVector3DKeyFrameAnimation();
                swapInAnimation.Target = "Offset";
                swapInAnimation.InsertKeyFrame(0.0f, _visual.Offset with { Y = -40 * motionMultiple } , new QuadraticEaseIn());
                swapInAnimation.InsertKeyFrame(1.0f, _visual.Offset with { Y = 0 }, new QuadraticEaseIn());
                swapInAnimation.DelayTime = TimeSpan.FromMilliseconds(125);
                swapInAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueAfterDelay;
                swapInAnimation.Duration = TimeSpan.FromMilliseconds(125);
                _swapAnimationGroup.Add(swapInAnimation);

                _fadeAnimationGroup = compositor.CreateAnimationGroup();
                
                ScalarKeyFrameAnimation fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
                fadeOutAnimation.Target = "Opacity";
                fadeOutAnimation.InsertKeyFrame(0.0f, 1.0f, new QuadraticEaseIn());
                fadeOutAnimation.InsertKeyFrame(1.0f, 0.0f, new QuadraticEaseIn());
                fadeOutAnimation.Duration = TimeSpan.FromMilliseconds(125);
                _fadeAnimationGroup.Add(fadeOutAnimation);
            
                ScalarKeyFrameAnimation fadeInAnimation = compositor.CreateScalarKeyFrameAnimation();
                fadeInAnimation.Target = "Opacity";
                fadeInAnimation.InsertKeyFrame(0.0f, 0.0f, new QuadraticEaseIn());
                fadeInAnimation.InsertKeyFrame(1.0f, 1.0f, new QuadraticEaseIn());
                fadeInAnimation.DelayTime = TimeSpan.FromMilliseconds(125);
                fadeInAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueAfterDelay;
                fadeInAnimation.Duration = TimeSpan.FromMilliseconds(125);
                _fadeAnimationGroup.Add(fadeInAnimation);

                _isInitialized = true;
            }
        }
        readonly ContentControl _target;
        CompositionVisual _visual;

        CompositionAnimationGroup _swapAnimationGroup;
        CompositionAnimationGroup _fadeAnimationGroup;
        
        string _targetContent = string.Empty;
        bool _isInitialized;
        
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
            if (!_isInitialized) return;
            if (_renderLock) return;
            _renderLock = true;
            // TODO: 待修复:只播放后半段
            Dispatcher.UIThread.InvokeAsync(async () => {
                if (!isAnimated) {
                } else if (isSwapAnimEnabled) {
                    _visual.StartAnimationGroup(_swapAnimationGroup);
                    await Task.Delay(TimeSpan.FromMilliseconds(125));

                } else {
                    _visual.StartAnimationGroup(_fadeAnimationGroup);
                    await Task.Delay(TimeSpan.FromMilliseconds(125));
                }
                _target.Content = content;
                _renderLock = false;
                return Task.CompletedTask;
            });
        }

        public void SilentUpdate(string content) {
            _targetContent = content;
            SilentUpdate((object)content);
        }
        
        public void SilentUpdate(object content) {
            Dispatcher.UIThread.InvokeAsync(() => {
                _target.Content = content;
            });
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