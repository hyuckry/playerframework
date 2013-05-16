﻿using System;
using System.Windows.Input;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    internal static class TimelineTemplateParts
    {
        public const string PositionedItemsControl = "PositionedItemsControl";
        public const string DownloadProgressBarElement = "DownloadProgressBar";
        public const string ProgressSliderElement = "ProgressSlider";
    }

    /// <summary>
    /// Provides a Timeline control that can be easily bound to an InteractiveViewModel (e.g. MediaPlayer.InteractiveViewModel)
    /// </summary>
    [TemplatePart(Name = TimelineTemplateParts.DownloadProgressBarElement, Type = typeof(ProgressBar))]
    [TemplatePart(Name = TimelineTemplateParts.ProgressSliderElement, Type = typeof(SeekableSlider))]
    [TemplatePart(Name = TimelineTemplateParts.PositionedItemsControl, Type = typeof(PositionedItemsControl))]
    public sealed class Timeline : Control, IMediaPlayerControl
    {
        ProgressBar downloadProgressBarElement;

        SeekableSlider progressSliderElement;

        PositionedItemsControl positionedItemsControl;

        /// <summary>
        /// Creates a new instance of Timeline
        /// </summary>
        public Timeline()
        {
            this.DefaultStyleKey = typeof(Timeline);
            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("TimelineLabel"));
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            if (progressSliderElement != null)
            {
                // unwire existing event handlers if template was already applied
                UnwireProgressSliderEvents();
            }

            if (positionedItemsControl != null)
            {
                positionedItemsControl.ItemLoaded -= PositionedItemsControl_ItemLoaded;
                positionedItemsControl.ItemUnloaded -= PositionedItemsControl_ItemUnloaded;
            }

            positionedItemsControl = GetTemplateChild(TimelineTemplateParts.PositionedItemsControl) as PositionedItemsControl;
            downloadProgressBarElement = GetTemplateChild(TimelineTemplateParts.DownloadProgressBarElement) as ProgressBar;
            progressSliderElement = GetTemplateChild(TimelineTemplateParts.ProgressSliderElement) as SeekableSlider;
            
            if (progressSliderElement != null)
            {
                progressSliderElement.Style = SliderStyle;
                // wire up events to bubble
                WireProgressSliderEvents();
            }

            InitializeProgressSlider(ViewModel);

            if (positionedItemsControl != null)
            {
                positionedItemsControl.ItemLoaded += PositionedItemsControl_ItemLoaded;
                positionedItemsControl.ItemUnloaded += PositionedItemsControl_ItemUnloaded;
            }
        }

        private void UnwireProgressSliderEvents()
        {
            progressSliderElement.Seeked -= ProgressSliderElement_Seeked;
            progressSliderElement.ScrubbingStarted -= ProgressSliderElement_ScrubbingStarted;
            progressSliderElement.Scrubbing -= ProgressSliderElement_Scrubbing;
            progressSliderElement.ScrubbingCompleted -= ProgressSliderElement_ScrubbingCompleted;
        }

        private void WireProgressSliderEvents()
        {
            progressSliderElement.Seeked += ProgressSliderElement_Seeked;
            progressSliderElement.ScrubbingStarted += ProgressSliderElement_ScrubbingStarted;
            progressSliderElement.Scrubbing += ProgressSliderElement_Scrubbing;
            progressSliderElement.ScrubbingCompleted += ProgressSliderElement_ScrubbingCompleted;
        }

        void PositionedItemsControl_ItemUnloaded(object sender, FrameworkElementEventArgs args)
        {
            if (args.Element is ButtonBase)
            {
                ((ButtonBase)args.Element).Click -= Timeline_Click;
            }
        }

        void PositionedItemsControl_ItemLoaded(object sender, FrameworkElementEventArgs args)
        {
            if (args.Element is ButtonBase)
            {
                ((ButtonBase)args.Element).Click += Timeline_Click;
            }
        }

        void Timeline_Click(object sender, RoutedEventArgs e)
        {
            var marker = ((ButtonBase)sender).DataContext as VisualMarker;
            bool canceled = false;
            ViewModel.Seek(marker.Time, out canceled);
        }

        void ProgressSliderElement_Seeked(object sender, SeekableSliderManipulatedEventArgs e)
        {
            if (ViewModel != null)
            {
                bool canceled = false;
                ViewModel.Seek(TimeSpan.FromSeconds(e.Value), out canceled);
                e.Canceled = canceled;
            }
        }

        void ProgressSliderElement_Scrubbing(object sender, SeekableSliderManipulatedEventArgs e)
        {
            if (ViewModel != null)
            {
                var vm = ViewModel; // hold onto this in case the ViewModel changes because of this action. This way we can ensure we're calling the same one.
                bool canceled = false;
                vm.Scrub(TimeSpan.FromSeconds(e.Value), out canceled);
                if (canceled)
                {
                    progressSliderElement.CancelScrub();
                    vm.CompleteScrub(TimeSpan.FromSeconds(e.Value), canceled, out canceled);
                }
                e.Canceled = canceled;
            }
        }

        void ProgressSliderElement_ScrubbingCompleted(object sender, SeekableSliderManipulatedEventArgs e)
        {
            if (ViewModel != null)
            {
                bool canceled = false;
                ViewModel.CompleteScrub(TimeSpan.FromSeconds(e.Value), canceled, out canceled);
                e.Canceled = canceled;
            }
        }

        void ProgressSliderElement_ScrubbingStarted(object sender, SeekableSliderManipulatedEventArgs e)
        {
            if (ViewModel != null)
            {
                var vm = ViewModel; // hold onto this in case the ViewModel changes because of this action. This way we can ensure we're calling the same one.
                bool canceled = false;
                vm.StartScrub(TimeSpan.FromSeconds(e.Value), out canceled);
                if (canceled)
                {
                    progressSliderElement.CancelScrub();
                    vm.CompleteScrub(TimeSpan.FromSeconds(e.Value), canceled, out canceled);
                }
                e.Canceled = canceled;
            }
        }

        IInteractiveViewModel ViewModel
        {
            get { return MediaPlayerControl.GetViewModel(this); }
        }

        void IMediaPlayerControl.OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            InitializeProgressSlider(newValue);
        }

        private void InitializeProgressSlider(IInteractiveViewModel viewModel)
        {
            if (progressSliderElement != null)
            {
                UnwireProgressSliderEvents();

                if (viewModel != null)
                {
                    progressSliderElement.SetBinding(SeekableSlider.IsEnabledProperty, new Binding() { Path = new PropertyPath("IsScrubbingEnabled"), Source = viewModel });
                    progressSliderElement.SetBinding(SeekableSlider.ActualValueProperty, new Binding() { Path = new PropertyPath("Position.TotalSeconds"), Source = viewModel });
                    progressSliderElement.SetBinding(SeekableSlider.MinimumProperty, new Binding() { Path = new PropertyPath("StartTime.TotalSeconds"), Source = viewModel });
                    progressSliderElement.SetBinding(SeekableSlider.MaximumProperty, new Binding() { Path = new PropertyPath("EndTime.TotalSeconds"), Source = viewModel });
                    progressSliderElement.SetBinding(SeekableSlider.MaxValueProperty, new Binding() { Path = new PropertyPath("MaxPosition.TotalSeconds"), Source = viewModel });
                }
                else
                {
                    progressSliderElement.ClearValue(SeekableSlider.IsEnabledProperty);
                    progressSliderElement.ClearValue(SeekableSlider.ActualValueProperty);
                    progressSliderElement.ClearValue(SeekableSlider.MinimumProperty);
                    progressSliderElement.ClearValue(SeekableSlider.MaximumProperty);
                    progressSliderElement.ClearValue(SeekableSlider.MaxValueProperty);
                }

                WireProgressSliderEvents();
            }
        }

        /// <summary>
        /// Identifies the MediaPlayer dependency property.
        /// </summary>
        public static DependencyProperty SliderStyleProperty { get { return sliderStyleProperty; } }
        static readonly DependencyProperty sliderStyleProperty = DependencyProperty.Register("SliderStyle", typeof(Style), typeof(Timeline), new PropertyMetadata(null, (d, e) => ((Timeline)d).OnSliderStyleChanged(e.NewValue as Style)));

        void OnSliderStyleChanged(Style newValue)
        {
            if (progressSliderElement != null)
            {
                progressSliderElement.Style = newValue;
            }
        }

        /// <summary>
        /// The InteractiveMediaPlayer object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public Style SliderStyle
        {
            get { return GetValue(SliderStyleProperty) as Style; }
            set { SetValue(SliderStyleProperty, value); }
        }
    }
}
