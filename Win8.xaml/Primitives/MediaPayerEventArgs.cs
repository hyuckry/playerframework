using System;
#if SILVERLIGHT
using System.IO;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows;
#else
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Event arguments for Initialized events.
    /// </summary>
#if SILVERLIGHT
    public sealed class InitializedEventArgs : EventArgs
#else
    public sealed class InitializedEventArgs
#endif
    {
        internal InitializedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for Updated events.
    /// </summary>
#if SILVERLIGHT
    public sealed class UpdatedEventArgs : EventArgs
#else
    public sealed class UpdatedEventArgs
#endif
    {
        internal UpdatedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaStarted events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaStartedEventArgs : EventArgs
#else
    public sealed class MediaStartedEventArgs
#endif
    {
        internal MediaStartedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaClosed events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaClosedEventArgs : EventArgs
#else
    public sealed class MediaClosedEventArgs
#endif
    {
        internal MediaClosedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for IsMutedChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsMutedChangedEventArgs : EventArgs
#else
    public sealed class IsMutedChangedEventArgs
#endif
    {
        internal IsMutedChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for IsLiveChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsLiveChangedEventArgs : EventArgs
#else
    public sealed class IsLiveChangedEventArgs
#endif
    {
        internal IsLiveChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for IsFullScreenChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsFullScreenChangedEventArgs : EventArgs
#else
    public sealed class IsFullScreenChangedEventArgs
#endif
    {
        internal IsFullScreenChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for IsCaptionsActiveChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsCaptionsActiveChangedEventArgs : EventArgs
#else
    public sealed class IsCaptionsActiveChangedEventArgs
#endif
    {
        internal IsCaptionsActiveChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaQualityChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaQualityChangedEventArgs : EventArgs
#else
    public sealed class MediaQualityChangedEventArgs
#endif
    {
        internal MediaQualityChangedEventArgs(MediaQuality oldValue, MediaQuality newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the previous MediaQuality
        /// </summary>
        public MediaQuality OldValue { get; private set; }

        /// <summary>
        /// Gets the new MediaQuality
        /// </summary>
        public MediaQuality NewValue { get; private set; }
    }

    /// <summary>
    /// Event arguments for IsSlowMotionChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsSlowMotionChangedEventArgs : EventArgs
#else
    public sealed class IsSlowMotionChangedEventArgs
#endif
    {
        internal IsSlowMotionChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for GoLiveInvoked events.
    /// </summary>
#if SILVERLIGHT
    public sealed class GoLiveInvokedEventArgs : EventArgs
#else
    public sealed class GoLiveInvokedEventArgs
#endif
    {
        internal GoLiveInvokedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for CaptionSelectionInvoked events.
    /// </summary>
#if SILVERLIGHT
    public sealed class CaptionSelectionInvokedEventArgs : EventArgs
#else
    public sealed class CaptionSelectionInvokedEventArgs
#endif
    {
        internal CaptionSelectionInvokedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for AudioSelectionInvoked events.
    /// </summary>
#if SILVERLIGHT
    public sealed class AudioSelectionInvokedEventArgs : EventArgs
#else
    public sealed class AudioSelectionInvokedEventArgs
#endif
    {
        internal AudioSelectionInvokedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaPlayerFeatureEnabledChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaPlayerFeatureEnabledChangedEventArgs : EventArgs
#else
    public sealed class MediaPlayerFeatureEnabledChangedEventArgs
#endif
    {
        internal MediaPlayerFeatureEnabledChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaPlayerFeatureAllowedChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaPlayerFeatureAllowedChangedEventArgs : EventArgs
#else
    public sealed class MediaPlayerFeatureAllowedChangedEventArgs
#endif
    {
        internal MediaPlayerFeatureAllowedChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for MediaPlayerFeatureVisibleChanged events.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaPlayerFeatureVisibleChangedEventArgs : EventArgs
#else
    public sealed class MediaPlayerFeatureVisibleChangedEventArgs
#endif
    {
        internal MediaPlayerFeatureVisibleChangedEventArgs() { }
    }

    /// <summary>
    /// Event arguments for IsInteractive events.
    /// </summary>
#if SILVERLIGHT
    public sealed class IsInteractiveEventArgs : EventArgs
#else
    public sealed class IsInteractiveEventArgs
#endif
    {
        internal IsInteractiveEventArgs() { }
    }

    /// <summary>
    /// EventArgs associated with a Position in the media.
    /// </summary>
#if SILVERLIGHT
    public sealed class SeekingEventArgs : EventArgs
#else
    public sealed class SeekingEventArgs
#endif
    {
        internal SeekingEventArgs(TimeSpan previousPosition, TimeSpan newPosition)
        {
            this.Position = newPosition;
            this.PreviousPosition = previousPosition;
        }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan PreviousPosition { get; private set; }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }
    }

    /// <summary>
    /// EventArgs associated with a skip operation.
    /// </summary>
#if SILVERLIGHT
    public sealed class SkippingEventArgs : EventArgs
#else
    public sealed class SkippingEventArgs
#endif
    {
        internal SkippingEventArgs(TimeSpan position)
        {
            this.Position = position;
            Canceled = false;
        }

        internal SkippingEventArgs(TimeSpan position, bool canceled)
            : this(position)
        {
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// EventArgs associated with a scrubbing operation.
    /// </summary>
#if SILVERLIGHT
    public sealed class StartingScrubEventArgs : EventArgs
#else
    public sealed class StartingScrubEventArgs
#endif
    {
        internal StartingScrubEventArgs(TimeSpan position)
        {
            this.Position = position;
            Canceled = false;
        }

        internal StartingScrubEventArgs(TimeSpan position, bool canceled)
            : this(position)
        {
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// EventArgs associated with a scrubbing operation that is in progress.
    /// </summary>
#if SILVERLIGHT
    public sealed class CompletingScrubEventArgs : EventArgs
#else
    public sealed class CompletingScrubEventArgs
#endif
    {
        internal CompletingScrubEventArgs(TimeSpan startPosition, TimeSpan newPosition)
            : this(startPosition, newPosition, false)
        { }

        internal CompletingScrubEventArgs(TimeSpan startPosition, TimeSpan newPosition, bool canceled)
        {
            this.Position = newPosition;
            StartPosition = startPosition;
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position when scrubbing started
        /// </summary>
        public TimeSpan StartPosition { get; private set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// EventArgs associated with a scrubbing operation that is in progress.
    /// </summary>
#if SILVERLIGHT
    public sealed class ScrubbingEventArgs : EventArgs
#else
    public sealed class ScrubbingEventArgs
#endif
    {
        internal ScrubbingEventArgs(TimeSpan startPosition, TimeSpan newPosition)
            : this(startPosition, newPosition, false)
        { }

        internal ScrubbingEventArgs(TimeSpan startPosition, TimeSpan newPosition, bool canceled)
        {
            this.Position = newPosition;
            StartPosition = startPosition;
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position when scrubbing started
        /// </summary>
        public TimeSpan StartPosition { get; private set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the AdvertisingStateChanged event.
    /// </summary>
    public sealed class AdvertisingStateChangedEventArgs
    {
        internal AdvertisingStateChangedEventArgs(AdvertisingState oldValue, AdvertisingState newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public AdvertisingState NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public AdvertisingState OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the AudioStreamIndexChanged event.
    /// </summary>
    public sealed class AudioStreamIndexChangedEventArgs
    {
        internal AudioStreamIndexChangedEventArgs(int? oldValue, int? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public int? NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public int? OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the DurationChanged event.
    /// </summary>
    public sealed class DurationChangedEventArgs
    {
        internal DurationChangedEventArgs(TimeSpan oldValue, TimeSpan newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the EndTimeChanged event.
    /// </summary>
    public sealed class EndTimeChangedEventArgs
    {
        internal EndTimeChangedEventArgs(TimeSpan oldValue, TimeSpan newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan OldValue { get; private set; }
    }

    /// <summary>
    /// Provides data about a change in value to a dependency property as reported by particular routed events, including hte previous and current value of the property that changed.
    /// </summary>
    /// <typeparam name="T">The type of the dependency property that has changed.</typeparam>
    public sealed class InteractiveViewModelChangedEventArgs
    {
        /// <summary>
        /// Creates a new instance of InteractiveViewModelChangedEventArgs.
        /// </summary>
        /// <param name="oldValue">the previous value of the property.</param>
        /// <param name="newValue">The new value of the property.</param>
        internal InteractiveViewModelChangedEventArgs(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IInteractiveViewModel NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IInteractiveViewModel OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the LivePositionChanged event.
    /// </summary>
    public sealed class LivePositionChangedEventArgs
    {
        internal LivePositionChangedEventArgs(TimeSpan? oldValue, TimeSpan? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan? NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan? OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with a MediaPlayer action event.
    /// </summary>
    public sealed class MediaEndedEventArgs : RoutedEventArgs
    {
        internal MediaEndedEventArgs(RoutedEventArgs e) { }

        /// <summary>
        /// Gets or sets whether the event was already handled.
        /// </summary>
        public bool Handled { get; set; }
    }

    /// <summary>
    /// Provides data for a deferrable MediaLoading event.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaLoadingEventArgs : EventArgs
#else
    public sealed class MediaLoadingEventArgs
#endif
    {
        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, Uri source)
        {
            Source = source;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source Uri of the loading operation. This may be null if the source is a stream.
        /// </summary>
        public Uri Source { get; set; }

#if NETFX_CORE
        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, IRandomAccessStream sourceStream, string mimeType)
        {
            SourceStream = sourceStream;
            MimeType = mimeType;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source stream of the loading operation. This may be null if the source is a Uri.
        /// </summary>
        public IRandomAccessStream SourceStream { get; set; }

        /// <summary>
        /// Gets or sets the mime type of the loading operation. This is unused if the source is a Uri.
        /// </summary>
        public string MimeType { get; set; }
#else

        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, Stream sourceStream)
        {
            SourceStream = sourceStream;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source MediaStreamSource of the loading operation. This may be null if the source is a Uri or MediaStreamSource.
        /// </summary>
        public Stream SourceStream { get; set; }

        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, MediaStreamSource mediaStreamSource)
        {
            MediaStreamSource = mediaStreamSource;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source MediaStreamSource of the loading operation. This may be null if the source is a stream or Uri.
        /// </summary>
        public MediaStreamSource MediaStreamSource { get; set; }
#endif

        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Provides data for an Initializing event.
    /// </summary>
#if SILVERLIGHT
    public sealed class InitializingEventArgs : EventArgs
#else
    public sealed class InitializingEventArgs
#endif
    {
        internal InitializingEventArgs(MediaPlayerDeferrableOperation deferrableOperation)
        {
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Provides data for an MediaEnding event.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaEndingEventArgs : EventArgs
#else
    public sealed class MediaEndingEventArgs
#endif
    {
        internal MediaEndingEventArgs(MediaPlayerDeferrableOperation deferrableOperation)
        {
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Provides data for a deferrable event.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaStartingEventArgs : EventArgs
#else
    public sealed class MediaStartingEventArgs
#endif
    {
        internal MediaStartingEventArgs(MediaPlayerDeferrableOperation deferrableOperation)
        {
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the PlayerStateChanged event.
    /// </summary>
    public sealed class PlayerStateChangedEventArgs
    {
        internal PlayerStateChangedEventArgs(PlayerState oldValue, PlayerState newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public PlayerState NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public PlayerState OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the PositionChanged event.
    /// </summary>
    public sealed class PositionChangedEventArgs
    {
        internal PositionChangedEventArgs(TimeSpan oldValue, TimeSpan newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the SelectedAudioStreamChanged event.
    /// </summary>
    public sealed class SelectedAudioStreamChangedEventArgs
    {
        internal SelectedAudioStreamChangedEventArgs(IAudioStream oldValue, IAudioStream newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets or sets whether the event handler took responsibility for modifying the selected audio stream.
        /// Setting to true will prevent the MediaPlayer from setting the AudioStreamIndex property automatically.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IAudioStream NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IAudioStream OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the SelectedCaptionChanged event.
    /// </summary>
    public sealed class SelectedCaptionChangedEventArgs
    {
        internal SelectedCaptionChangedEventArgs(ICaption oldValue, ICaption newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public ICaption NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public ICaption OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the SignalStrengthChanged event.
    /// </summary>
    public sealed class SignalStrengthChangedEventArgs
    {
        internal SignalStrengthChangedEventArgs(double oldValue, double newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public double NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public double OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the SkipAheadIntervalChanged event.
    /// </summary>
    public sealed class SkipAheadIntervalChangedEventArgs
    {
        internal SkipAheadIntervalChangedEventArgs(TimeSpan? oldValue, TimeSpan? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan? NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan? OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the SkipBackIntervalChanged event.
    /// </summary>
    public sealed class SkipBackIntervalChangedEventArgs
    {
        internal SkipBackIntervalChangedEventArgs(TimeSpan? oldValue, TimeSpan? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan? NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan? OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the StartTimeChanged event.
    /// </summary>
    public sealed class StartTimeChangedEventArgs
    {
        internal StartTimeChangedEventArgs(TimeSpan oldValue, TimeSpan newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the TimeFormatConverterChanged event.
    /// </summary>
    public sealed class TimeFormatConverterChangedEventArgs
    {
        internal TimeFormatConverterChangedEventArgs(IValueConverter oldValue, IValueConverter newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IValueConverter NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IValueConverter OldValue { get; private set; }
    }

    /// <summary>
    /// Contains state information and event data associated with the TimeRemainingChanged event.
    /// </summary>
    public sealed class TimeRemainingChangedEventArgs
    {
        internal TimeRemainingChangedEventArgs(TimeSpan oldValue, TimeSpan newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public TimeSpan NewValue { get; private set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public TimeSpan OldValue { get; private set; }
    }
}
