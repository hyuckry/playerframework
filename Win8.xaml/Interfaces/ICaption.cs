using System;

namespace Microsoft.PlayerFramework
{
    public interface ICaption
    {
        /// <summary>
        /// Gets or sets the description of the caption track.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets or sets the Id of the caption track.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets the payload of the caption track. This can be any object.
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// Indicates that the Payload should be appended
        /// </summary>
        event EventHandler<PayloadAugmentedEventArgs> PayloadAugmented;

        /// <summary>
        /// Indicates that the Payload property has changed
        /// </summary>
#if SILVERLIGHT
        event EventHandler PayloadChanged;
#else
        event EventHandler<object> PayloadChanged;
#endif
    }

    /// <summary>
    /// Includes information that allows a caption engine to augment the existing caption information.
    /// Useful for in-stream caption support where caption data comes in chunks.
    /// </summary>
#if SILVERLIGHT
    public sealed class PayloadAugmentedEventArgs : EventArgs
#else
    public sealed class PayloadAugmentedEventArgs
#endif
    {
        /// <summary>
        /// Creates a new instance of PayloadAugmentedEventArgs.
        /// </summary>
        /// <param name="payload">The caption payload (typically a bytearry or string).</param>
        /// <param name="startTime">The offset for the caption data. Typically set from the chunk timestamp.</param>
        /// <param name="endTime">The end time of the caption data. Typically set from the chunk starttime + duration (or timestamp of next chunk for sparse text tracks).</param>
        public PayloadAugmentedEventArgs(object payload, TimeSpan startTime, TimeSpan endTime)
        {
            Payload = payload;
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// The caption payload (typically a bytearry or string).
        /// </summary>
        public object Payload { get; private set; }

        /// <summary>
        /// The offset for the caption data. Typically set from the chunk timestamp.
        /// </summary>
        public TimeSpan StartTime { get; private set; }

        /// <summary>
        /// The end time of the caption data. Typically set from the chunk starttime + duration (or timestamp of next chunk for sparse text tracks).
        /// </summary>
        public TimeSpan EndTime { get; private set; }
    }
}
