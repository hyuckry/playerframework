using System;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents the method that will handle a MediaPlayer action.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void MediaEndedEventHandler(object sender, MediaEndedEventArgs e);

    /// <summary>
    /// Contains state information and event data associated with a MediaPlayer action event.
    /// </summary>
    public sealed class MediaEndedEventArgs : RoutedEventArgs
    {
        internal MediaEndedEventArgs() { }

        /// <summary>
        /// Gets or sets whether the event was already handled.
        /// </summary>
        public bool Handled { get; set; }
    }
}
