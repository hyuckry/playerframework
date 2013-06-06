﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A MediaPlayer control panel to allow user control over audio or video.
    /// </summary>
    public sealed partial class ControlPanel : Control
    {
        /// <summary>
        /// Instantiates a new instance of the ControlPanel class.
        /// </summary>
        public ControlPanel()
        {
            this.DefaultStyleKey = typeof(ControlPanel);
        }

        bool IsTemplateApplied;

        /// <inheritdoc /> 
#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            UninitializeTemplateChildren();
            base.OnApplyTemplate();
            GetTemplateChildren();
            InitializeTemplateChildren();
            SetDefaultVisualStates();
            IsTemplateApplied = true;
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(ControlPanel), new PropertyMetadata(null, (s, d) => ((ControlPanel)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            if (oldValue != null)
            {
                UninitializeViewModel(oldValue);
            }
            if (newValue != null)
            {
                if (IsTemplateApplied)
                {
                    InitializeViewModel(newValue);
                }
            }
        }
        
        /// <summary>
        /// Identifies the IsGoLiveButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsGoLiveButtonVisibleProperty { get { return isGoLiveButtonVisibleProperty; } }
        static readonly DependencyProperty isGoLiveButtonVisibleProperty = DependencyProperty.Register("IsGoLiveButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the GoLiveButton is visible or not.
        /// </summary>
        public bool IsGoLiveButtonVisible
        {
            get { return (bool)GetValue(IsGoLiveButtonVisibleProperty); }
            set { SetValue(IsGoLiveButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsAudioSelectionButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsAudioSelectionButtonVisibleProperty { get { return isAudioSelectionButtonVisibleProperty; } }
        static readonly DependencyProperty isAudioSelectionButtonVisibleProperty = DependencyProperty.Register("IsAudioSelectionButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the AudioSelectionButton is visible or not.
        /// </summary>
        public bool IsAudioSelectionButtonVisible
        {
            get { return (bool)GetValue(IsAudioSelectionButtonVisibleProperty); }
            set { SetValue(IsAudioSelectionButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsCaptionSelectionButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsCaptionSelectionButtonVisibleProperty { get { return isCaptionSelectionButtonVisibleProperty; } }
        static readonly DependencyProperty isCaptionSelectionButtonVisibleProperty = DependencyProperty.Register("IsCaptionSelectionButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the CaptionSelectionButton is visible or not.
        /// </summary>
        public bool IsCaptionSelectionButtonVisible
        {
            get { return (bool)GetValue(IsCaptionSelectionButtonVisibleProperty); }
            set { SetValue(IsCaptionSelectionButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsTimeElapsedButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimeElapsedButtonVisibleProperty { get { return isTimeElapsedButtonVisibleProperty; } }
        static readonly DependencyProperty isTimeElapsedButtonVisibleProperty = DependencyProperty.Register("IsTimeElapsedButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets if the TimeElapsedButton is visible or not.
        /// </summary>
        public bool IsTimeElapsedButtonVisible
        {
            get { return (bool)GetValue(IsTimeElapsedButtonVisibleProperty); }
            set { SetValue(IsTimeElapsedButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsDurationButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsDurationButtonVisibleProperty { get { return isDurationButtonVisibleProperty; } }
        static readonly DependencyProperty isDurationButtonVisibleProperty = DependencyProperty.Register("IsDurationButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the DurationButton is visible or not.
        /// </summary>
        public bool IsDurationButtonVisible
        {
            get { return (bool)GetValue(IsDurationButtonVisibleProperty); }
            set { SetValue(IsDurationButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsTimeRemainingButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimeRemainingButtonVisibleProperty { get { return isTimeRemainingButtonVisibleProperty; } }
        static readonly DependencyProperty isTimeRemainingButtonVisibleProperty = DependencyProperty.Register("IsTimeRemainingButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets if the TimeRemainingButton is visible or not.
        /// </summary>
        public bool IsTimeRemainingButtonVisible
        {
            get { return (bool)GetValue(IsTimeRemainingButtonVisibleProperty); }
            set { SetValue(IsTimeRemainingButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSkipNextButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipNextButtonVisibleProperty { get { return isSkipNextButtonVisibleProperty; } }
        static readonly DependencyProperty isSkipNextButtonVisibleProperty = DependencyProperty.Register("IsSkipNextButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SkipNextButton is visible or not.
        /// </summary>
        public bool IsSkipNextButtonVisible
        {
            get { return (bool)GetValue(IsSkipNextButtonVisibleProperty); }
            set { SetValue(IsSkipNextButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSkipPreviousButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipPreviousButtonVisibleProperty { get { return isSkipPreviousButtonVisibleProperty; } }
        static readonly DependencyProperty isSkipPreviousButtonVisibleProperty = DependencyProperty.Register("IsSkipPreviousButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SkipPreviousButton is visible or not.
        /// </summary>
        public bool IsSkipPreviousButtonVisible
        {
            get { return (bool)GetValue(IsSkipPreviousButtonVisibleProperty); }
            set { SetValue(IsSkipPreviousButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSkipAheadButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipAheadButtonVisibleProperty { get { return isSkipAheadButtonVisibleProperty; } }
        static readonly DependencyProperty isSkipAheadButtonVisibleProperty = DependencyProperty.Register("IsSkipAheadButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SkipAheadButton is visible or not.
        /// </summary>
        public bool IsSkipAheadButtonVisible
        {
            get { return (bool)GetValue(IsSkipAheadButtonVisibleProperty); }
            set { SetValue(IsSkipAheadButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSkipBackButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSkipBackButtonVisibleProperty { get { return isSkipBackButtonVisibleProperty; } }
        static readonly DependencyProperty isSkipBackButtonVisibleProperty = DependencyProperty.Register("IsSkipBackButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SkipBackButton is visible or not.
        /// </summary>
        public bool IsSkipBackButtonVisible
        {
            get { return (bool)GetValue(IsSkipBackButtonVisibleProperty); }
            set { SetValue(IsSkipBackButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsFastForwardButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsFastForwardButtonVisibleProperty { get { return isFastForwardButtonVisibleProperty; } }
        static readonly DependencyProperty isFastForwardButtonVisibleProperty = DependencyProperty.Register("IsFastForwardButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the FastForwardButton is visible or not.
        /// </summary>
        public bool IsFastForwardButtonVisible
        {
            get { return (bool)GetValue(IsFastForwardButtonVisibleProperty); }
            set { SetValue(IsFastForwardButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsStopButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsStopButtonVisibleProperty { get { return isStopButtonVisibleProperty; } }
        static readonly DependencyProperty isStopButtonVisibleProperty = DependencyProperty.Register("IsStopButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the StopButton is visible or not.
        /// </summary>
        public bool IsStopButtonVisible
        {
            get { return (bool)GetValue(IsStopButtonVisibleProperty); }
            set { SetValue(IsStopButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsRewindButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsRewindButtonVisibleProperty { get { return isRewindButtonVisibleProperty; } }
        static readonly DependencyProperty isRewindButtonVisibleProperty = DependencyProperty.Register("IsRewindButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the RewindButton is visible or not.
        /// </summary>
        public bool IsRewindButtonVisible
        {
            get { return (bool)GetValue(IsRewindButtonVisibleProperty); }
            set { SetValue(IsRewindButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsReplayButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsReplayButtonVisibleProperty { get { return isReplayButtonVisibleProperty; } }
        static readonly DependencyProperty isReplayButtonVisibleProperty = DependencyProperty.Register("IsReplayButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the ReplayButton is visible or not.
        /// </summary>
        public bool IsReplayButtonVisible
        {
            get { return (bool)GetValue(IsReplayButtonVisibleProperty); }
            set { SetValue(IsReplayButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsResolutionIndicatorVisible dependency property.
        /// </summary>
        public static DependencyProperty IsResolutionIndicatorVisibleProperty { get { return isResolutionIndicatorVisibleProperty; } }
        static readonly DependencyProperty isResolutionIndicatorVisibleProperty = DependencyProperty.Register("IsResolutionIndicatorVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the ResolutionIndicator is visible or not.
        /// </summary>
        public bool IsResolutionIndicatorVisible
        {
            get { return (bool)GetValue(IsResolutionIndicatorVisibleProperty); }
            set { SetValue(IsResolutionIndicatorVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSignalStrengthVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSignalStrengthVisibleProperty { get { return isSignalStrengthVisibleProperty; } }
        static readonly DependencyProperty isSignalStrengthVisibleProperty = DependencyProperty.Register("IsSignalStrengthVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SignalStrength is visible or not.
        /// </summary>
        public bool IsSignalStrengthVisible
        {
            get { return (bool)GetValue(IsSignalStrengthVisibleProperty); }
            set { SetValue(IsSignalStrengthVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsFullScreenButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsFullScreenButtonVisibleProperty { get { return isFullScreenButtonVisibleProperty; } }
        static readonly DependencyProperty isFullScreenButtonVisibleProperty = DependencyProperty.Register("IsFullScreenButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the FullScreenButton is visible or not.
        /// </summary>
        public bool IsFullScreenButtonVisible
        {
            get { return (bool)GetValue(IsFullScreenButtonVisibleProperty); }
            set { SetValue(IsFullScreenButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsMuteButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsMuteButtonVisibleProperty { get { return isMuteButtonVisibleProperty; } }
        static readonly DependencyProperty isMuteButtonVisibleProperty = DependencyProperty.Register("IsMuteButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the MuteButton is visible or not.
        /// </summary>
        public bool IsMuteButtonVisible
        {
            get { return (bool)GetValue(IsMuteButtonVisibleProperty); }
            set { SetValue(IsMuteButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsSlowMotionButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsSlowMotionButtonVisibleProperty { get { return isSlowMotionButtonVisibleProperty; } }
        static readonly DependencyProperty isSlowMotionButtonVisibleProperty = DependencyProperty.Register("IsSlowMotionButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the SlowMotionButton is visible or not.
        /// </summary>
        public bool IsSlowMotionButtonVisible
        {
            get { return (bool)GetValue(IsSlowMotionButtonVisibleProperty); }
            set { SetValue(IsSlowMotionButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsPlayPauseButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsPlayPauseButtonVisibleProperty { get { return isPlayPauseButtonVisibleProperty; } }
        static readonly DependencyProperty isPlayPauseButtonVisibleProperty = DependencyProperty.Register("IsPlayPauseButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets if the PlayPauseButton is visible or not.
        /// </summary>
        public bool IsPlayPauseButtonVisible
        {
            get { return (bool)GetValue(IsPlayPauseButtonVisibleProperty); }
            set { SetValue(IsPlayPauseButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsVolumeButtonVisible dependency property.
        /// </summary>
        public static DependencyProperty IsVolumeButtonVisibleProperty { get { return isVolumeButtonVisibleProperty; } }
        static readonly DependencyProperty isVolumeButtonVisibleProperty = DependencyProperty.Register("IsVolumeButtonVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets if the VolumeButton is visible or not.
        /// </summary>
        public bool IsVolumeButtonVisible
        {
            get { return (bool)GetValue(IsVolumeButtonVisibleProperty); }
            set { SetValue(IsVolumeButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsVolumeSliderVisible dependency property.
        /// </summary>
        public static DependencyProperty IsVolumeSliderVisibleProperty { get { return isVolumeSliderVisibleProperty; } }
        static readonly DependencyProperty isVolumeSliderVisibleProperty = DependencyProperty.Register("IsVolumeSliderVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets if the VolumeSlider is visible or not.
        /// </summary>
        public bool IsVolumeSliderVisible
        {
            get { return (bool)GetValue(IsVolumeSliderVisibleProperty); }
            set { SetValue(IsVolumeSliderVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the IsTimelineVisible dependency property.
        /// </summary>
        public static DependencyProperty IsTimelineVisibleProperty { get { return isTimelineVisibleProperty; } }
        static readonly DependencyProperty isTimelineVisibleProperty = DependencyProperty.Register("IsTimelineVisible", typeof(bool), typeof(ControlPanel), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets if the Timeline is visible or not.
        /// </summary>
        public bool IsTimelineVisible
        {
            get { return (bool)GetValue(IsTimelineVisibleProperty); }
            set { SetValue(IsTimelineVisibleProperty, value); }
        }
    }
}