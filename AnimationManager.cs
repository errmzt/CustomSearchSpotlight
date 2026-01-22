using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace CustomSearchSpotlight
{
    public class AnimationManager
    {
        private readonly Window _window;
        private Storyboard _searchingAnimation;
        private Storyboard _voiceAnimation;
        private Storyboard _windowAnimation;
        
        public AnimationManager(Window window)
        {
            _window = window;
            InitializeAnimations();
        }
        
        private void InitializeAnimations()
        {
            // Searching animation (pulsing gradient)
            _searchingAnimation = new Storyboard();
            
            var colorAnim1 = new ColorAnimation
            {
                From = Color.FromArgb(255, 0, 122, 255),  // #007AFF
                To = Color.FromArgb(255, 10, 132, 255),   // #0A84FF
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            
            var colorAnim2 = new ColorAnimation
            {
                From = Color.FromArgb(255, 10, 132, 255),  // #0A84FF
                To = Color.FromArgb(255, 0, 86, 214),      // #0056D6
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.25)
            };
            
            Storyboard.SetTargetName(colorAnim1, "SearchGradient1");
            Storyboard.SetTargetProperty(colorAnim1, 
                new PropertyPath("(Border.Background).(LinearGradientBrush.GradientStops)[1].(GradientStop.Color)"));
            
            Storyboard.SetTargetName(colorAnim2, "SearchGradient2");
            Storyboard.SetTargetProperty(colorAnim2, 
                new PropertyPath("(Border.Background).(LinearGradientBrush.GradientStops)[2].(GradientStop.Color)"));
            
            _searchingAnimation.Children.Add(colorAnim1);
            _searchingAnimation.Children.Add(colorAnim2);
            
            // Voice speaking animation (wave)
            _voiceAnimation = new Storyboard();
            
            for (int i = 1; i <= 3; i++)
            {
                var heightAnim = new DoubleAnimation
                {
                    From = 4,
                    To = 20,
                    Duration = TimeSpan.FromSeconds(0.6),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(i * 0.2)
                };
                
                Storyboard.SetTargetName(heightAnim, $"VoiceWave{i}");
                Storyboard.SetTargetProperty(heightAnim, 
                    new PropertyPath("(Rectangle.Height)"));
                
                _voiceAnimation.Children.Add(heightAnim);
            }
            
            // Window appear animation
            _windowAnimation = new Storyboard();
            
            var opacityAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            
            var scaleXAnim = new DoubleAnimation
            {
                From = 0.9,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1
                }
            };
            
            var scaleYAnim = new DoubleAnimation
            {
                From = 0.9,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1
                }
            };
            
            Storyboard.SetTarget(opacityAnim, _window);
            Storyboard.SetTargetProperty(opacityAnim, 
                new PropertyPath("Opacity"));
            
            Storyboard.SetTarget(scaleXAnim, _window);
            Storyboard.SetTargetProperty(scaleXAnim, 
                new PropertyPath("RenderTransform.ScaleX"));
            
            Storyboard.SetTarget(scaleYAnim, _window);
            Storyboard.SetTargetProperty(scaleYAnim, 
                new PropertyPath("RenderTransform.ScaleY"));
            
            _windowAnimation.Children.Add(opacityAnim);
            _windowAnimation.Children.Add(scaleXAnim);
            _windowAnimation.Children.Add(scaleYAnim);
        }
        
        public void StartSearchingAnimation(FrameworkElement target)
        {
            _searchingAnimation.Begin(target, true);
        }
        
        public void StopSearchingAnimation(FrameworkElement target)
        {
            _searchingAnimation.Stop(target);
        }
        
        public void StartVoiceAnimation(FrameworkElement target)
        {
            _voiceAnimation.Begin(target, true);
        }
        
        public void StopVoiceAnimation(FrameworkElement target)
        {
            _voiceAnimation.Stop(target);
        }
        
        public void PlayWindowAppearAnimation()
        {
            _windowAnimation.Begin();
        }
        
        public void PlayButtonPressAnimation(FrameworkElement button)
        {
            var pressAnim = new Storyboard();
            
            var scaleXAnim = new DoubleAnimation
            {
                To = 0.95,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            
            var scaleYAnim = new DoubleAnimation
            {
                To = 0.95,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            
            Storyboard.SetTarget(scaleXAnim, button);
            Storyboard.SetTargetProperty(scaleXAnim, 
                new PropertyPath("RenderTransform.ScaleX"));
            
            Storyboard.SetTarget(scaleYAnim, button);
            Storyboard.SetTargetProperty(scaleYAnim, 
                new PropertyPath("RenderTransform.ScaleY"));
            
            pressAnim.Children.Add(scaleXAnim);
            pressAnim.Children.Add(scaleYAnim);
            pressAnim.Begin();
        }
        
        public void PlayButtonReleaseAnimation(FrameworkElement button)
        {
            var releaseAnim = new Storyboard();
            
            var scaleXAnim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 2
                }
            };
            
            var scaleYAnim = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new ElasticEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 2
                }
            };
            
            Storyboard.SetTarget(scaleXAnim, button);
            Storyboard.SetTargetProperty(scaleXAnim, 
                new PropertyPath("RenderTransform.ScaleX"));
            
            Storyboard.SetTarget(scaleYAnim, button);
            Storyboard.SetTargetProperty(scaleYAnim, 
                new PropertyPath("RenderTransform.ScaleY"));
            
            releaseAnim.Children.Add(scaleXAnim);
            releaseAnim.Children.Add(scaleYAnim);
            releaseAnim.Begin();
        }
        
        public void PlayGlitchEffect(FrameworkElement element)
        {
            var glitchAnim = new Storyboard();
            
            var translateAnim = new DoubleAnimationUsingKeyFrames();
            translateAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
            translateAnim.KeyFrames.Add(new LinearDoubleKeyFrame(-2, TimeSpan.FromSeconds(0.05)));
            translateAnim.KeyFrames.Add(new LinearDoubleKeyFrame(2, TimeSpan.FromSeconds(0.1)));
            translateAnim.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0.15)));
            
            Storyboard.SetTarget(translateAnim, element);
            Storyboard.SetTargetProperty(translateAnim, 
                new PropertyPath("RenderTransform.TranslateX"));
            
            glitchAnim.Children.Add(translateAnim);
            glitchAnim.Begin();
        }
    }
}
