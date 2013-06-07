using System;
using System.Net;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.VideoAdvertising;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// Binds a vpaid ad player to the UI. This does 2 things: 1) reacts to changes in the vpaid player 2) pushes commands at the vpaid player to pause, resume, stop and skip.
    /// Supports both VPAID 1.1 and VPAID 2.0 players
    /// </summary>
    public sealed class VpaidLinearAdViewModel : IInteractiveViewModel, INotifyPropertyChanged
    {
        private vPaidState state;
        private TimeSpan estimatedDuration;

        enum vPaidState
        {
            None,
            Loaded,
            Playing,
            Paused,
            Completed,
            Failure
        }

        /// <summary>
        /// HACK: Allows an instance to be created from Xaml. Without this, xamltypeinfo is not generated and binding will not work.
        /// </summary>
        public VpaidLinearAdViewModel()
        {
            SkipPreviousThreshold = TimeSpan.FromSeconds(2);
        }

        internal VpaidLinearAdViewModel(IVpaid vpaid, MediaPlayer mediaPlayer)
            : this()
        {
            MediaPlayer = mediaPlayer;
            Vpaid = vpaid;
            if (Vpaid is IVpaid2)
            {
                Vpaid2 = Vpaid as IVpaid2;
            }
            state = vPaidState.None;

            WireVpaid();
            WireMediaPlayer();
        }

        /// <summary>
        /// Gets or sets how far away from the previous marker you should be for it to be recognized when skipping previous.
        /// Default is 2 seconds.
        /// </summary>
        public TimeSpan SkipPreviousThreshold { get; set; }

        /// <summary>
        /// Gets the MediaPlayer instance the ViewModel is associated with.
        /// </summary>
        public MediaPlayer MediaPlayer { get; private set; }

        /// <summary>
        /// Gets the VPAID 1.1 player associated with the view model.
        /// </summary>
        public IVpaid Vpaid { get; private set; }

        /// <summary>
        /// Gets the VPAID 2.0 player associated with the view mode. This is the same object as Vpaid and is null if the player only supports v1.1
        /// </summary>
        public IVpaid2 Vpaid2 { get; private set; }

        private void WireMediaPlayer()
        {
            MediaPlayer.IsFullScreenChanged += MediaPlayer_IsFullScreenChanged;
            MediaPlayer.IsMutedChanged += MediaPlayer_IsMutedChanged;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
        }

        private void UnwireMediaPlayer()
        {
            MediaPlayer.IsFullScreenChanged -= MediaPlayer_IsFullScreenChanged;
            MediaPlayer.IsMutedChanged -= MediaPlayer_IsMutedChanged;
            MediaPlayer.VolumeChanged -= MediaPlayer_VolumeChanged;
        }

        void MediaPlayer_VolumeChanged(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(() => Volume);
        }

        void MediaPlayer_IsMutedChanged(object sender, object e)
        {
            OnPropertyChanged(() => IsMuted);
        }

        void MediaPlayer_IsFullScreenChanged(object sender, object e)
        {
            OnPropertyChanged(() => IsFullScreen);
        }
        private void WireVpaid()
        {
            Vpaid.AdRemainingTimeChange += vpaid_AdRemainingTimeChange;
            Vpaid.AdError += vpaid_AdError;
            Vpaid.AdLoaded += vpaid_AdLoaded;
            Vpaid.AdStarted += vpaid_AdStarted;
            Vpaid.AdStopped += vpaid_AdStopped;
            Vpaid.AdPlaying += vpaid_AdPlaying;
            Vpaid.AdPaused += vpaid_AdPaused;
            Vpaid.AdClickThru += Vpaid_AdClickThru;
            if (Vpaid2 != null)
            {
                Vpaid2 = Vpaid as IVpaid2;
                Vpaid2.AdSkippableStateChange += Vpaid2_AdSkippableStateChange;
                Vpaid2.AdDurationChange += Vpaid2_AdDurationChange;
            }
        }

        private void UnwireVpaid()
        {
            Vpaid.AdRemainingTimeChange -= vpaid_AdRemainingTimeChange;
            Vpaid.AdError -= vpaid_AdError;
            Vpaid.AdLoaded -= vpaid_AdLoaded;
            Vpaid.AdStarted -= vpaid_AdStarted;
            Vpaid.AdStopped -= vpaid_AdStopped;
            Vpaid.AdPlaying -= vpaid_AdPlaying;
            Vpaid.AdPaused -= vpaid_AdPaused;
            Vpaid.AdClickThru -= Vpaid_AdClickThru;
            if (Vpaid2 != null)
            {
                Vpaid2.AdSkippableStateChange -= Vpaid2_AdSkippableStateChange;
                Vpaid2.AdDurationChange -= Vpaid2_AdDurationChange;
                Vpaid2 = null;
            }
            Vpaid = null;
        }

        void Vpaid2_AdDurationChange(object sender, object e)
        {
            OnPropertyChanged(() => Duration);
            OnPropertyChanged(() => EndTime);
            OnPropertyChanged(() => TimeRemaining);
        }

        void Vpaid2_AdSkippableStateChange(object sender, object e)
        {
            NotifyIsSkipNextEnabledChanged();
        }

        void Vpaid_AdClickThru(object sender, ClickThroughEventArgs e)
        {
            OnInteracting(InteractionType.Hard);
        }

        void vpaid_AdPaused(object sender, object e)
        {
            state = vPaidState.Paused;
            NotifyIsPlayResumeEnabledChanged();
            NotifyIsPauseEnabledChanged();
            NotifyCurrentStateChanged(new RoutedEventArgs());
        }

        void vpaid_AdPlaying(object sender, object e)
        {
            state = vPaidState.Playing;
            NotifyIsPlayResumeEnabledChanged();
            NotifyIsPauseEnabledChanged();
            NotifyCurrentStateChanged(new RoutedEventArgs());
        }

        void vpaid_AdStopped(object sender, object e)
        {
            state = vPaidState.Completed;
            NotifyIsPlayResumeEnabledChanged();
            NotifyIsPauseEnabledChanged();
            NotifyCurrentStateChanged(new RoutedEventArgs());

            UnwireVpaid();
            UnwireMediaPlayer();
        }

        void vpaid_AdStarted(object sender, object e)
        {
            state = vPaidState.Playing;
            NotifyIsPlayResumeEnabledChanged();
            NotifyIsPauseEnabledChanged();
            NotifyCurrentStateChanged(new RoutedEventArgs());
        }

        void vpaid_AdLoaded(object sender, object e)
        {
            state = vPaidState.Loaded;
            OnPropertyChanged(() => Duration);
            OnPropertyChanged(() => TimeRemaining);
            OnPropertyChanged(() => SignalStrength);
            OnPropertyChanged(() => MediaQuality);
            NotifyCurrentStateChanged(new RoutedEventArgs());
            estimatedDuration = Vpaid.AdRemainingTime;  // used to estimate the duration of the ad for vpaid 1.1
        }

        void vpaid_AdError(object sender, VpaidMessageEventArgs e)
        {
            state = vPaidState.Failure;
            NotifyIsPlayResumeEnabledChanged();
            NotifyIsPauseEnabledChanged();
            NotifyCurrentStateChanged(new RoutedEventArgs());
        }

        void vpaid_AdRemainingTimeChange(object sender, object e)
        {
            OnPropertyChanged(() => TimeRemaining);
            OnPropertyChanged(() => MaxPosition);
            OnPropertyChanged(() => Position);
        }

        /// <inheritdoc /> 
        public void OnInteracting(InteractionType interactionType)
        {
            if (Interacting != null) Interacting(this, new InteractionEventArgs(interactionType));
        }

        /// <inheritdoc /> 
        public void Stop()
        {
            Vpaid.StopAd();
        }

        /// <inheritdoc /> 
        public void Pause()
        {
            Vpaid.PauseAd();
        }

        /// <inheritdoc /> 
        public void InvokeCaptionSelection()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void InvokeAudioSelection()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void SkipPrevious()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void SkipNext()
        {
            if (Vpaid2 != null) Vpaid2.SkipAd();
        }

        /// <inheritdoc /> 
        public void SkipBack()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void SkipAhead()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void Seek(TimeSpan position, out bool canceled)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void CompleteScrub(TimeSpan position, bool canceled, out bool cancel)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void StartScrub(TimeSpan position, out bool canceled)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void Scrub(TimeSpan position, out bool canceled)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void GoLive()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void PlayResume()
        {
            Vpaid.ResumeAd();
        }

        /// <inheritdoc /> 
        public void Replay()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void DecreasePlaybackRate()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc /> 
        public void IncreasePlaybackRate()
        {
            throw new NotImplementedException();
        }

#if SILVERLIGHT
        /// <inheritdoc /> 
        public void CycleDisplayMode()
        {
            throw new NotImplementedException();
        }
#endif

        /// <inheritdoc /> 
        public IEnumerable<ICaption> AvailableCaptions
        {
            get { return Enumerable.Empty<ICaption>(); }
        }

        /// <inheritdoc /> 
        public ICaption SelectedCaption { get; set; }

        /// <inheritdoc /> 
        public IEnumerable<VisualMarker> VisualMarkers
        {
            get { return Enumerable.Empty<VisualMarker>(); }
        }

        /// <inheritdoc /> 
        public bool IsGoLiveEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsPlayResumeEnabled
        {
            get { return state == vPaidState.Paused; }
        }

        /// <inheritdoc /> 
        public bool IsPauseEnabled
        {
            get { return state == vPaidState.Playing; }
        }

        /// <inheritdoc /> 
        public bool IsStopEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsReplayEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsAudioSelectionEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsCaptionSelectionEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsRewindEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsFastForwardEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsSlowMotionEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsSeekEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsSkipPreviousEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsSkipNextEnabled
        {
            get { return Vpaid2 != null ? Vpaid2.AdSkippableState : false; }
        }

        /// <inheritdoc /> 
        public bool IsSkipBackEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsSkipAheadEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsScrubbingEnabled
        {
            get { return false; }
        }

        /// <inheritdoc /> 
        public bool IsMuted
        {
            get { return MediaPlayer.IsMuted; }
            set { MediaPlayer.IsMuted = value; }
        }

        /// <inheritdoc /> 
        public bool IsFullScreen
        {
            get { return MediaPlayer.IsFullScreen; }
            set { MediaPlayer.IsFullScreen = value; }
        }

        /// <inheritdoc /> 
        public bool IsSlowMotion
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        /// <inheritdoc /> 
        public double Volume
        {
            get { return MediaPlayer.Volume; }
            set
            {
                // this will end up setting the vpaid player because it's monitoring the main player
                MediaPlayer.Volume = value;
            }
        }

        /// <inheritdoc /> 
        public double BufferingProgress
        {
            get { return 0; }
        }

        /// <inheritdoc /> 
        public double DownloadProgress
        {
            get { return 1; }
        }

        /// <inheritdoc /> 
        public TimeSpan Duration
        {
            get
            {
                if (Vpaid2 != null)
                {
                    return Vpaid2.AdDuration;
                }
                else
                {
                    return estimatedDuration;   // assume the timeremaining when the ad started is the duration.
                }
            }
        }

        /// <inheritdoc /> 
        public TimeSpan EndTime
        {
            get { return Duration; }
        }

        /// <inheritdoc /> 
        public TimeSpan StartTime
        {
            get { return TimeSpan.Zero; }
        }

        /// <inheritdoc /> 
        public TimeSpan TimeRemaining
        {
            get { return Vpaid.AdRemainingTime; }
        }

        /// <inheritdoc /> 
        public TimeSpan Position
        {
            get { return Duration.Subtract(TimeRemaining); }
        }

        /// <inheritdoc /> 
        public TimeSpan MaxPosition
        {
            get { return Position; }
        }

        /// <inheritdoc /> 
        public MediaElementState CurrentState
        {
            get
            {
                switch (state)
                {
                    case vPaidState.None:
                        return MediaElementState.Closed;
                    case vPaidState.Loaded:
                        return MediaElementState.Opening;
                    case vPaidState.Paused:
                        return MediaElementState.Paused;
                    case vPaidState.Playing:
                        return MediaElementState.Playing;
                    case vPaidState.Failure:
                        return MediaElementState.Closed;
                    case vPaidState.Completed:
                        return MediaElementState.Stopped;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <inheritdoc /> 
        public IEnumerable<IAudioStream> AvailableAudioStreams
        {
            get { return Enumerable.Empty<IAudioStream>(); }
        }

        /// <inheritdoc /> 
        public IAudioStream SelectedAudioStream { get; set; }

        /// <inheritdoc /> 
        public IValueConverter TimeFormatConverter
        {
            get { return new StringFormatConverter() { StringFormat = MediaPlayer.DefaultTimeFormat }; }
        }

        /// <inheritdoc /> 
        public TimeSpan? SkipBackInterval
        {
            get { return TimeSpan.FromSeconds(5); }
        }

        /// <inheritdoc /> 
        public TimeSpan? SkipAheadInterval
        {
            get { return TimeSpan.FromSeconds(5); }
        }

        /// <inheritdoc /> 
        public double SignalStrength
        {
            get { return 1; }
        }

        /// <inheritdoc /> 
        public MediaQuality MediaQuality
        {
            get
            {
                if (Vpaid2 != null)
                {
                    return Vpaid2.AdHeight >= 720 ? MediaQuality.HighDefinition : MediaQuality.StandardDefinition;
                }
                else
                {
                    return MediaPlayer.MediaQuality; // vpaid 1.1 has no way to determine this, fall back on whatever the content of the main video was.
                }
            }
        }





















        #region Events

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler CurrentStateChanged;
#else
        public event EventHandler<object> CurrentStateChanged;
#endif

        /// <summary>
        /// Invokes the CurrentStateChanged event.
        /// </summary>
        /// <param name="e">The event args to pass</param>
        void NotifyCurrentStateChanged(RoutedEventArgs e)
        {
            if (CurrentStateChanged != null) CurrentStateChanged(this, e);
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsGoLiveEnabledChanged;
#else
        public event EventHandler<object> IsGoLiveEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the go live enabled state may have changed.
        /// </summary>
        void NotifyIsGoLiveEnabledChanged()
        {
            OnPropertyChanged(() => IsGoLiveEnabled);
            if (IsGoLiveEnabledChanged != null) IsGoLiveEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsPlayResumeEnabledChanged;
#else
        public event EventHandler<object> IsPlayResumeEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the play resume enabled state may have changed.
        /// </summary>
        void NotifyIsPlayResumeEnabledChanged()
        {
            OnPropertyChanged(() => IsPlayResumeEnabled);
            if (IsPlayResumeEnabledChanged != null) IsPlayResumeEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsPauseEnabledChanged;
#else
        public event EventHandler<object> IsPauseEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the pause enabled state may have changed.
        /// </summary>
        void NotifyIsPauseEnabledChanged()
        {
            OnPropertyChanged(() => IsPauseEnabled);
            if (IsPauseEnabledChanged != null) IsPauseEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsStopEnabledChanged;
#else
        public event EventHandler<object> IsStopEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the stop enabled state may have changed.
        /// </summary>
        void NotifyIsStopEnabledChanged()
        {
            OnPropertyChanged(() => IsStopEnabled);
            if (IsStopEnabledChanged != null) IsStopEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsReplayEnabledChanged;
#else
        public event EventHandler<object> IsReplayEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the replay enabled state may have changed.
        /// </summary>
        void NotifyIsReplayEnabledChanged()
        {
            OnPropertyChanged(() => IsReplayEnabled);
            if (IsReplayEnabledChanged != null) IsReplayEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsAudioSelectionEnabledChanged;
#else
        public event EventHandler<object> IsAudioSelectionEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the audio stream selection enabled state may have changed.
        /// </summary>
        void NotifyIsAudioSelectionEnabledChanged()
        {
            OnPropertyChanged(() => IsAudioSelectionEnabled);
            if (IsAudioSelectionEnabledChanged != null) IsAudioSelectionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsCaptionSelectionEnabledChanged;
#else
        public event EventHandler<object> IsCaptionSelectionEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the audio stream selection enabled state may have changed.
        /// </summary>
        void NotifyIsCaptionSelectionEnabledChanged()
        {
            OnPropertyChanged(() => IsCaptionSelectionEnabled);
            if (IsCaptionSelectionEnabledChanged != null) IsCaptionSelectionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsRewindEnabledChanged;
#else
        public event EventHandler<object> IsRewindEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the rewind enabled state may have changed.
        /// </summary>
        void NotifyIsRewindEnabledChanged()
        {
            OnPropertyChanged(() => IsRewindEnabled);
            if (IsRewindEnabledChanged != null) IsRewindEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsFastForwardEnabledChanged;
#else
        public event EventHandler<object> IsFastForwardEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the fast forward enabled state may have changed.
        /// </summary>
        void NotifyIsFastForwardEnabledChanged()
        {
            OnPropertyChanged(() => IsFastForwardEnabled);
            if (IsFastForwardEnabledChanged != null) IsFastForwardEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSlowMotionEnabledChanged;
#else
        public event EventHandler<object> IsSlowMotionEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the slow motion enabled state may have changed.
        /// </summary>
        void NotifyIsSlowMotionEnabledChanged()
        {
            OnPropertyChanged(() => IsSlowMotionEnabled);
            if (IsSlowMotionEnabledChanged != null) IsSlowMotionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSeekEnabledChanged;
#else
        public event EventHandler<object> IsSeekEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the seek enabled state may have changed.
        /// </summary>
        void NotifyIsSeekEnabledChanged()
        {
            OnPropertyChanged(() => IsSeekEnabled);
            if (IsSeekEnabledChanged != null) IsSeekEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSkipPreviousEnabledChanged;
#else
        public event EventHandler<object> IsSkipPreviousEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the skip Previous enabled state may have changed.
        /// </summary>
        void NotifyIsSkipPreviousEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipPreviousEnabled);
            if (IsSkipPreviousEnabledChanged != null) IsSkipPreviousEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSkipNextEnabledChanged;
#else
        public event EventHandler<object> IsSkipNextEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the skip next enabled state may have changed.
        /// </summary>
        void NotifyIsSkipNextEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipNextEnabled);
            if (IsSkipNextEnabledChanged != null) IsSkipNextEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSkipBackEnabledChanged;
#else
        public event EventHandler<object> IsSkipBackEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the skip back enabled state may have changed.
        /// </summary>
        void NotifyIsSkipBackEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipBackEnabled);
            if (IsSkipBackEnabledChanged != null) IsSkipBackEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsSkipAheadEnabledChanged;
#else
        public event EventHandler<object> IsSkipAheadEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the skip Ahead enabled state may have changed.
        /// </summary>
        void NotifyIsSkipAheadEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipAheadEnabled);
            if (IsSkipAheadEnabledChanged != null) IsSkipAheadEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler IsScrubbingEnabledChanged;
#else
        public event EventHandler<object> IsScrubbingEnabledChanged;
#endif

        /// <summary>
        /// Indicates that the scrubbing enabled state may have changed.
        /// </summary>
        void NotifyIsScrubbingEnabledChanged()
        {
            OnPropertyChanged(() => IsScrubbingEnabled);
            if (IsScrubbingEnabledChanged != null) IsScrubbingEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event EventHandler<SkipRequestedEventArgs> SkipBackRequested;

        /// <inheritdoc /> 
        public event EventHandler<SkipRequestedEventArgs> SkipAheadRequested;

        /// <inheritdoc /> 
        public event EventHandler<SeekRequestedEventArgs> SeekRequested;

        /// <inheritdoc /> 
        public event EventHandler<ScrubStartRequestedEventArgs> ScrubStartRequested;

        /// <inheritdoc /> 
        public event EventHandler<ScrubRequestedEventArgs> ScrubRequested;

        /// <inheritdoc /> 
        public event EventHandler<ScrubCompleteRequestedEventArgs> ScrubCompleteRequested;

        /// <inheritdoc /> 
        public event EventHandler<InteractionEventArgs> Interacting;

        #endregion

        #region PropertyChanged

        /// <inheritdoc /> 
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the property changed event.
        /// </summary>
        /// <param name="PropertyName">The name of the property that changed.</param>
        void OnPropertyChanged(string PropertyName)
        {
            try
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
            catch (NullReferenceException)
            {
                // HACK: This will throw an exception on Win8 CPU2 sometimes.
            }
        }

        /// <summary>
        /// Invokes the property changed event.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="property">A lambda expression returning the property</param>
        void OnPropertyChanged<T>(Expression<Func<T>> property)
        {
            OnPropertyChanged(GetPropertyName(property));
        }

        static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            return (property.Body as MemberExpression).Member.Name;
        }

        #endregion

    }
}
