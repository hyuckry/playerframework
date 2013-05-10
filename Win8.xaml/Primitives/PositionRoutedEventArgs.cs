using System;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// EventArgs associated with a Position in the media.
    /// </summary>
    public sealed class SeekRoutedEventArgs : RoutedEventArgs
    {
        internal SeekRoutedEventArgs(TimeSpan previousPosition, TimeSpan newPosition)
        {
            this.Position = newPosition;
            this.PreviousPosition = previousPosition;
        }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan PreviousPosition { get; private set; }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }
    }

    /// <summary>
    /// EventArgs associated with a skip operation.
    /// </summary>
    public sealed class SkipRoutedEventArgs : RoutedEventArgs
    {
        internal SkipRoutedEventArgs(TimeSpan position)
        {
            this.Position = position;
            Canceled = false;
        }

        internal SkipRoutedEventArgs(TimeSpan position, bool canceled)
            : this(position)
        {
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// EventArgs associated with a scrubbing operation.
    /// </summary>
    public sealed class ScrubRoutedEventArgs : RoutedEventArgs
    {
        internal ScrubRoutedEventArgs(TimeSpan position)
        {
            this.Position = position;
            Canceled = false;
        }

        internal ScrubRoutedEventArgs(TimeSpan position, bool canceled)
            : this(position)
        {
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }

    /// <summary>
    /// EventArgs associated with a scrubbing operation that is in progress.
    /// </summary>
    public sealed class ScrubProgressRoutedEventArgs : RoutedEventArgs
    {
        internal ScrubProgressRoutedEventArgs(TimeSpan startPosition, TimeSpan newPosition)
            : this(startPosition, newPosition, false)
        { }

        internal ScrubProgressRoutedEventArgs(TimeSpan startPosition, TimeSpan newPosition, bool canceled)
        {
            this.Position = newPosition;
            StartPosition = startPosition;
            Canceled = canceled;
        }

        /// <summary>
        /// Indicates that action should be aborted.
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// The position when scrubbing started
        /// </summary>
        public TimeSpan StartPosition { get; private set; }

        /// <summary>
        /// The position associated with the event.
        /// </summary>
        public TimeSpan Position { get; private set; }
    }
}
