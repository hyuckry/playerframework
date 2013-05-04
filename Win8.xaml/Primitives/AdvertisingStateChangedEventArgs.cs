﻿using System;
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
    public delegate void AdvertisingStateChangedEventHandler(object sender, AdvertisingStateChangedEventArgs e);

    /// <summary>
    /// Contains state information and event data associated with the AdvertisingStateChanged event.
    /// </summary>
    public sealed class AdvertisingStateChangedEventArgs
    {
        internal AdvertisingStateChangedEventArgs(AdvertisingState oldValue, AdvertisingState newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public AdvertisingState NewValue { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public AdvertisingState OldValue { get; internal set; }
    }
}
