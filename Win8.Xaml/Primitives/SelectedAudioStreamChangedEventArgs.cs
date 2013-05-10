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
    public delegate void SelectedAudioStreamChangedEventHandler(object sender, SelectedAudioStreamChangedEventArgs e);

    /// <summary>
    /// Contains state information and event data associated with the SelectedAudioStreamChanged event.
    /// </summary>
    public sealed class SelectedAudioStreamChangedEventArgs
    {
        internal SelectedAudioStreamChangedEventArgs(IAudioStream oldValue, IAudioStream newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets or sets whether the event handler took responsibility for modifying the selected audio stream.
        /// Setting to true will prevent the MediaPlayer from setting the AudioStreamIndex property automatically.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public IAudioStream NewValue { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public IAudioStream OldValue { get; internal set; }
    }
}
