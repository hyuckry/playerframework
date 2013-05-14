#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Provides an interface to control media interactivity. 
    /// The MediaPlayer implements this by default but a plugin could also provide an implementation for features like advertising.
    /// </summary>
    public interface IInteractiveViewModel
    {
        /// <summary>
        /// Raised to indicate that the user is requesting to skip back
        /// </summary>
        event EventHandler<SkipRequestedEventArgs> SkipBackRequested;

        /// <summary>
        /// Raised to indicate that the user is requesting to skip ahead
        /// </summary>
        event EventHandler<SkipRequestedEventArgs> SkipAheadRequested;

        /// <summary>
        /// Raised to indicate that the user is requesting to seek
        /// </summary>
        event EventHandler<SeekRequestedEventArgs> SeekRequested;

        /// <summary>
        /// Raised to indicate that the user is requesting to start scrubbing
        /// </summary>
        event EventHandler<ScrubStartRequestedEventArgs> ScrubStartRequested;

        /// <summary>
        /// Raised to indicate that the user is scrubbing
        /// </summary>
        event EventHandler<ScrubRequestedEventArgs> ScrubRequested;

        /// <summary>
        /// Raised to indicate that the user is completing the scrub
        /// </summary>
        event EventHandler<ScrubCompleteRequestedEventArgs> ScrubCompleteRequested;

        /// <summary>
        /// Raised when the user interacts
        /// </summary>
        event EventHandler<InteractionEventArgs> Interacting;

        /// <summary>
        /// Occurs when the value of the CurrentState property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler CurrentStateChanged;
#else
        event EventHandler<object> CurrentStateChanged;
#endif

        /// <summary>
        /// Can be called by UI elements to indicate that the user is interacting
        /// </summary>
        void OnInteracting(InteractionType interactionType);

        /// <summary>
        /// Gets a collection of markers to display
        /// </summary>
        IEnumerable<VisualMarker> VisualMarkers { get; }

        /// <summary>
        /// Gets whether or not he media is playing at high definition. Usually this means a resolution >= 1280×720 pixels (720p)
        /// </summary>
        MediaQuality MediaQuality { get; }

        /// <summary>
        /// Gets a value that indicates the current buffering progress.
        /// The amount of buffering that is completed for media content. The value ranges from 0 to 1. Multiply by 100 to obtain a percentage.
        /// </summary>
        double BufferingProgress { get; }

        /// <summary>
        /// Gets a percentage value indicating the amount of download completed for content located on a remote server.
        /// The value ranges from 0 to 1. Multiply by 100 to obtain a percentage.
        /// </summary>
        double DownloadProgress { get; }

        /// <summary>
        /// Gets the start time of the current video or audio.
        /// </summary>
        TimeSpan StartTime { get; }

        /// <summary>
        /// Gets the end time of the current video or audio.
        /// </summary>
        TimeSpan EndTime { get; }

        /// <summary>
        /// Gets the duration of the current video or audio.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the time remaining before the media will finish.
        /// </summary>
        TimeSpan TimeRemaining { get; }

        /// <summary>
        /// Gets the current position of progress through the media's playback time (or the amount of time since the beginning of the media).
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        /// Gets the maximum position that the user can seek or scrub to in the timeline.
        /// Useful for realtime/live playback.
        /// </summary>
        TimeSpan MaxPosition { get; }

        /// <summary>
        /// Gets the status of the MediaElement.
        /// The state can be one of the following (as defined in the MediaElementState enumeration):
        /// Buffering, Closed, Opening, Paused, Playing, Stopped.
        /// </summary>
        MediaElementState CurrentState { get; }

        /// <summary>
        /// Gets or sets the media's volume.
        /// The media's volume represented on a linear scale between 0 and 1. The default is 0.5.
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Gets or sets the selected caption.
        /// </summary>
        ICaption SelectedCaption { get; set; }

        /// <summary>
        /// Gets the caption stream names to be displayed to the user for selecting from multiple captions.
        /// </summary>
        IEnumerable<ICaption> AvailableCaptions { get; }

        /// <summary>
        /// Gets or sets the selected caption.
        /// </summary>
        IAudioStream SelectedAudioStream { get; set; }

        /// <summary>
        /// Gets the caption stream names to be displayed to the user for selecting from multiple captions.
        /// </summary>
        IEnumerable<IAudioStream> AvailableAudioStreams { get; }

        /// <summary>
        /// Gets a an IValueConverter that is used to display the time to the user such as the position, duration, and time remaining.
        /// The default value applies the string format of "h\\:mm\\:ss".
        /// </summary>
        IValueConverter TimeFormatConverter { get; }

        /// <summary>
        /// Gets the amount of time in the video to skip back when the user selects skip back.
        /// This can be set to null to cause the skip back action to go back to the beginning.
        /// </summary>
        TimeSpan? SkipBackInterval { get; }

        /// <summary>
        /// Gets the amount of time in the video to skip next when the user selects skip ahead.
        /// This can be set to null to cause the skip next action to go directly to the end.
        /// </summary>
        TimeSpan? SkipAheadInterval { get; }

        /// <summary>
        /// Gets or sets whether or not the media is playing in slow motion.
        /// </summary>
        bool IsSlowMotion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio is muted.
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// Gets or sets if the player should indicate it is in fullscreen mode.
        /// </summary>
        bool IsFullScreen { get; set; }

        /// <summary>
        /// Gets the signal strength used to indicate visually to the user the quality of the bitrate.
        /// Note: This is only useful for adaptive streaming.
        /// </summary>
        double SignalStrength { get; }

        /// <summary>
        /// Gets the enabled state of the closed captions feature.
        /// </summary>
        bool IsCaptionSelectionEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the play/resume feature.
        /// </summary>
        bool IsPlayResumeEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the pause feature.
        /// </summary>
        bool IsPauseEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the stop feature.
        /// </summary>
        bool IsStopEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the instant replay feature.
        /// </summary>
        bool IsReplayEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the audio stream selection feature.
        /// </summary>
        bool IsAudioSelectionEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the rewind feature.
        /// </summary>
        bool IsRewindEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the fast forward feature.
        /// </summary>
        bool IsFastForwardEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the slow motion feature.
        /// </summary>
        bool IsSlowMotionEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the seek feature.
        /// </summary>
        bool IsSeekEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the skip previous feature.
        /// </summary>
        bool IsSkipPreviousEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the skip next feature.
        /// </summary>
        bool IsSkipNextEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the skip back feature.
        /// </summary>
        bool IsSkipBackEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the skip ahead feature.
        /// </summary>
        bool IsSkipAheadEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the scrubbing feature.
        /// </summary>
        bool IsScrubbingEnabled { get; }

        /// <summary>
        /// Gets the enabled state of the go live feature.
        /// </summary>
        bool IsGoLiveEnabled { get; }

        /// <summary>
        /// Raised when the IsPlayResumeEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsPlayResumeEnabledChanged;
#else
        event EventHandler<object> IsPlayResumeEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsPauseEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsPauseEnabledChanged;
#else
        event EventHandler<object> IsPauseEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsStopEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsStopEnabledChanged;
#else
        event EventHandler<object> IsStopEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsReplayEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsReplayEnabledChanged;
#else
        event EventHandler<object> IsReplayEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsAudioSelectionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsAudioSelectionEnabledChanged;
#else
        event EventHandler<object> IsAudioSelectionEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsAudioSelectionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsCaptionSelectionEnabledChanged;
#else
        event EventHandler<object> IsCaptionSelectionEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsRewindEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsRewindEnabledChanged;
#else
        event EventHandler<object> IsRewindEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsFastForwardEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsFastForwardEnabledChanged;
#else
        event EventHandler<object> IsFastForwardEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSlowMotionEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSlowMotionEnabledChanged;
#else
        event EventHandler<object> IsSlowMotionEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSeekEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSeekEnabledChanged;
#else
        event EventHandler<object> IsSeekEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSkipPreviousEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSkipPreviousEnabledChanged;
#else
        event EventHandler<object> IsSkipPreviousEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSkipNextEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSkipNextEnabledChanged;
#else
        event EventHandler<object> IsSkipNextEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSkipBackEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSkipBackEnabledChanged;
#else
        event EventHandler<object> IsSkipBackEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsSkipAheadEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsSkipAheadEnabledChanged;
#else
        event EventHandler<object> IsSkipAheadEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsScrubbingEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsScrubbingEnabledChanged;
#else
        event EventHandler<object> IsScrubbingEnabledChanged;
#endif

        /// <summary>
        /// Raised when the IsGoLiveEnabled property changes.
        /// </summary>
#if SILVERLIGHT
        event EventHandler IsGoLiveEnabledChanged;
#else
        event EventHandler<object> IsGoLiveEnabledChanged;
#endif

        /// <summary>
        /// Invokes the closed captioning feature.
        /// </summary>
        void InvokeCaptionSelection();

        /// <summary>
        /// Invokes the audio stream selection feature.
        /// </summary>
        void InvokeAudioSelection();

        /// <summary>
        /// Seeks to the live position on the media. Only supported during live media playback.
        /// </summary>
        void GoLive();

        /// <summary>
        /// Actively scrubbing at the indicated position.
        /// </summary>
        /// <param name="position">The position on the timeline that the user is actively scrubbing over.</param>
        /// <param name="canceled">Gets or sets whether the operation should be cancelled</param>
        void Scrub(TimeSpan position, out bool canceled);

        /// <summary>
        /// A scrub action has been initiated.
        /// </summary>
        /// <param name="position">The position that the scrubbing action was initiated at.</param>
        /// <param name="canceled">Gets or sets whether the operation should be cancelled</param>
        void StartScrub(TimeSpan position, out bool canceled);

        /// <summary>
        /// A scrub action has completed.
        /// </summary>
        /// <param name="position">The position that the scrubbing action was completed.</param>
        /// <param name="canceled">Gets whether the operation was already cancelled</param>
        /// <param name="cancel">Sets whether the operation should be cancelled</param>
        void CompleteScrub(TimeSpan position, bool canceled, out bool cancel);

        /// <summary>
        /// Skip back to the previous marker in the timeline.
        /// </summary>
        void SkipPrevious();

        /// <summary>
        /// Skip forward to the next position in the timeline.
        /// </summary>
        void SkipNext();

        /// <summary>
        /// Skip back to a previous position in the timeline. (usually 30 seconds)
        /// </summary>
        void SkipBack();

        /// <summary>
        /// Skip ahead to a future position in the timeline. (usually 30 seconds)
        /// </summary>
        void SkipAhead();

        /// <summary>
        /// Seek to a specific position.
        /// </summary>
        /// <param name="position">The position to seek to.</param>
        /// <param name="canceled">Allows the consumer to indicate the seek should be canceled.</param>
        void Seek(TimeSpan position, out bool canceled);

        /// <summary>
        /// Play or resume media playback. If playback is in rewind or fastforward mode, restore the original playback rate.
        /// </summary>
        void PlayResume();

        /// <summary>
        /// Pause the media.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop the media.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "MediaElement compatibility")]
        void Stop();

        /// <summary>
        /// Replay the media (instant replay).
        /// </summary>
        void Replay();

        /// <summary>
        /// Decrease the playback rate.
        /// </summary>
        void DecreasePlaybackRate();

        /// <summary>
        /// Increase the playback rate.
        /// </summary>
        void IncreasePlaybackRate();

#if SILVERLIGHT
        /// <summary>
        /// Change the display mode to the next available option.
        /// </summary>
        void CycleDisplayMode();
#endif
    }

    /// <summary>
    /// Provides information about the interaction that occurred.
    /// </summary>
#if SILVERLIGHT
    public sealed class InteractionEventArgs : EventArgs
#else
    public sealed class InteractionEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the InteractionEventArgs
        /// </summary>
        /// <param name="interactionType">The type of interaction that occurred.</param>
        public InteractionEventArgs(InteractionType interactionType)
        {
            InteractionType = interactionType;
        }

        /// <summary>
        /// The type of interaction that occurred.
        /// </summary>
        public InteractionType InteractionType { get; private set; }
    }

    /// <summary>
    /// Provides information about the Seeking event.
    /// </summary>
#if SILVERLIGHT
    public sealed class SeekRequestedEventArgs : EventArgs
#else
    public sealed class SeekRequestedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the SeekingEventArgs
        /// </summary>
        /// <param name="position">The position the user is seeking to.</param>
        public SeekRequestedEventArgs(TimeSpan position)
        {
            Position = position;
        }

        /// <summary>
        /// The position the user is seeking to.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// Gets or sets if the operation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }
    }
    
    /// <summary>
    /// Provides information about a CompletingScrub event.
    /// </summary>
#if SILVERLIGHT
    public sealed class ScrubCompleteRequestedEventArgs : EventArgs
#else
    public sealed class ScrubCompleteRequestedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the SkippingEventArgs
        /// </summary>
        /// <param name="position">The position the user is scrubbing to.</param>
        /// <param name="canceled">Indicates that the operation was already cancelled.</param>
        public ScrubCompleteRequestedEventArgs(TimeSpan position, bool canceled)
        {
            Position = position;
            Canceled = canceled;
        }

        /// <summary>
        /// The position the user is scrubbing to.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// Gets or sets if the operation was already cancelled.
        /// </summary>
        public bool Canceled { get; private set; }

        /// <summary>
        /// Gets or sets if the operation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about a Scrubbing event.
    /// </summary>
#if SILVERLIGHT
    public sealed class ScrubRequestedEventArgs : EventArgs
#else
    public sealed class ScrubRequestedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the ScrubbingEventArgs
        /// </summary>
        /// <param name="position">The position the user is scrubbing to.</param>
        public ScrubRequestedEventArgs(TimeSpan position)
        {
            Position = position;
        }

        /// <summary>
        /// The position the user is scrubbing to.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// Gets or sets if the operation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about a StartingScrub event.
    /// </summary>
#if SILVERLIGHT
    public sealed class ScrubStartRequestedEventArgs : EventArgs
#else
    public sealed class ScrubStartRequestedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the StartingScrubEventArgs
        /// </summary>
        /// <param name="position">The position the user is scrubbing to.</param>
        public ScrubStartRequestedEventArgs(TimeSpan position)
        {
            Position = position;
        }

        /// <summary>
        /// The position the user is scrubbing to.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// Gets or sets if the operation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides information about a SkippingAhead or SkippingBack event.
    /// </summary>
#if SILVERLIGHT
    public sealed class SkipRequestedEventArgs : EventArgs
#else
    public sealed class SkipRequestedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of the SkippingEventArgs
        /// </summary>
        /// <param name="position">The position the user is seeking to.</param>
        public SkipRequestedEventArgs(TimeSpan position)
        {
            Position = position;
        }

        /// <summary>
        /// The position the user is skipping to.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }
}
