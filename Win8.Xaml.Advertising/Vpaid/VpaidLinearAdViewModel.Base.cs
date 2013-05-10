using System;
using System.Linq;
using System.Net;
using System.ComponentModel;
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
    /// Provides a base class to help implement IInteractiveViewModel
    /// </summary>
    public partial class VpaidLinearAdViewModel : IInteractiveViewModel, INotifyPropertyChanged
    {
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

#pragma warning disable 0067
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
#pragma warning restore 0067
        
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
