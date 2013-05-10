using System;
using Microsoft.AdaptiveStreaming;

namespace Microsoft.PlayerFramework.Adaptive
{
    /// <summary>
    /// Wraps a smooth streaming AdaptiveCaptionStream class to allow it to implement ICaption and participate in the player framework's caption selection APIs.
    /// </summary>
    public sealed class CaptionStreamWrapper: ICaption
    {
        internal CaptionStreamWrapper(AdaptiveCaptionStream adaptiveCaptionStream)
        {
            AdaptiveCaptionStream = adaptiveCaptionStream;
            Id = adaptiveCaptionStream.Name;
            Description = adaptiveCaptionStream.Language;
        }

        /// <summary>
        /// Invokes the PayloadChanged event
        /// </summary>
        public void AugmentPayload(object payload, TimeSpan startTime, TimeSpan endTime)
        {
            if (PayloadAugmented != null) PayloadAugmented(this, new PayloadAugmentedEventArgs(payload, startTime, endTime));
        }

        /// <summary>
        /// Gets the underlying smooth streaming AdaptiveCaptionStream instance.
        /// </summary>
        public AdaptiveCaptionStream AdaptiveCaptionStream { get; private set; }

        /// <inheritdoc /> 
        public event EventHandler<PayloadAugmentedEventArgs> PayloadAugmented;

        /// <inheritdoc /> 
        public event EventHandler<object> PayloadChanged;

        /// <inheritdoc /> 
        public string Description { get; private set; }

        /// <inheritdoc /> 
        public string Id { get; private set; }

        /// <inheritdoc /> 
        public object Payload { get; private set; }

        /// <inheritdoc /> 
        public override string ToString()
        {
            return Description;
        }
    }
}
