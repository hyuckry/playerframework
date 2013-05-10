using Microsoft.AdaptiveStreaming;

namespace Microsoft.PlayerFramework.Adaptive
{
    /// <summary>
    /// Wraps a smooth streaming AdaptiveAudioStream class to allow it to inherit AudioStream and participate in the player framework's audio selection APIs.
    /// </summary>
    public sealed class AudioStreamWrapper : IAudioStream
    {
        internal AudioStreamWrapper(AdaptiveAudioStream adaptiveAudioStream)
        {
            AdaptiveAudioStream = adaptiveAudioStream;
            Name = adaptiveAudioStream.Name;
            Language = adaptiveAudioStream.Language;
        }

        /// <summary>
        /// Gets the underlying smooth streaming AdaptiveAudioStream instance.
        /// </summary>
        public AdaptiveAudioStream AdaptiveAudioStream { get; private set; }

        /// <inheritdoc /> 
        public string Language { get; private set; }

        /// <inheritdoc /> 
        public string Name { get; private set; }
    }
}
