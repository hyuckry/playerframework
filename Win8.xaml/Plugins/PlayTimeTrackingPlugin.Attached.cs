using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Helper class used to attach config data specific to each playlist item.
    /// </summary>
    public static class PlayTimeTracking
    {
        /// <summary>
        /// Identifies the TrackingEvents attached property.
        /// </summary>
        public static DependencyProperty TrackingEventsProperty { get { return trackingEventsProperty; } }
        static readonly DependencyProperty trackingEventsProperty = DependencyProperty.RegisterAttached("TrackingEvents", typeof(IList<PlayTimeTrackingEvent>), typeof(PlayTimeTracking), null);

        /// <summary>
        /// Sets the TrackingEvents attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <param name="propertyValue">A value containing the TrackingEvents to apply to the plugin.</param>
        public static void SetTrackingEvents(DependencyObject obj, IList<PlayTimeTrackingEvent> propertyValue)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            obj.SetValue(TrackingEventsProperty, propertyValue);
        }

        /// <summary>
        /// Gets the TrackingEvents attached property value.
        /// </summary>
        /// <param name="obj">An instance of the MediaPlayer or PlaylistItem.</param>
        /// <returns>A value containing the TrackingEvents to apply to the plugin.</returns>
        public static IList<PlayTimeTrackingEvent> GetTrackingEvents(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return obj.GetValue(TrackingEventsProperty) as IList<PlayTimeTrackingEvent>;
        }

    }
}
