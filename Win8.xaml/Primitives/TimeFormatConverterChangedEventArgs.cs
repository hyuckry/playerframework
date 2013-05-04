using System;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents methods that will handle various routed events that track property value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value where changes in value are reported.</typeparam>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void TimeFormatConverterChangedEventHandler(object sender, TimeFormatConverterChangedEventArgs e);

    /// <summary>
    /// Contains state information and event data associated with the TimeFormatConverterChanged event.
    /// </summary>
    public sealed class TimeFormatConverterChangedEventArgs
    {
        internal TimeFormatConverterChangedEventArgs(IValueConverter oldValue, IValueConverter newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IValueConverter NewValue { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IValueConverter OldValue { get; internal set; }
    }
}
