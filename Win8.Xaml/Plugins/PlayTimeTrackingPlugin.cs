using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A plugin used to help track when specific events occur during playback.
    /// </summary>
    public sealed class PlayTimeTrackingPlugin : DependencyObject, IPlugin, IEventTracker
    {
        private readonly List<PlayTimeTrackingEvent> spentPlayTimeEvents = new List<PlayTimeTrackingEvent>();

        private DateTime? startTime;
        
        /// <summary>
        /// Creates a new instance of TrackingPlugin.
        /// </summary>
        public PlayTimeTrackingPlugin()
        {
            TrackingEvents = new ObservableCollection<PlayTimeTrackingEvent>();
        }

        /// <inheritdoc /> 
#if SILVERLIGHT
        public event EventHandler<EventTrackedEventArgs> EventTracked;
#else
        public event EventHandler<IEventTrackedEventArgs> EventTracked;
#endif

        /// <summary>
        /// Identifies the PreloadTime dependency property.
        /// </summary>
        public static DependencyProperty TrackingEventsProperty { get { return trackingEventsProperty; } }
        static readonly DependencyProperty trackingEventsProperty = DependencyProperty.Register("TrackingEvents", typeof(IList<PlayTimeTrackingEvent>), typeof(PlayTimeTrackingPlugin), null);
        
        /// <summary>
        /// Gets or sets the amount of time before an ad will occur that preloading will begin. Set to null to disable preloading. Default is 5 seconds.
        /// </summary>
        public IList<PlayTimeTrackingEvent> TrackingEvents
        {
            get { return GetValue(TrackingEventsProperty) as IList<PlayTimeTrackingEvent>; }
            set { SetValue(TrackingEventsProperty, value); }
        }

        /// <summary>
        /// Manually fires an event
        /// </summary>
        /// <param name="eventArgs">The event to fire</param>
        void OnTrackEvent(EventTrackedEventArgs eventArgs)
        {
            if (EventTracked != null) EventTracked(this, eventArgs);
        }

        /// <summary>
        /// Gets the total time watched in the current session. Reset when new media is loaded but not reset when media plays to the end, loops, or restarts.
        /// </summary>
        public TimeSpan PlayTime { get; private set; }

        /// <summary>
        /// Gets or sets the percentage of time compared to the duration that the media has been played before the tracking event will fire.
        /// .5 = 50%, 2 = 200%
        /// </summary>
        public double PlayTimePercentage { get; private set; }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            MediaPlayer.Updated += MediaPlayer_Updated;
            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            TrackingEvents = PlayTimeTracking.GetTrackingEvents((DependencyObject)mediaSource).ToList();
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            MediaPlayer.Updated -= MediaPlayer_Updated;
            MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
        }

        void MediaPlayer_MediaEnded(object sender, MediaEndedEventArgs e)
        {
            EvaluteTrackingEvents();
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (MediaPlayer.CurrentState)
            {
                case MediaElementState.Playing:
                    startTime = DateTime.Now.Subtract(PlayTime);
                    break;
                case MediaElementState.Closed:
                    spentPlayTimeEvents.Clear();
                    break;
                case MediaElementState.Opening:
                    spentPlayTimeEvents.Clear();
                    startTime = null;
                    PlayTime = TimeSpan.Zero;
                    PlayTimePercentage = 0;
                    break;
                case MediaElementState.Buffering:
                case MediaElementState.Paused:
                case MediaElementState.Stopped:
                    if (startTime.HasValue) PlayTime = DateTime.Now.Subtract(startTime.Value);
                    break;
            }
        }

        private void MediaPlayer_Updated(object sender, object e)
        {
            EvaluteTrackingEvents();
        }

        private void EvaluteTrackingEvents()
        {
            if (MediaPlayer.CurrentState == MediaElementState.Playing && startTime.HasValue)
            {
                PlayTime = DateTime.Now.Subtract(startTime.Value);
                PlayTimePercentage = PlayTime.TotalSeconds / MediaPlayer.Duration.TotalSeconds;
                foreach (var playTimeTrackingEvent in TrackingEvents.OfType<PlayTimeTrackingEvent>().Except(spentPlayTimeEvents).ToList())
                {
                    if ((!playTimeTrackingEvent.PlayTimePercentage.HasValue && playTimeTrackingEvent.PlayTime <= PlayTime) || (playTimeTrackingEvent.PlayTimePercentage.HasValue && playTimeTrackingEvent.PlayTimePercentage <= PlayTimePercentage))
                    {
                        spentPlayTimeEvents.Add(playTimeTrackingEvent);
                        OnTrackEvent(new EventTrackedEventArgs(playTimeTrackingEvent));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used to identify and track when the media has been played a specified amount of time.
    /// </summary>
    public sealed class PlayTimeTrackingEvent : ITrackingEvent
    {
        /// <summary>
        /// Gets or sets the absolute amount of time that the media has been played before the tracking event will fire.
        /// </summary>
        public TimeSpan PlayTime { get; set; }

        /// <summary>
        /// Gets or sets the percentage of time compared to the duration that the media has been played before the tracking event will fire.
        /// .5 = 50%, 2 = 200%
        /// </summary>
        public double? PlayTimePercentage { get; set; }

        /// <inheritdoc /> 
        public object Data { get; set; }

        /// <inheritdoc /> 
        public string Area { get; set; }
    }
}
