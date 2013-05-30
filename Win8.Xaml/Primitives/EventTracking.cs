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
    /// An interface used to implement a tracking plugin.
    /// </summary>
    public interface IEventTracker
    {
        /// <summary>
        /// Raised when a tracking event occurs
        /// </summary>
#if SILVERLIGHT
        event EventHandler<EventTrackedEventArgs> EventTracked;
#else
        event EventHandler<IEventTrackedEventArgs> EventTracked;
#endif
    }

    /// <summary>
    /// An interface used to define a tracking event.
    /// </summary>
    public interface ITrackingEvent
    {
        /// <summary>
        /// Gets or sets extra data associated with a tracking event.
        /// </summary>
        object Data { get; }

        /// <summary>
        /// Gets or sets a string used to identify which component is in need of this event. 
        /// This can allow multiple components to register events to track and only care about the ones they created.
        /// </summary>
        string Area { get; }
    }

    /// <summary>
    /// Provides additional information about a tracking event that has occurred.
    /// </summary>
    public interface IEventTrackedEventArgs
    {
        /// <summary>
        /// Gets the timestamp when the event occurred.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Gets the tracking event instance that occurred.
        /// </summary>
        ITrackingEvent TrackingEvent { get; }
    }

    /// <summary>
    /// Provides a default implementation of IEventTrackedEventArgs
    /// </summary>
#if SILVERLIGHT
    public class EventTrackedEventArgs : EventArgs, IEventTrackedEventArgs
#else
    public sealed class EventTrackedEventArgs : IEventTrackedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of EventTrackedEventArgs
        /// </summary>
        internal EventTrackedEventArgs()
        {
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Creates a new instance of EventTrackedEventArgs
        /// </summary>
        /// <param name="trackingEvent">The event that was tracked</param>
        internal EventTrackedEventArgs(ITrackingEvent trackingEvent)
            : this()
        {
            TrackingEvent = trackingEvent;
        }

        /// <inheritdoc /> 
        public DateTimeOffset Timestamp { get; private set; }

        /// <inheritdoc /> 
        public ITrackingEvent TrackingEvent { get; private set; }
    }

}
