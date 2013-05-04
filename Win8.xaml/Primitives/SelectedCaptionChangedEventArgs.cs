using System;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents methods that will handle various routed events that track property value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value where changes in value are reported.</typeparam>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void SelectedCaptionChangedEventHandler(object sender, SelectedCaptionChangedEventArgs e);

    /// <summary>
    /// Contains state information and event data associated with the SelectedCaptionChanged event.
    /// </summary>
    public sealed class SelectedCaptionChangedEventArgs
    {
        internal SelectedCaptionChangedEventArgs(Caption oldValue, Caption newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public Caption NewValue { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public Caption OldValue { get; internal set; }
    }
}
