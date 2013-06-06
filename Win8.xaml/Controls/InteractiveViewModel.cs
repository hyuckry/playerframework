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

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Provides an IInteractiveViewModel implementation for MediaPlayer
    /// </summary>
    public sealed class InteractiveViewModel : IInteractiveViewModel, INotifyPropertyChanged
    {
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// Creates a new instance of InteractiveViewModel
        /// </summary>
        public InteractiveViewModel()
        {
            SkipPreviousThreshold = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Creates a new instance of InteractiveViewModel
        /// </summary>
        /// <param name="mediaPlayer">The mediaplayer instance to adapt.</param>
        public InteractiveViewModel(MediaPlayer mediaPlayer)
            : this()
        {
            MediaPlayer = mediaPlayer;
        }

        /// <summary>
        /// Gets or sets how far away from the previous marker you should be for it to be recognized when skipping previous.
        /// Default is 2 seconds.
        /// </summary>
        public TimeSpan SkipPreviousThreshold { get; set; }

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

        /// <summary>
        /// Invokes the CurrentStateChanged event.
        /// </summary>
        /// <param name="e">The event args to pass</param>
        void OnCurrentStateChanged(RoutedEventArgs e)
        {
            if (CurrentStateChanged != null) CurrentStateChanged(this, e);
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

        #region Properties

        /// <inheritdoc /> 
        public IEnumerable<ICaption> AvailableCaptions
        {
            get { return MediaPlayer.AvailableCaptions; }
        }

        /// <inheritdoc /> 
        public ICaption SelectedCaption
        {
            get { return MediaPlayer.SelectedCaption; }
            set { MediaPlayer.SelectedCaption = value; }
        }

        /// <inheritdoc /> 
        public IEnumerable<IAudioStream> AvailableAudioStreams
        {
            get { return MediaPlayer.AvailableAudioStreams; }
        }

        /// <inheritdoc /> 
        public IAudioStream SelectedAudioStream
        {
            get { return MediaPlayer.SelectedAudioStream; }
            set { MediaPlayer.SelectedAudioStream = value; }
        }

        /// <inheritdoc /> 
        public IEnumerable<VisualMarker> VisualMarkers
        {
            get { return MediaPlayer.VisualMarkers; }
        }

        /// <inheritdoc /> 
        public bool IsGoLiveEnabled
        {
            get { return MediaPlayer.IsGoLiveEnabled && MediaPlayer.IsGoLiveAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsPlayResumeEnabled
        {
            get { return MediaPlayer.IsPlayResumeEnabled && MediaPlayer.IsPlayResumeAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsPauseEnabled
        {
            get { return MediaPlayer.IsPauseEnabled && MediaPlayer.IsPauseAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsStopEnabled
        {
            get { return MediaPlayer.IsStopEnabled && MediaPlayer.IsStopAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsReplayEnabled
        {
            get { return MediaPlayer.IsReplayEnabled && MediaPlayer.IsReplayAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsAudioSelectionEnabled
        {
            get { return MediaPlayer.IsAudioSelectionEnabled && MediaPlayer.IsAudioSelectionAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsCaptionSelectionEnabled
        {
            get { return MediaPlayer.IsCaptionSelectionEnabled && MediaPlayer.IsCaptionSelectionAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsRewindEnabled
        {
            get { return MediaPlayer.IsRewindEnabled && MediaPlayer.IsRewindAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsFastForwardEnabled
        {
            get { return MediaPlayer.IsFastForwardEnabled && MediaPlayer.IsFastForwardAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSlowMotionEnabled
        {
            get { return MediaPlayer.IsSlowMotionEnabled && MediaPlayer.IsSlowMotionAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSeekEnabled
        {
            get { return MediaPlayer.IsSeekEnabled && MediaPlayer.IsSeekAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSkipPreviousEnabled
        {
            get { return MediaPlayer.IsSkipPreviousEnabled && MediaPlayer.IsSkipPreviousAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSkipNextEnabled
        {
            get { return MediaPlayer.IsSkipNextEnabled && MediaPlayer.IsSkipNextAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSkipBackEnabled
        {
            get { return MediaPlayer.IsSkipBackEnabled && MediaPlayer.IsSkipBackAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsSkipAheadEnabled
        {
            get { return MediaPlayer.IsSkipAheadEnabled && MediaPlayer.IsSkipAheadAllowed; }
        }

        /// <inheritdoc /> 
        public bool IsScrubbingEnabled
        {
            get { return MediaPlayer.IsScrubbingEnabled && MediaPlayer.IsScrubbingAllowed; }
        }

        /// <inheritdoc /> 
        public double BufferingProgress
        {
            get { return MediaPlayer.BufferingProgress; }
        }

        /// <inheritdoc /> 
        public double DownloadProgress
        {
            get { return MediaPlayer.DownloadProgress; }
        }

        /// <inheritdoc /> 
        public TimeSpan StartTime
        {
            get { return GetViewModelPosition(MediaPlayer.StartTime); }
        }

        /// <inheritdoc /> 
        public TimeSpan EndTime
        {
            get { return GetViewModelPosition(MediaPlayer.EndTime); }
        }

        /// <inheritdoc /> 
        public TimeSpan Duration
        {
            get { return MediaPlayer.Duration; }
        }

        /// <inheritdoc /> 
        public TimeSpan TimeRemaining
        {
            get { return MediaPlayer.TimeRemaining; }
        }

        /// <inheritdoc /> 
        public TimeSpan Position
        {
            get { return GetViewModelPosition(MediaPlayer.Position); }
        }

        /// <inheritdoc /> 
        public TimeSpan MaxPosition
        {
            get { return GetViewModelPosition(MediaPlayer.LivePosition.GetValueOrDefault(MediaPlayer.EndTime)); }
        }

        /// <inheritdoc /> 
        public MediaElementState CurrentState
        {
            get { return MediaPlayer.CurrentState; }
        }

        /// <inheritdoc /> 
        public IValueConverter TimeFormatConverter
        {
            get { return MediaPlayer.TimeFormatConverter; }
        }

        /// <inheritdoc /> 
        public TimeSpan? SkipBackInterval
        {
            get { return MediaPlayer.SkipBackInterval; }
        }

        /// <inheritdoc /> 
        public TimeSpan? SkipAheadInterval
        {
            get { return MediaPlayer.SkipAheadInterval; }
        }

        /// <inheritdoc /> 
        public double SignalStrength
        {
            get { return MediaPlayer.SignalStrength; }
        }

        /// <inheritdoc /> 
        public MediaQuality MediaQuality
        {
            get { return MediaPlayer.MediaQuality; }
        }

        /// <inheritdoc /> 
        public bool IsMuted
        {
            get
            {
                return MediaPlayer.IsMuted;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                MediaPlayer.IsMuted = value;
            }
        }

        /// <inheritdoc /> 
        public bool IsFullScreen
        {
            get
            {
                return MediaPlayer.IsFullScreen;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                MediaPlayer.IsFullScreen = value;
            }
        }

        /// <inheritdoc /> 
        public bool IsSlowMotion
        {
            get
            {
                return MediaPlayer.IsSlowMotion;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                MediaPlayer.IsSlowMotion = value;
            }
        }

        /// <inheritdoc /> 
        public double Volume
        {
            get
            {
                return MediaPlayer.Volume;
            }
            set
            {
                OnInteracting(InteractionType.Hard);
                MediaPlayer.Volume = value;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc /> 
        public void OnInteracting(InteractionType interactionType)
        {
            if (Interacting != null) Interacting(this, new InteractionEventArgs(interactionType));
        }

        /// <inheritdoc /> 
        public void Stop()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.Stop();
        }

        /// <inheritdoc /> 
        public void Pause()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.Pause();
        }

        /// <inheritdoc /> 
        public void InvokeCaptionSelection()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.InvokeCaptionSelection();
        }

        /// <inheritdoc /> 
        public void InvokeAudioSelection()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.InvokeAudioSelection();
        }

        /// <inheritdoc /> 
        public void SkipPrevious()
        {
            OnInteracting(InteractionType.Hard);

            VisualMarker marker = VisualMarkers
                .Where(m => m.IsSeekable && m.Time.Add(SkipPreviousThreshold) < Position && m.Time < MaxPosition)
                .OrderByDescending(m => m.Time).FirstOrDefault();

            if (SkipBackRequested != null) SkipBackRequested(this, new SkipRequestedEventArgs(marker != null ? GetMediaPlayerPosition(marker.Time) : MediaPlayer.StartTime));
        }

        /// <inheritdoc /> 
        public void SkipNext()
        {
            OnInteracting(InteractionType.Hard);

            VisualMarker marker = VisualMarkers
                .Where(m => m.IsSeekable && m.Time > Position && m.Time < MaxPosition)
                .OrderBy(m => m.Time).FirstOrDefault();

            if (SkipAheadRequested != null) SkipAheadRequested(this, new SkipRequestedEventArgs(marker != null ? GetMediaPlayerPosition(marker.Time) : MediaPlayer.LivePosition.GetValueOrDefault(MediaPlayer.EndTime)));
        }

        /// <inheritdoc /> 
        public void SkipBack()
        {
            OnInteracting(InteractionType.Hard);
            TimeSpan position = SkipBackInterval.HasValue ? TimeSpanExtensions.Max(Position.Subtract(SkipBackInterval.Value), StartTime) : StartTime;

            if (SkipBackRequested != null) SkipBackRequested(this, new SkipRequestedEventArgs(GetMediaPlayerPosition(position)));
        }

        /// <inheritdoc /> 
        public void SkipAhead()
        {
            OnInteracting(InteractionType.Hard);
            TimeSpan position = SkipAheadInterval.HasValue ? TimeSpanExtensions.Min(Position.Add(SkipAheadInterval.Value), MaxPosition) : MaxPosition;

            if (SkipAheadRequested != null) SkipAheadRequested(this, new SkipRequestedEventArgs(GetMediaPlayerPosition(position)));
        }

        /// <inheritdoc /> 
        public void Seek(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);

            var args = new SeekRequestedEventArgs(GetMediaPlayerPosition(position));
            if (SeekRequested != null) SeekRequested(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        public void CompleteScrub(TimeSpan position, bool canceled, out bool cancel)
        {
            OnInteracting(InteractionType.Hard);

            var args = new ScrubCompleteRequestedEventArgs(GetMediaPlayerPosition(position), canceled);
            if (ScrubCompleteRequested != null) ScrubCompleteRequested(this, args);
            cancel = canceled || args.Cancel;
        }

        /// <inheritdoc /> 
        public void StartScrub(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);

            var args = new ScrubStartRequestedEventArgs(GetMediaPlayerPosition(position));
            if (ScrubStartRequested != null) ScrubStartRequested(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        public void Scrub(TimeSpan position, out bool canceled)
        {
            OnInteracting(InteractionType.Hard);

            var args = new ScrubRequestedEventArgs(GetMediaPlayerPosition(position));
            if (ScrubRequested != null) ScrubRequested(this, args);
            canceled = args.Cancel;
        }

        /// <inheritdoc /> 
        public void GoLive()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.SeekToLive();
        }

        /// <inheritdoc /> 
        public void PlayResume()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.PlayResume();
        }

        /// <inheritdoc /> 
        public void Replay()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.Replay();
        }

        /// <inheritdoc /> 
        public void DecreasePlaybackRate()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.DecreasePlaybackRate();
        }

        /// <inheritdoc /> 
        public void IncreasePlaybackRate()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.IncreasePlaybackRate();
        }

#if SILVERLIGHT
        /// <inheritdoc /> 
        public void CycleDisplayMode()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.CycleDisplayMode();
        }
#endif
        #endregion

        #region Events

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
            if (IsGoLiveEnabledChanged != null) IsGoLiveEnabledChanged(this, EventArgs.Empty);
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
            if (IsPlayResumeEnabledChanged != null) IsPlayResumeEnabledChanged(this, EventArgs.Empty);
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
            if (IsPauseEnabledChanged != null) IsPauseEnabledChanged(this, EventArgs.Empty);
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
            if (IsStopEnabledChanged != null) IsStopEnabledChanged(this, EventArgs.Empty);
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
            if (IsReplayEnabledChanged != null) IsReplayEnabledChanged(this, EventArgs.Empty);
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
            if (IsAudioSelectionEnabledChanged != null) IsAudioSelectionEnabledChanged(this, EventArgs.Empty);
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
            if (IsCaptionSelectionEnabledChanged != null) IsCaptionSelectionEnabledChanged(this, EventArgs.Empty);
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
            if (IsRewindEnabledChanged != null) IsRewindEnabledChanged(this, EventArgs.Empty);
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
            if (IsFastForwardEnabledChanged != null) IsFastForwardEnabledChanged(this, EventArgs.Empty);
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
            if (IsSlowMotionEnabledChanged != null) IsSlowMotionEnabledChanged(this, EventArgs.Empty);
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
            if (IsSeekEnabledChanged != null) IsSeekEnabledChanged(this, EventArgs.Empty);
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
            if (IsSkipPreviousEnabledChanged != null) IsSkipPreviousEnabledChanged(this, EventArgs.Empty);
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
            if (IsSkipNextEnabledChanged != null) IsSkipNextEnabledChanged(this, EventArgs.Empty);
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
            if (IsSkipBackEnabledChanged != null) IsSkipBackEnabledChanged(this, EventArgs.Empty);
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
            if (IsSkipAheadEnabledChanged != null) IsSkipAheadEnabledChanged(this, EventArgs.Empty);
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
            if (IsScrubbingEnabledChanged != null) IsScrubbingEnabledChanged(this, EventArgs.Empty);
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler CurrentStateChanged;
#else
        public event EventHandler<object> CurrentStateChanged;
#endif

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
        /// <param name="propertyName">The name of the property that changed.</param>
        void OnPropertyChanged(string propertyName)
        {
            try
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
