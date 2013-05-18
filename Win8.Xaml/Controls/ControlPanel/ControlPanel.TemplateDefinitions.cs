using System;
using System.Linq;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Input;
#endif

namespace Microsoft.PlayerFramework
{

    internal static class ControlPanelTemplateParts
    {
        public const string BorderElement = "Border";
        public const string LeftItemsPanelElement = "LeftItemsPanel";

        public const string ReplayButtonElement = "ReplayButton";
        public const string SkipPreviousButtonElement = "SkipPreviousButton";
        public const string RewindButtonElement = "RewindButton";
        public const string SkipBackButtonElement = "SkipBackButton";
        public const string PlayPauseButtonElement = "PlayPauseButton";
        public const string StopButtonElement = "StopButton";
        public const string SkipAheadButtonElement = "SkipAheadButton";
        public const string FastForwardButtonElement = "FastForwardButton";
        public const string SlowMotionButtonElement = "SlowMotionButton";
        public const string SkipNextButtonElement = "SkipNextButton";
        public const string TimeElapsedButtonElement = "TimeElapsedButton";

        public const string TimelineElement = "Timeline";

        public const string RightItemsPanelElement = "RightItemsPanel";
        public const string DurationButtonElement = "DurationButton";
        public const string TimeRemainingButtonElement = "TotalTimeRemainingButton";
        public const string CaptionSelectionButtonElement = "CaptionSelectionButton";
        public const string AudioSelectionButtonElement = "AudioSelectionButton";

        public const string VolumeButtonElement = "VolumeButton";
        public const string MuteButtonElement = "MuteButton";
        public const string VolumeSliderElement = "VolumeSlider";
        public const string FullScreenButtonElement = "FullScreenButton";
#if SILVERLIGHT
        public const string DisplayModeButtonElement = "StretchButton";
#endif
        public const string GoLiveButtonElement = "GoLiveButton";
        public const string SignalStrengthElement = "SignalStrength";
        public const string ResolutionIndicatorElement = "ResolutionIndicator";
    }

    internal static class ControlPanelVisualStates
    {
        internal static class GroupNames
        {
            internal const string LayoutStates = "LayoutStates";
            internal const string MediaStates = "MediaStates";
            internal const string MediaTypeStates = "MediaType";
        }

        internal static class MediaStates
        {
            internal const string Playing = "Playing";
            internal const string Closed = "Closed";
            internal const string Opening = "Opening";
            internal const string Paused = "Paused";
            internal const string Buffering = "Buffering";
        }

        internal static class MediaTypeStates
        {
            internal const string Video = "Video";
            internal const string Audio = "Audio";
        }

        internal static class LayoutStates
        {
            internal const string Compact = "Compact";
            internal const string Normal = "Normal";
        }
    }

    [TemplatePart(Name = ControlPanelTemplateParts.BorderElement, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ControlPanelTemplateParts.LeftItemsPanelElement, Type = typeof(Panel))]
    [TemplatePart(Name = ControlPanelTemplateParts.ReplayButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.SkipPreviousButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.RewindButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.SkipBackButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.PlayPauseButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.StopButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.SkipAheadButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.FastForwardButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.SlowMotionButtonElement, Type = typeof(ToggleButton))]
    [TemplatePart(Name = ControlPanelTemplateParts.SkipNextButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.TimeElapsedButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.TimelineElement, Type = typeof(Timeline))]
    [TemplatePart(Name = ControlPanelTemplateParts.RightItemsPanelElement, Type = typeof(Panel))]
    [TemplatePart(Name = ControlPanelTemplateParts.TimeRemainingButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.DurationButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.CaptionSelectionButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.AudioSelectionButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.VolumeButtonElement, Type = typeof(VolumeButton))]
    [TemplatePart(Name = ControlPanelTemplateParts.MuteButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.VolumeSliderElement, Type = typeof(Slider))]
    [TemplatePart(Name = ControlPanelTemplateParts.FullScreenButtonElement, Type = typeof(ToggleButton))]
    [TemplatePart(Name = ControlPanelTemplateParts.GoLiveButtonElement, Type = typeof(ButtonBase))]
    [TemplatePart(Name = ControlPanelTemplateParts.SignalStrengthElement, Type = typeof(SignalStrength))]
    [TemplatePart(Name = ControlPanelTemplateParts.ResolutionIndicatorElement, Type = typeof(ResolutionIndicator))]
#if SILVERLIGHT
    [TemplatePart(Name = ControlPanelTemplateParts.DisplayModeButtonElement, Type = typeof(ButtonBase))]
#endif
    [TemplateVisualState(Name = ControlPanelVisualStates.LayoutStates.Normal, GroupName = ControlPanelVisualStates.GroupNames.LayoutStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.LayoutStates.Compact, GroupName = ControlPanelVisualStates.GroupNames.LayoutStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaStates.Buffering, GroupName = ControlPanelVisualStates.GroupNames.MediaStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaStates.Closed, GroupName = ControlPanelVisualStates.GroupNames.MediaStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaStates.Opening, GroupName = ControlPanelVisualStates.GroupNames.MediaStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaStates.Paused, GroupName = ControlPanelVisualStates.GroupNames.MediaStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaStates.Playing, GroupName = ControlPanelVisualStates.GroupNames.MediaStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaTypeStates.Audio, GroupName = ControlPanelVisualStates.GroupNames.MediaTypeStates)]
    [TemplateVisualState(Name = ControlPanelVisualStates.MediaTypeStates.Video, GroupName = ControlPanelVisualStates.GroupNames.MediaTypeStates)]
    public partial class ControlPanel
    {
        #region Initialization

        const int MinTimelineWidth = 100;

        /// <summary>
        /// The left panel where all the controls before the timeline are placed.
        /// </summary>
        public Panel LeftItemsPanel { get; private set; }
        /// <summary>
        /// The panel that contains all controls to the right of the timeline.
        /// </summary>
        public Panel RightItemsPanel { get; private set; }
        /// <summary>
        /// The main panel element containing the controls.
        /// </summary>
        FrameworkElement borderElement;
        /// <summary>
        /// The replay (instant replay) button.
        /// </summary>
        ButtonBase replayButtonElement;
        /// <summary>
        /// The skip previous button (sends the user to the previous marker).
        /// </summary>
        ButtonBase skipPreviousButtonElement;
        /// <summary>
        /// The skip back button (sends the user x seconds back in the timeline).
        /// </summary>
        ButtonBase skipBackButtonElement;
        /// <summary>
        /// The rewind/reverse button.
        /// </summary>
        ButtonBase rewindButtonElement;
        /// <summary>
        /// The play and pause toggle button.
        /// </summary>
        ButtonBase playPauseButtonElement;
        /// <summary>
        /// The stop button.
        /// </summary>
        ButtonBase stopButtonElement;
        /// <summary>
        /// The fast forward button.
        /// </summary>
        ButtonBase fastForwardButtonElement;
        /// <summary>
        /// The slow motion button.
        /// </summary>
        ToggleButton slowMotionButtonElement;
        /// <summary>
        /// The skip next button. Sends the user to the next marker.
        /// </summary>
        ButtonBase skipNextButtonElement;
        /// <summary>
        /// The skip ahead button (sends the user x seconds forward in the timeline).
        /// </summary>
        ButtonBase skipAheadButtonElement;
        /// <summary>
        /// The current position of the media. Also sends the user back 30 seconds in the timeline.
        /// </summary>
        ButtonBase timeElapsedButtonElement;
        /// <summary>
        /// The panel that contains the timeline.
        /// </summary>
        Timeline timelineElement;
        /// <summary>
        /// The total duration of the media. Also Sends user 30 seconds ahead in the timeline.
        /// </summary>
        ButtonBase durationButtonElement;
        /// <summary>
        /// The time remaining in the media. Also Sends user 30 seconds ahead in the timeline.
        /// </summary>
        ButtonBase timeRemainingButtonElement;
        /// <summary>
        /// The Captions button.
        /// </summary>
        ButtonBase captionSelectionButtonElement;
        /// <summary>
        /// Allow you to select an audio track.
        /// </summary>
        ButtonBase audioSelectionButtonElement;
        /// <summary>
        /// The mute and volume slider toggle button (used for video media).
        /// </summary>
        VolumeButton volumeButtonElement;
        /// <summary>
        /// The mute toggle button (used for audio media).
        /// </summary>
        ButtonBase muteButtonElement;
        /// <summary>
        /// The horizontal volume slider (used for audio media where you can't extend vertically).
        /// </summary>
        VolumeSlider volumeSliderElement;
        /// <summary>
        /// The fullscreen toggle button.
        /// </summary>
        ButtonBase fullScreenButtonElement;
        /// <summary>
        /// The go live button.
        /// </summary>
        ButtonBase goLiveButtonElement;
        /// <summary>
        /// The signal strength indicator (usually adaptive only).
        /// </summary>
        SignalStrength signalStrengthElement;
        /// <summary>
        /// The signal strength indicator (usually adaptive only).
        /// </summary>
        ResolutionIndicator resolutionIndicatorElement;

#if SILVERLIGHT
        /// <summary>
        /// The button to allow the user to cycle through display modes (Letterbox, stretch, native, ...etc).
        /// </summary>
        ButtonBase displayModeButtonElement;
#endif

        /// <inheritdoc /> 
        void GetTemplateChildren()
        {
            borderElement = GetTemplateChild(ControlPanelTemplateParts.BorderElement) as FrameworkElement;
            LeftItemsPanel = GetTemplateChild(ControlPanelTemplateParts.LeftItemsPanelElement) as Panel;
            RightItemsPanel = GetTemplateChild(ControlPanelTemplateParts.RightItemsPanelElement) as Panel;

            replayButtonElement = GetTemplateChild(ControlPanelTemplateParts.ReplayButtonElement) as ButtonBase;
            skipPreviousButtonElement = GetTemplateChild(ControlPanelTemplateParts.SkipPreviousButtonElement) as ButtonBase;
            rewindButtonElement = GetTemplateChild(ControlPanelTemplateParts.RewindButtonElement) as ButtonBase;
            skipBackButtonElement = GetTemplateChild(ControlPanelTemplateParts.SkipBackButtonElement) as ButtonBase;
            playPauseButtonElement = GetTemplateChild(ControlPanelTemplateParts.PlayPauseButtonElement) as ButtonBase;
            stopButtonElement = GetTemplateChild(ControlPanelTemplateParts.StopButtonElement) as ButtonBase;
            skipAheadButtonElement = GetTemplateChild(ControlPanelTemplateParts.SkipAheadButtonElement) as ButtonBase;
            fastForwardButtonElement = GetTemplateChild(ControlPanelTemplateParts.FastForwardButtonElement) as ButtonBase;
            slowMotionButtonElement = GetTemplateChild(ControlPanelTemplateParts.SlowMotionButtonElement) as ToggleButton;
            skipNextButtonElement = GetTemplateChild(ControlPanelTemplateParts.SkipNextButtonElement) as ButtonBase;
            timeElapsedButtonElement = GetTemplateChild(ControlPanelTemplateParts.TimeElapsedButtonElement) as ButtonBase;

            timelineElement = GetTemplateChild(ControlPanelTemplateParts.TimelineElement) as Timeline;

            durationButtonElement = GetTemplateChild(ControlPanelTemplateParts.DurationButtonElement) as ButtonBase;
            timeRemainingButtonElement = GetTemplateChild(ControlPanelTemplateParts.TimeRemainingButtonElement) as ButtonBase;
            captionSelectionButtonElement = GetTemplateChild(ControlPanelTemplateParts.CaptionSelectionButtonElement) as ButtonBase;
            audioSelectionButtonElement = GetTemplateChild(ControlPanelTemplateParts.AudioSelectionButtonElement) as ButtonBase;

            volumeButtonElement = GetTemplateChild(ControlPanelTemplateParts.VolumeButtonElement) as VolumeButton;
            muteButtonElement = GetTemplateChild(ControlPanelTemplateParts.MuteButtonElement) as ButtonBase;
            volumeSliderElement = GetTemplateChild(ControlPanelTemplateParts.VolumeSliderElement) as VolumeSlider;

            fullScreenButtonElement = GetTemplateChild(ControlPanelTemplateParts.FullScreenButtonElement) as ButtonBase;
            goLiveButtonElement = GetTemplateChild(ControlPanelTemplateParts.GoLiveButtonElement) as ButtonBase;
            signalStrengthElement = GetTemplateChild(ControlPanelTemplateParts.SignalStrengthElement) as SignalStrength;
            resolutionIndicatorElement = GetTemplateChild(ControlPanelTemplateParts.ResolutionIndicatorElement) as ResolutionIndicator;
#if SILVERLIGHT
            displayModeButtonElement = GetTemplateChild(ControlPanelTemplateParts.DisplayModeButtonElement) as ButtonBase;
#endif
        }

        void SetDefaultVisualStates()
        {
            UpdateCurrentStateVisualState();
            UpdateCompactVisualState();
        }

        private void UpdateCompactVisualState()
        {
            if (IsCompact)
            {
                this.GoToVisualState(ControlPanelVisualStates.LayoutStates.Compact);
            }
            else
            {
                this.GoToVisualState(ControlPanelVisualStates.LayoutStates.Normal);
            }
        }

        void InitializeTemplateChildren()
        {
            if (timelineElement != null)
            {
                timelineElement.SizeChanged += TimelineElement_SizeChanged;
            }

            if (ViewModel != null)
            {
                InitializeViewModel(ViewModel);
            }
        }

        void UninitializeTemplateChildren()
        {
            if (timelineElement != null)
            {
                timelineElement.SizeChanged -= TimelineElement_SizeChanged;
            }

            if (ViewModel != null)
            {
                UninitializeViewModel(ViewModel);
            }
        }

        void InitializeViewModel(IInteractiveViewModel vm)
        {
            vm.CurrentStateChanged += ViewModel_CurrentStateChanged;
        }

        void UninitializeViewModel(IInteractiveViewModel vm)
        {
            vm.CurrentStateChanged -= ViewModel_CurrentStateChanged;
        }

        #endregion

        #region Element events

        double lastToggleWidth = 320;
        void TimelineElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsCompact && e.NewSize.Width > MinTimelineWidth && lastToggleWidth + 5 < this.ActualWidth)
            {
                Dispatcher.BeginInvoke(() => IsCompact = false);
            }
            else if (!IsCompact && e.NewSize.Width < MinTimelineWidth)
            {
                // don't pop into compact mode unless you reach the same point at which you entered
                lastToggleWidth = this.ActualWidth;
                Dispatcher.BeginInvoke(() => IsCompact = true);
            }
        }
        
        #endregion

        #region MediaPlayer Events

        void ViewModel_CurrentStateChanged(object sender, object e)
        {
            UpdateCurrentStateVisualState();
        }

        private void UpdateCurrentStateVisualState()
        {
            if (ViewModel == null)
            {
                this.GoToVisualState(ControlPanelVisualStates.MediaStates.Closed);
            }
            else
            {
                switch (ViewModel.CurrentState)
                {
#if SILVERLIGHT
                    case MediaElementState.AcquiringLicense:
                    case MediaElementState.Individualizing:
#endif
                    case MediaElementState.Opening:
                        this.GoToVisualState(ControlPanelVisualStates.MediaStates.Opening);
                        break;
                    case MediaElementState.Buffering:
                        this.GoToVisualState(ControlPanelVisualStates.MediaStates.Buffering);
                        break;
                    case MediaElementState.Playing:
                        this.GoToVisualState(ControlPanelVisualStates.MediaStates.Playing);
                        break;
                    case MediaElementState.Closed:
                        this.GoToVisualState(ControlPanelVisualStates.MediaStates.Closed);
                        break;
                    case MediaElementState.Paused:
                    case MediaElementState.Stopped:
                        this.GoToVisualState(ControlPanelVisualStates.MediaStates.Paused);
                        break;
                }
            }
        }

        #endregion

        #region Properties

        bool isCompact;
        private bool IsCompact
        {
            get { return isCompact; }
            set
            {
                if (isCompact != value)
                {
                    isCompact = value;
                    UpdateCompactVisualState();
                }
            }
        }

        #endregion

    }
}
