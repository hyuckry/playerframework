using System;
using System.Net;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
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
    /// Provides an IInteractiveViewModel implementation for MediaPlayer
    /// </summary>
    public partial class VpaidNonLinearAdViewModel : IInteractiveViewModel, INotifyPropertyChanged
    {
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// The MediaPlayer instance the ViewModel is wrapping
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set
            {
                if (mediaPlayer != null)
                {
                    UnwireMediaPlayer();
                }
                mediaPlayer = value;
                if (mediaPlayer != null)
                {
                    WireMediaPlayer();
                }
            }
        }

        private void UnwireMediaPlayer()
        {
            // this is meant to exist for the lifetime of the MediaPlayer.
            throw new NotImplementedException();
        }

        private void WireMediaPlayer()
        {
            MediaPlayer.IsPlayResumeEnabledChanged += (s, e) => NotifyIsPlayResumeEnabledChanged();
            MediaPlayer.IsPauseEnabledChanged += (s, e) => NotifyIsPauseEnabledChanged();
            MediaPlayer.IsStopEnabledChanged += (s, e) => NotifyIsStopEnabledChanged();
            MediaPlayer.IsReplayEnabledChanged += (s, e) => NotifyIsReplayEnabledChanged();
            MediaPlayer.IsAudioSelectionEnabledChanged += (s, e) => NotifyIsAudioSelectionEnabledChanged();
            MediaPlayer.IsCaptionSelectionEnabledChanged += (s, e) => NotifyIsCaptionSelectionEnabledChanged();
            MediaPlayer.IsRewindEnabledChanged += (s, e) => NotifyIsRewindEnabledChanged();
            MediaPlayer.IsFastForwardEnabledChanged += (s, e) => NotifyIsFastForwardEnabledChanged();
            MediaPlayer.IsSlowMotionEnabledChanged += (s, e) => NotifyIsSlowMotionEnabledChanged();
            MediaPlayer.IsSeekEnabledChanged += (s, e) => NotifyIsSeekEnabledChanged();
            MediaPlayer.IsSkipPreviousEnabledChanged += (s, e) => NotifyIsSkipPreviousEnabledChanged();
            MediaPlayer.IsSkipNextEnabledChanged += (s, e) => NotifyIsSkipNextEnabledChanged();
            MediaPlayer.IsSkipBackEnabledChanged += (s, e) => NotifyIsSkipBackEnabledChanged();
            MediaPlayer.IsSkipAheadEnabledChanged += (s, e) => NotifyIsSkipAheadEnabledChanged();
            MediaPlayer.IsScrubbingEnabledChanged += (s, e) => NotifyIsScrubbingEnabledChanged();
            MediaPlayer.IsGoLiveEnabledChanged += (s, e) => NotifyIsGoLiveEnabledChanged();

            MediaPlayer.IsPlayResumeAllowedChanged += (s, e) => NotifyIsPlayResumeEnabledChanged();
            MediaPlayer.IsPauseAllowedChanged += (s, e) => NotifyIsPauseEnabledChanged();
            MediaPlayer.IsStopAllowedChanged += (s, e) => NotifyIsStopEnabledChanged();
            MediaPlayer.IsReplayAllowedChanged += (s, e) => NotifyIsReplayEnabledChanged();
            MediaPlayer.IsAudioSelectionAllowedChanged += (s, e) => NotifyIsAudioSelectionEnabledChanged();
            MediaPlayer.IsCaptionSelectionAllowedChanged += (s, e) => NotifyIsCaptionSelectionEnabledChanged();
            MediaPlayer.IsRewindAllowedChanged += (s, e) => NotifyIsRewindEnabledChanged();
            MediaPlayer.IsFastForwardAllowedChanged += (s, e) => NotifyIsFastForwardEnabledChanged();
            MediaPlayer.IsSlowMotionAllowedChanged += (s, e) => NotifyIsSlowMotionEnabledChanged();
            MediaPlayer.IsSeekAllowedChanged += (s, e) => NotifyIsSeekEnabledChanged();
            MediaPlayer.IsSkipPreviousAllowedChanged += (s, e) => NotifyIsSkipPreviousEnabledChanged();
            MediaPlayer.IsSkipNextAllowedChanged += (s, e) => NotifyIsSkipNextEnabledChanged();
            MediaPlayer.IsSkipBackAllowedChanged += (s, e) => NotifyIsSkipBackEnabledChanged();
            MediaPlayer.IsSkipAheadAllowedChanged += (s, e) => NotifyIsSkipAheadEnabledChanged();
            MediaPlayer.IsScrubbingAllowedChanged += (s, e) => NotifyIsScrubbingEnabledChanged();
            MediaPlayer.IsGoLiveAllowedChanged += (s, e) => NotifyIsGoLiveEnabledChanged();

            MediaPlayer.IsMutedChanged += (s, e) => OnPropertyChanged(() => IsMuted);
            MediaPlayer.IsFullScreenChanged += (s, e) => OnPropertyChanged(() => IsFullScreen);
            MediaPlayer.IsSlowMotionChanged += (s, e) => OnPropertyChanged(() => IsSlowMotion);
            MediaPlayer.CurrentStateChanged += (s, e) => OnCurrentStateChanged(e);
            MediaPlayer.BufferingProgressChanged += (s, e) => OnPropertyChanged(() => BufferingProgress);
            MediaPlayer.DownloadProgressChanged += (s, e) => OnPropertyChanged(() => DownloadProgress);
            MediaPlayer.VolumeChanged += (s, e) => OnPropertyChanged(() => Volume);
            MediaPlayer.StartTimeChanged += (s, e) => OnPropertyChanged(() => StartTime);
            MediaPlayer.EndTimeChanged += (s, e) => OnPropertyChanged(() => EndTime);
            MediaPlayer.EndTimeChanged += (s, e) => OnPropertyChanged(() => MaxPosition);
            MediaPlayer.DurationChanged += (s, e) => OnPropertyChanged(() => Duration);
            MediaPlayer.TimeRemainingChanged += (s, e) => OnPropertyChanged(() => TimeRemaining);
            MediaPlayer.LivePositionChanged += (s, e) => OnPropertyChanged(() => MaxPosition);
            MediaPlayer.TimeFormatConverterChanged += (s, e) => OnPropertyChanged(() => TimeFormatConverter);
            MediaPlayer.SkipBackIntervalChanged += (s, e) => OnPropertyChanged(() => SkipBackInterval);
            MediaPlayer.SkipAheadIntervalChanged += (s, e) => OnPropertyChanged(() => SkipAheadInterval);
            MediaPlayer.PositionChanged += (s, e) => OnPropertyChanged(() => Position);
            MediaPlayer.SignalStrengthChanged += (s, e) => OnPropertyChanged(() => SignalStrength);
            MediaPlayer.MediaQualityChanged += (s, e) => OnPropertyChanged(() => MediaQuality);
        }

        #region Methods

        /// <inheritdoc /> 
        void OnStop()
        {
            MediaPlayer.Stop();
        }

        void _OnPause()
        {
            MediaPlayer.Pause();
        }

        /// <inheritdoc /> 
        void OnInvokeCaptionSelection()
        {
            MediaPlayer.InvokeCaptionSelection();
        }

        /// <inheritdoc /> 
        void OnInvokeAudioSelection()
        {
            MediaPlayer.InvokeAudioSelection();
        }

        /// <inheritdoc /> 
        void OnSkipPrevious(VisualMarker marker)
        {
            if (SkippingBack != null) SkippingBack(this, new SkippingEventArgs(marker != null ? GetMediaPlayerPosition(marker.Time) : MediaPlayer.StartTime));
        }

        /// <inheritdoc /> 
        void OnSkipNext(VisualMarker marker)
        {
            if (SkippingAhead != null) SkippingAhead(this, new SkippingEventArgs(marker != null ? GetMediaPlayerPosition(marker.Time) : MediaPlayer.LivePosition.GetValueOrDefault(MediaPlayer.EndTime)));
        }

        /// <inheritdoc /> 
        void OnSkipBack(TimeSpan position)
        {
            if (SkippingBack != null) SkippingBack(this, new SkippingEventArgs(GetMediaPlayerPosition(position)));
        }

        /// <inheritdoc /> 
        void OnSkipAhead(TimeSpan position)
        {
            if (SkippingAhead != null) SkippingAhead(this, new SkippingEventArgs(GetMediaPlayerPosition(position)));
        }

        /// <inheritdoc /> 
        void OnSeek(TimeSpan position, out bool canceled)
        {
            var args = new SeekingEventArgs(GetMediaPlayerPosition(position));
            if (Seeking != null) Seeking(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        void OnCompleteScrub(TimeSpan position, bool canceled, out bool cancel)
        {
            var args = new CompletingScrubEventArgs(GetMediaPlayerPosition(position), canceled);
            if (CompletingScrub != null) CompletingScrub(this, args);
            cancel = canceled || args.Cancel;
        }

        /// <inheritdoc /> 
        void OnStartScrub(TimeSpan position, out bool canceled)
        {
            var args = new StartingScrubEventArgs(GetMediaPlayerPosition(position));
            if (StartingScrub != null) StartingScrub(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        void OnScrub(TimeSpan position, out bool canceled)
        {
            var args = new ScrubbingEventArgs(GetMediaPlayerPosition(position));
            if (Scrubbing != null) Scrubbing(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        void OnGoLive()
        {
            MediaPlayer.SeekToLive();
        }

        void _OnPlayResume()
        {
            MediaPlayer.PlayResume();
        }

        /// <inheritdoc /> 
        void OnReplay()
        {
            MediaPlayer.Replay();
        }

        /// <inheritdoc /> 
        void OnDecreasePlaybackRate()
        {
            MediaPlayer.DecreasePlaybackRate();
        }

        /// <inheritdoc /> 
        void OnIncreasePlaybackRate()
        {
            MediaPlayer.IncreasePlaybackRate();
        }

        /// <summary>
        /// Converts the position of the view model to that of the media player
        /// </summary>
        /// <param name="viewModelPosition">The view model's position</param>
        /// <returns>The media player's position</returns>
        TimeSpan GetMediaPlayerPosition(TimeSpan viewModelPosition)
        {
            return MediaPlayer.IsStartTimeOffset ? viewModelPosition : MediaPlayer.StartTime.Add(viewModelPosition);
        }

        /// <summary>
        /// Converts the position of the media player to that of the view model
        /// </summary>
        /// <param name="mediaPlayerPosition">The media player's position</param>
        /// <returns>The view model's position</returns>
        TimeSpan GetViewModelPosition(TimeSpan mediaPlayerPosition)
        {
            return MediaPlayer.IsStartTimeOffset ? mediaPlayerPosition : mediaPlayerPosition.Subtract(MediaPlayer.StartTime);
        }

        TimeSpan? GetViewModelPosition(TimeSpan? mediaPlayerPosition)
        {
            return mediaPlayerPosition.HasValue ? GetViewModelPosition(mediaPlayerPosition.Value) : mediaPlayerPosition;
        }

#if SILVERLIGHT
        /// <inheritdoc /> 
        void OnCycleDisplayMode()
        {
            MediaPlayer.CycleDisplayMode();
        }
#endif
        #endregion

        #region AvailableCaptions
        /// <inheritdoc /> 
        public IEnumerable<ICaption> AvailableCaptions
        {
            get { return MediaPlayer.AvailableCaptions; }
        }
        #endregion

        #region SelectedCaption
        /// <inheritdoc /> 
        public ICaption SelectedCaption
        {
            get { return MediaPlayer.SelectedCaption; }
            set { MediaPlayer.SelectedCaption = value; }
        }
        #endregion

        #region AvailableAudioStreams
        /// <inheritdoc /> 
        public IEnumerable<IAudioStream> AvailableAudioStreams
        {
            get { return MediaPlayer.AvailableAudioStreams; }
        }
        #endregion

        #region SelectedAudioStream
        /// <inheritdoc /> 
        public IAudioStream SelectedAudioStream
        {
            get { return MediaPlayer.SelectedAudioStream; }
            set { MediaPlayer.SelectedAudioStream = value; }
        }
        #endregion

        #region VisualMarkers
        /// <inheritdoc /> 
        public IEnumerable<VisualMarker> VisualMarkers
        {
            get { return MediaPlayer.VisualMarkers; }
        }
        #endregion

        #region IsGoLiveEnabled

        /// <inheritdoc /> 
        public bool IsGoLiveEnabled
        {
            get { return MediaPlayer.IsGoLiveEnabled && MediaPlayer.IsGoLiveAllowed; }
        }
        #endregion

        #region IsPlayResumeEnabled

        /// <inheritdoc /> 
        public bool IsPlayResumeEnabled
        {
            get { return MediaPlayer.IsPlayResumeEnabled && MediaPlayer.IsPlayResumeAllowed; }
        }
        #endregion

        #region IsPauseEnabled

        /// <inheritdoc /> 
        public bool IsPauseEnabled
        {
            get { return MediaPlayer.IsPauseEnabled && MediaPlayer.IsPauseAllowed; }
        }

        #endregion

        #region IsStopEnabled

        /// <inheritdoc /> 
        public bool IsStopEnabled
        {
            get { return MediaPlayer.IsStopEnabled && MediaPlayer.IsStopAllowed; }
        }

        #endregion

        #region IsReplayEnabled

        /// <inheritdoc /> 
        public bool IsReplayEnabled
        {
            get { return MediaPlayer.IsReplayEnabled && MediaPlayer.IsReplayAllowed; }
        }

        #endregion

        #region IsAudioSelectionEnabled

        /// <inheritdoc /> 
        public bool IsAudioSelectionEnabled
        {
            get { return MediaPlayer.IsAudioSelectionEnabled && MediaPlayer.IsAudioSelectionAllowed; }
        }

        #endregion

        #region IsCaptionSelectionEnabled

        /// <inheritdoc /> 
        public bool IsCaptionSelectionEnabled
        {
            get { return MediaPlayer.IsCaptionSelectionEnabled && MediaPlayer.IsCaptionSelectionAllowed; }
        }

        #endregion

        #region IsRewindEnabled

        /// <inheritdoc /> 
        public bool IsRewindEnabled
        {
            get { return MediaPlayer.IsRewindEnabled && MediaPlayer.IsRewindAllowed; }
        }

        #endregion

        #region IsFastForwardEnabled

        /// <inheritdoc /> 
        public bool IsFastForwardEnabled
        {
            get { return MediaPlayer.IsFastForwardEnabled && MediaPlayer.IsFastForwardAllowed; }
        }

        #endregion

        #region IsSlowMotionEnabled

        /// <inheritdoc /> 
        public bool IsSlowMotionEnabled
        {
            get { return MediaPlayer.IsSlowMotionEnabled && MediaPlayer.IsSlowMotionAllowed; }
        }

        #endregion

        #region IsSeekEnabled

        /// <inheritdoc /> 
        public bool IsSeekEnabled
        {
            get { return MediaPlayer.IsSeekEnabled && MediaPlayer.IsSeekAllowed; }
        }

        #endregion

        #region IsSkipPreviousEnabled

        /// <inheritdoc /> 
        public bool IsSkipPreviousEnabled
        {
            get { return MediaPlayer.IsSkipPreviousEnabled && MediaPlayer.IsSkipPreviousAllowed; }
        }

        #endregion

        #region IsSkipNextEnabled

        /// <inheritdoc /> 
        public bool IsSkipNextEnabled
        {
            get
            {
                return MediaPlayer.IsSkipNextEnabled && MediaPlayer.IsSkipNextAllowed;
            }
        }

        #endregion

        #region IsSkipBackEnabled

        /// <inheritdoc /> 
        public bool IsSkipBackEnabled
        {
            get { return MediaPlayer.IsSkipBackEnabled && MediaPlayer.IsSkipBackAllowed; }
        }

        #endregion

        #region IsSkipAheadEnabled

        /// <inheritdoc /> 
        public bool IsSkipAheadEnabled
        {
            get
            {
                return MediaPlayer.IsSkipAheadEnabled && MediaPlayer.IsSkipAheadAllowed;
            }
        }

        #endregion

        #region IsScrubbingEnabled

        /// <inheritdoc /> 
        public bool IsScrubbingEnabled
        {
            get
            {
                return MediaPlayer.IsScrubbingEnabled && MediaPlayer.IsScrubbingAllowed;
            }
        }

        #endregion

        /// <inheritdoc /> 
        bool _IsMuted
        {
            get { return MediaPlayer.IsMuted; }
            set { mediaPlayer.IsMuted = value; }
        }
        /// <inheritdoc /> 
        bool _IsFullScreen
        {
            get { return MediaPlayer.IsFullScreen; }
            set { mediaPlayer.IsFullScreen = value; }
        }
        /// <inheritdoc /> 
        bool _IsSlowMotion
        {
            get { return MediaPlayer.IsSlowMotion; }
            set { mediaPlayer.IsSlowMotion = value; }
        }
        /// <inheritdoc /> 
        double _Volume
        {
            get { return MediaPlayer.Volume; }
            set { mediaPlayer.Volume = value; }
        }
        /// <inheritdoc /> 
        public double BufferingProgress { get { return MediaPlayer.BufferingProgress; } }
        /// <inheritdoc /> 
        public double DownloadProgress { get { return MediaPlayer.DownloadProgress; } }
        /// <inheritdoc /> 
        public TimeSpan StartTime { get { return GetViewModelPosition(MediaPlayer.StartTime); } }
        /// <inheritdoc /> 
        public TimeSpan EndTime { get { return GetViewModelPosition(MediaPlayer.EndTime); } }
        /// <inheritdoc /> 
        public TimeSpan Duration { get { return MediaPlayer.Duration; } }
        /// <inheritdoc /> 
        public TimeSpan TimeRemaining { get { return MediaPlayer.TimeRemaining; } }
        /// <inheritdoc /> 
        public TimeSpan Position { get { return GetViewModelPosition(MediaPlayer.Position); } }
        /// <inheritdoc /> 
        public TimeSpan MaxPosition { get { return GetViewModelPosition(MediaPlayer.LivePosition.GetValueOrDefault(MediaPlayer.EndTime)); } }
        /// <inheritdoc /> 
        public MediaElementState CurrentState { get { return MediaPlayer.CurrentState; } }
        /// <inheritdoc /> 
        public IValueConverter TimeFormatConverter { get { return MediaPlayer.TimeFormatConverter; } }
        /// <inheritdoc /> 
        public TimeSpan? SkipBackInterval { get { return MediaPlayer.SkipBackInterval; } }
        /// <inheritdoc /> 
        public TimeSpan? SkipAheadInterval { get { return MediaPlayer.SkipAheadInterval; } }
        /// <inheritdoc /> 
        public double SignalStrength { get { return MediaPlayer.SignalStrength; } }
        /// <inheritdoc /> 
        public MediaQuality MediaQuality { get { return MediaPlayer.MediaQuality; } }


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


        /// <summary>
        /// Gets or sets how far away from the previous marker you should be for it to be recognized when skipping previous.
        /// Default is 2 seconds.
        /// </summary>
        public TimeSpan SkipPreviousThreshold { get; set; }

        /// <inheritdoc /> 
        public void OnInteracting(InteractionType interactionType)
        {
            if (Interacting != null) Interacting(this, new InteractionEventArgs(interactionType));
        }

        /// <summary>
        /// Invokes the CurrentStateChanged event.
        /// </summary>
        /// <param name="e">The event args to pass</param>
        void OnCurrentStateChanged(RoutedEventArgs e)
        {
            if (CurrentStateChanged != null) CurrentStateChanged(this, e);
        }

        /// <inheritdoc /> 
        public event EventHandler<InteractionEventArgs> Interacting;

        #region Methods

        /// <inheritdoc /> 
        public void Stop()
        {
            OnInteracting(InteractionType.Hard);
            OnStop();
        }

        /// <inheritdoc /> 
        public void Pause()
        {
            OnInteracting(InteractionType.Hard);
            OnPause();
        }

        /// <inheritdoc /> 
        public void InvokeCaptionSelection()
        {
            OnInteracting(InteractionType.Hard);
            OnInvokeCaptionSelection();
        }

        /// <inheritdoc /> 
        public void InvokeAudioSelection()
        {
            OnInteracting(InteractionType.Hard);
            OnInvokeAudioSelection();
        }

        /// <inheritdoc /> 
        public void SkipPrevious()
        {
            OnInteracting(InteractionType.Hard);

            VisualMarker marker = VisualMarkers
                .Where(m => m.IsSeekable && m.Time.Add(SkipPreviousThreshold) < Position && m.Time < MaxPosition)
                .OrderByDescending(m => m.Time).FirstOrDefault();

            OnSkipPrevious(marker);
        }

        /// <inheritdoc /> 
        public void SkipNext()
        {
            OnInteracting(InteractionType.Hard);

            VisualMarker marker = VisualMarkers
                .Where(m => m.IsSeekable && m.Time > Position && m.Time < MaxPosition)
                .OrderBy(m => m.Time).FirstOrDefault();

            OnSkipNext(marker);
        }

        /// <inheritdoc /> 
        public void SkipBack()
        {
            OnInteracting(InteractionType.Hard);
            TimeSpan position = SkipBackInterval.HasValue ? TimeSpanExtensions.Max(Position.Subtract(SkipBackInterval.Value), StartTime) : StartTime;
            OnSkipBack(position);
        }

        /// <inheritdoc /> 
        public void SkipAhead()
        {
            OnInteracting(InteractionType.Hard);
            TimeSpan position = SkipAheadInterval.HasValue ? TimeSpanExtensions.Min(Position.Add(SkipAheadInterval.Value), MaxPosition) : MaxPosition;
            OnSkipAhead(position);
        }

        /// <inheritdoc /> 
        public void Seek(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);
            OnSeek(position, out canceled);
        }

        /// <inheritdoc /> 
        public void CompleteScrub(TimeSpan position, bool canceled, out bool cancel)
        {
            OnInteracting(InteractionType.Hard);
            OnCompleteScrub(position, canceled, out cancel);
        }

        /// <inheritdoc /> 
        public void StartScrub(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);
            OnStartScrub(position, out canceled);
        }

        /// <inheritdoc /> 
        public void Scrub(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);
            OnScrub(position, out canceled);
        }

        /// <inheritdoc /> 
        public void GoLive()
        {
            OnInteracting(InteractionType.Hard);
            OnGoLive();
        }

        /// <inheritdoc /> 
        public void PlayResume()
        {
            OnInteracting(InteractionType.Hard);
            OnPlayResume();
        }

        /// <inheritdoc /> 
        public void Replay()
        {
            OnInteracting(InteractionType.Hard);
            OnReplay();
        }

        /// <inheritdoc /> 
        public void DecreasePlaybackRate()
        {
            OnInteracting(InteractionType.Hard);
            OnDecreasePlaybackRate();
        }

        /// <inheritdoc /> 
        public void IncreasePlaybackRate()
        {
            OnInteracting(InteractionType.Hard);
            OnIncreasePlaybackRate();
        }

#if SILVERLIGHT
        /// <inheritdoc /> 
        public void CycleDisplayMode()
        {
            OnInteracting(InteractionType.Hard);
            OnCycleDisplayMode();
        }

        /// <summary>
        /// Notifies the subclass that the user has chosen the cycle display mode feature to change the stretch state of the player.
        /// </summary>
        abstract void OnCycleDisplayMode();
#endif
        #endregion

        /// <inheritdoc /> 
        public bool IsMuted
        {
            get
            {
                return this._IsMuted;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                this._IsMuted = value;
            }
        }

        /// <inheritdoc /> 
        public bool IsFullScreen
        {
            get
            {
                return this._IsFullScreen;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                this._IsFullScreen = value;
            }
        }

        /// <inheritdoc /> 
        public bool IsSlowMotion
        {
            get
            {
                return this._IsSlowMotion;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                this._IsSlowMotion = value;
            }
        }

        /// <inheritdoc /> 
        public double Volume
        {
            get
            {
                return this._Volume;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                this._Volume = value;
            }
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsGoLiveEnabledChanged;

        /// <summary>
        /// Indicates that the go live enabled state may have changed.
        /// </summary>
        void NotifyIsGoLiveEnabledChanged()
        {
            OnPropertyChanged(() => IsGoLiveEnabled);
            if (IsGoLiveEnabledChanged != null) IsGoLiveEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsPlayResumeEnabledChanged;

        /// <summary>
        /// Indicates that the play resume enabled state may have changed.
        /// </summary>
        void NotifyIsPlayResumeEnabledChanged()
        {
            OnPropertyChanged(() => IsPlayResumeEnabled);
            if (IsPlayResumeEnabledChanged != null) IsPlayResumeEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsPauseEnabledChanged;

        /// <summary>
        /// Indicates that the pause enabled state may have changed.
        /// </summary>
        void NotifyIsPauseEnabledChanged()
        {
            OnPropertyChanged(() => IsPauseEnabled);
            if (IsPauseEnabledChanged != null) IsPauseEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsStopEnabledChanged;

        /// <summary>
        /// Indicates that the stop enabled state may have changed.
        /// </summary>
        void NotifyIsStopEnabledChanged()
        {
            OnPropertyChanged(() => IsStopEnabled);
            if (IsStopEnabledChanged != null) IsStopEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsReplayEnabledChanged;

        /// <summary>
        /// Indicates that the replay enabled state may have changed.
        /// </summary>
        void NotifyIsReplayEnabledChanged()
        {
            OnPropertyChanged(() => IsReplayEnabled);
            if (IsReplayEnabledChanged != null) IsReplayEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsAudioSelectionEnabledChanged;

        /// <summary>
        /// Indicates that the audio stream selection enabled state may have changed.
        /// </summary>
        void NotifyIsAudioSelectionEnabledChanged()
        {
            OnPropertyChanged(() => IsAudioSelectionEnabled);
            if (IsAudioSelectionEnabledChanged != null) IsAudioSelectionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsCaptionSelectionEnabledChanged;

        /// <summary>
        /// Indicates that the audio stream selection enabled state may have changed.
        /// </summary>
        void NotifyIsCaptionSelectionEnabledChanged()
        {
            OnPropertyChanged(() => IsCaptionSelectionEnabled);
            if (IsCaptionSelectionEnabledChanged != null) IsCaptionSelectionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsRewindEnabledChanged;

        /// <summary>
        /// Indicates that the rewind enabled state may have changed.
        /// </summary>
        void NotifyIsRewindEnabledChanged()
        {
            OnPropertyChanged(() => IsRewindEnabled);
            if (IsRewindEnabledChanged != null) IsRewindEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsFastForwardEnabledChanged;

        /// <summary>
        /// Indicates that the fast forward enabled state may have changed.
        /// </summary>
        void NotifyIsFastForwardEnabledChanged()
        {
            OnPropertyChanged(() => IsFastForwardEnabled);
            if (IsFastForwardEnabledChanged != null) IsFastForwardEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSlowMotionEnabledChanged;

        /// <summary>
        /// Indicates that the slow motion enabled state may have changed.
        /// </summary>
        void NotifyIsSlowMotionEnabledChanged()
        {
            OnPropertyChanged(() => IsSlowMotionEnabled);
            if (IsSlowMotionEnabledChanged != null) IsSlowMotionEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSeekEnabledChanged;

        /// <summary>
        /// Indicates that the seek enabled state may have changed.
        /// </summary>
        void NotifyIsSeekEnabledChanged()
        {
            OnPropertyChanged(() => IsSeekEnabled);
            if (IsSeekEnabledChanged != null) IsSeekEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSkipPreviousEnabledChanged;

        /// <summary>
        /// Indicates that the skip Previous enabled state may have changed.
        /// </summary>
        void NotifyIsSkipPreviousEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipPreviousEnabled);
            if (IsSkipPreviousEnabledChanged != null) IsSkipPreviousEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSkipNextEnabledChanged;

        /// <summary>
        /// Indicates that the skip next enabled state may have changed.
        /// </summary>
        void NotifyIsSkipNextEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipNextEnabled);
            if (IsSkipNextEnabledChanged != null) IsSkipNextEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSkipBackEnabledChanged;

        /// <summary>
        /// Indicates that the skip back enabled state may have changed.
        /// </summary>
        void NotifyIsSkipBackEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipBackEnabled);
            if (IsSkipBackEnabledChanged != null) IsSkipBackEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsSkipAheadEnabledChanged;

        /// <summary>
        /// Indicates that the skip Ahead enabled state may have changed.
        /// </summary>
        void NotifyIsSkipAheadEnabledChanged()
        {
            OnPropertyChanged(() => IsSkipAheadEnabled);
            if (IsSkipAheadEnabledChanged != null) IsSkipAheadEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler IsScrubbingEnabledChanged;

        /// <summary>
        /// Indicates that the scrubbing enabled state may have changed.
        /// </summary>
        void NotifyIsScrubbingEnabledChanged()
        {
            OnPropertyChanged(() => IsScrubbingEnabled);
            if (IsScrubbingEnabledChanged != null) IsScrubbingEnabledChanged(this, new RoutedEventArgs());
        }

        /// <inheritdoc /> 
        public event RoutedEventHandler CurrentStateChanged;

        /// <inheritdoc /> 
        public event EventHandler<SkippingEventArgs> SkippingBack;

        /// <inheritdoc /> 
        public event EventHandler<SkippingEventArgs> SkippingAhead;

        /// <inheritdoc /> 
        public event EventHandler<SeekingEventArgs> Seeking;

        /// <inheritdoc /> 
        public event EventHandler<StartingScrubEventArgs> StartingScrub;

        /// <inheritdoc /> 
        public event EventHandler<ScrubbingEventArgs> Scrubbing;

        /// <inheritdoc /> 
        public event EventHandler<CompletingScrubEventArgs> CompletingScrub;
    }
}
