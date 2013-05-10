using System;

namespace Microsoft.PlayerFramework
{
    public interface IAudioStream
    {
        /// <summary>
        /// Gets or sets the Language of the audio stream.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Gets or sets the name of the audio stream.
        /// </summary>
        string Name { get; }
    }
}
