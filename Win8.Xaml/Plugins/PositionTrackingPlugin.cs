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
    public sealed class PositionTrackingPlugin : DependencyObject, IPlugin, IEventTracker
    {
        private readonly Dictionary<string, PositionTrackingEvent> activeMarkers = new Dictionary<string, PositionTrackingEvent>();
        private readonly List<PositionTrackingEvent> trackingEventsToInitializeOnStart = new List<PositionTrackingEvent>();
        
        /// <summary>
        /// Creates a new instance of TrackingPlugin.
        /// </summary>
        public PositionTrackingPlugin()
        {
            TrackingEvents = new ObservableCollection<PositionTrackingEvent>();
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
        static readonly DependencyProperty trackingEventsProperty = DependencyProperty.Register("TrackingEvents", typeof(IList<PositionTrackingEvent>), typeof(PositionTrackingPlugin), new PropertyMetadata(null, (d, e) => ((PositionTrackingPlugin)d).OnTrackingEventsChanged(e.OldValue as IList<PositionTrackingEvent>, e.NewValue as IList<PositionTrackingEvent>)));

        void OnTrackingEventsChanged(IList<PositionTrackingEvent> oldValue, IList<PositionTrackingEvent> newValue)
        {
            if (oldValue != null)
            {
                UninitializeTrackingEvents(oldValue);
                if (oldValue is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)oldValue).CollectionChanged -= TrackingPlugin_CollectionChanged;
                }
            }

            if (newValue != null)
            {
                InitializeTrackingEvents(newValue);
                if (newValue is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)newValue).CollectionChanged += TrackingPlugin_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the amount of time before an ad will occur that preloading will begin. Set to null to disable preloading. Default is 5 seconds.
        /// </summary>
        public IList<PositionTrackingEvent> TrackingEvents
        {
            get { return GetValue(TrackingEventsProperty) as IList<PositionTrackingEvent>; }
            set { SetValue(TrackingEventsProperty, value); }
        }

        private void TrackingPlugin_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MediaPlayer != null && MediaPlayer.PlayerState == PlayerState.Started) // make sure we're started. If not, wait until MediaStarted fires
            {
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.Cast<PositionTrackingEvent>())
                    {
                        UninitializeTrackingEvent(item);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.Cast<PositionTrackingEvent>())
                    {
                        InitializeTrackingEvent(item);
                    }
                }
            }
        }

        /// <summary>
        /// Manually fires an event
        /// </summary>
        /// <param name="eventArgs">The event to fire</param>
#if SILVERLIGHT
        private void OnTrackEvent(EventTrackedEventArgs eventArgs)
#else
        private void OnTrackEvent(IEventTrackedEventArgs eventArgs)
#endif
        {
            if (EventTracked != null) EventTracked(this, eventArgs);
        }

        /// <summary>
        /// Unsubscribes all tracking events.
        /// </summary>
        private void UninitializeTrackingEvents(IList<PositionTrackingEvent> trackingEvents)
        {
            foreach (var trackingEvent in trackingEvents)
            {
                UninitializeTrackingEvent(trackingEvent);
            }
        }

        /// <summary>
        /// Subscribes all tracking events.
        /// </summary>
        private void InitializeTrackingEvents(IList<PositionTrackingEvent> trackingEvents)
        {
            foreach (var trackingEvent in trackingEvents)
            {
                InitializeTrackingEvent(trackingEvent);
            }
        }

        /// <inheritdoc /> 
        public MediaPlayer MediaPlayer { get; set; }

        /// <inheritdoc /> 
        public void Load()
        {
            InitializeTrackingEvents(TrackingEvents);
            MediaPlayer.MediaClosed += MediaPlayer_MediaClosed;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.MediaStarted += MediaPlayer_MediaStarted;
            MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            MediaPlayer.MarkerReached += MediaPlayer_MarkerReached;
            MediaPlayer.Seeking += MediaPlayer_Seeking;
            MediaPlayer.CompletingScrub += MediaPlayer_CompletingScrub;
        }

        /// <inheritdoc /> 
        public void Update(IMediaSource mediaSource)
        {
            TrackingEvents = PositionTracking.GetTrackingEvents((DependencyObject)mediaSource).ToList();
        }

        /// <inheritdoc /> 
        public void Unload()
        {
            MediaPlayer.MediaClosed -= MediaPlayer_MediaClosed;
            MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
            MediaPlayer.MediaStarted -= MediaPlayer_MediaStarted;
            MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
            MediaPlayer.MarkerReached -= MediaPlayer_MarkerReached;
            MediaPlayer.Seeking -= MediaPlayer_Seeking;
            MediaPlayer.CompletingScrub -= MediaPlayer_CompletingScrub;
            UninitializeTrackingEvents(TrackingEvents);
        }

        /// <summary>
        /// The TimelineMarker ID used to store tracking events.
        /// </summary>
        public static string MarkerType_TrackingEvent { get { return "TrackingEvent"; } }

        /// <summary>
        /// Identifies the EvaluateOnForwardOnly dependency property.
        /// </summary>
        public static DependencyProperty EvaluateOnForwardOnlyProperty { get { return evaluateOnForwardOnlyProperty; } }
        static readonly DependencyProperty evaluateOnForwardOnlyProperty = DependencyProperty.Register("EvaluateOnForwardOnly", typeof(bool), typeof(PositionTrackingPlugin), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether seeking or scrubbing back in time can trigger ads. Set to false to allow ads to be played when seeking backwards. Default is true.
        /// </summary>
        public bool EvaluateOnForwardOnly
        {
            get { return (bool)GetValue(EvaluateOnForwardOnlyProperty); }
            set { SetValue(EvaluateOnForwardOnlyProperty, value); }
        }

        /// <summary>
        /// Provides an opportunity to initialize an individual tracking event.
        /// </summary>
        /// <param name="trackingEvent">The tracking event being subscribed to.</param>
        private void InitializeTrackingEvent(PositionTrackingEvent positionTrackingEvent)
        {
            if (MediaPlayer.PlayerState == PlayerState.Opened || MediaPlayer.PlayerState == PlayerState.Starting || MediaPlayer.PlayerState == PlayerState.Started)
            {
                InitializePositionTrackingEvent(positionTrackingEvent);
            }
            else
            {
                trackingEventsToInitializeOnStart.Add(positionTrackingEvent);
            }
        }

        private void InitializePositionTrackingEvent(PositionTrackingEvent positionTrackingEvent)
        {
            if (!positionTrackingEvent.PositionPercentage.HasValue || (positionTrackingEvent.PositionPercentage.Value > 0 && positionTrackingEvent.PositionPercentage.Value < 1))
            {
                var timelineMarker = new TimelineMarker();
                timelineMarker.Type = MarkerType_TrackingEvent;
                timelineMarker.Text = Guid.NewGuid().ToString();
                if (positionTrackingEvent.PositionPercentage.HasValue)
                {
                    timelineMarker.Time = TimeSpan.FromSeconds(positionTrackingEvent.PositionPercentage.Value * MediaPlayer.Duration.TotalSeconds);
                }
                else
                {
                    timelineMarker.Time = positionTrackingEvent.Position;
                }
                MediaPlayer.Markers.Add(timelineMarker);
                activeMarkers.Add(timelineMarker.Text, positionTrackingEvent);
            }
        }

        /// <summary>
        /// Provides an opportunity to uninitialize an individual tracking event.
        /// </summary>
        /// <param name="trackingEvent">The tracking event being unsubscribed.</param>
        private void UninitializeTrackingEvent(PositionTrackingEvent positionTrackingEvent)
        {
            if (trackingEventsToInitializeOnStart.Contains(positionTrackingEvent))
            {
                trackingEventsToInitializeOnStart.Remove(positionTrackingEvent);
            }
            else
            {
                if (!positionTrackingEvent.PositionPercentage.HasValue || (positionTrackingEvent.PositionPercentage.Value > 0 && positionTrackingEvent.PositionPercentage.Value < 1))
                {
                    var timelineMarkerKey = activeMarkers.First(kvp => kvp.Value == positionTrackingEvent).Key;
                    var timelineMarker = MediaPlayer.Markers.FirstOrDefault(m => m.Type == MarkerType_TrackingEvent && m.Text == timelineMarkerKey);
                    MediaPlayer.Markers.Remove(timelineMarker);
                    activeMarkers.Remove(timelineMarkerKey);
                }
            }
        }

        void MediaPlayer_MediaClosed(object sender, MediaClosedEventArgs e)
        {
            trackingEventsToInitializeOnStart.Clear();
        }

        void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            foreach (var positionTrackingEvent in trackingEventsToInitializeOnStart)
            {
                InitializePositionTrackingEvent(positionTrackingEvent);
            }
        }

        private void MediaPlayer_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            if (e.Marker.Type == MarkerType_TrackingEvent && activeMarkers.ContainsKey(e.Marker.Text))
            {
                var positionTrackingEvent = activeMarkers[e.Marker.Text];
                OnTrackEvent(new PositionEventTrackedEventArgs(positionTrackingEvent, false));
            }
        }

        private void MediaPlayer_MediaStarted(object sender, object e)
        {
            if (!MediaPlayer.StartupPosition.HasValue)
            {
                foreach (var trackingEvent in TrackingEvents.OfType<PositionTrackingEvent>().Where(t => t.PositionPercentage.HasValue && t.PositionPercentage.Value == 0).ToList())
                {
                    OnTrackEvent(new PositionEventTrackedEventArgs(trackingEvent, false));
                }
            }
        }

        private void MediaPlayer_MediaEnded(object sender, MediaEndedEventArgs e)
        {
            foreach (var trackingEvent in TrackingEvents.OfType<PositionTrackingEvent>().Where(t => t.PositionPercentage.HasValue && t.PositionPercentage.Value == 1).ToList())
            {
                OnTrackEvent(new PositionEventTrackedEventArgs(trackingEvent, false));
            }
        }

        private void MediaPlayer_Seeking(object sender, SeekingEventArgs e)
        {
            EvaluateMarkers(e.PreviousPosition, e.Position, true);
        }

        private void MediaPlayer_CompletingScrub(object sender, CompletingScrubEventArgs e)
        {
            EvaluateMarkers(e.StartPosition, e.Position, true);
        }

        /// <summary>
        /// Evaluates all markers in a window and plays an ad if applicable.
        /// </summary>
        /// <param name="originalPosition">The window start position.</param>
        /// <param name="newPosition">The window end position. Note: This can be before originalPosition if going backwards.</param>
        /// <param name="isSeeking">A flag indicating that the user is actively seeking</param>
        public void EvaluateMarkers(TimeSpan originalPosition, TimeSpan newPosition, bool isSeeking)
        {
            if (!EvaluateOnForwardOnly || newPosition > originalPosition)
            {
                foreach (var marker in MediaPlayer.Markers.Where(m => m.Type == MarkerType_TrackingEvent).ToList())
                {
                    if (marker.Time <= newPosition && marker.Time > originalPosition)
                    {
                        if (activeMarkers.ContainsKey(marker.Text))
                        {
                            var positionTrackingEvent = activeMarkers[marker.Text];
                            OnTrackEvent(new PositionEventTrackedEventArgs(positionTrackingEvent, isSeeking));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used to identify and track when the media reaches a specific position.
    /// </summary>
    public sealed class PositionTrackingEvent : ITrackingEvent
    {
        /// <summary>
        /// Gets or sets the playback position when the tracking event will fire.
        /// </summary>
        public TimeSpan Position { get; set; }

        /// <summary>
        /// Gets or sets the percentage of time compared to the duration of the playback position when the tracking event will fire.
        /// .5 = 50%, 2 = 200%
        /// </summary>
        public double? PositionPercentage { get; set; }

        /// <inheritdoc /> 
        public object Data { get; set; }

        /// <inheritdoc /> 
        public string Area { get; set; }
    }

    /// <summary>
    /// Contains additional information about a tracking event that has occurred.
    /// </summary>
#if SILVERLIGHT
    public sealed class PositionEventTrackedEventArgs : EventTrackedEventArgs, IEventTrackedEventArgs
#else
    public sealed class PositionEventTrackedEventArgs : IEventTrackedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of PositionEventTrackedEventArgs
        /// </summary>
        public PositionEventTrackedEventArgs()
        {
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Creates a new instance of PositionEventTrackedEventArgs
        /// </summary>
        /// <param name="trackingEvent">The event that was tracked</param>
        public PositionEventTrackedEventArgs(PositionTrackingEvent trackingEvent)
            : this()
        {
            TrackingEvent = trackingEvent;
        }

        /// <summary>
        /// Creates a new instance of EventTrackedEventArgs
        /// </summary>
        /// <param name="trackingEvent">The event that was tracked</param>
        /// <param name="skippedPast">A flag indicating whether or not the user was seeking when the event occurred</param>
        public PositionEventTrackedEventArgs(PositionTrackingEvent trackingEvent, bool skippedPast)
            : this(trackingEvent)
        {
            SkippedPast = skippedPast;
        }

        /// <summary>
        /// Gets a flag indicating whether or not the user was seeking or scrubbing when the event occurred.
        /// </summary>
        public bool SkippedPast { get; private set; }

        /// <inheritdoc /> 
        public DateTimeOffset Timestamp { get; private set; }

        /// <inheritdoc /> 
        public ITrackingEvent TrackingEvent { get; private set; }
    }
}
