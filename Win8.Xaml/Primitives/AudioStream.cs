using System;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents an audio stream.
    /// </summary>
    public sealed class AudioStream : IAudioStream
    {
        /// <summary>
        /// Creates a new instance of AudioStream
        /// </summary>
        public AudioStream() { }

        /// <summary>
        /// Creates a new instance of AudioStream
        /// </summary>
        /// <param name="name">The name of the audio stream</param>
        public AudioStream(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of AudioStream
        /// </summary>
        /// <param name="name">The name of the audio stream</param>
        /// <param name="language">The language of the audio stream</param>
        public AudioStream(string name, string language)
            : this(name)
        {
            Language = language;
        }

        /// <inheritdoc /> 
        public string Name { get; set; }

        /// <inheritdoc /> 
        public string Language { get; set; }

        /// <inheritdoc /> 
        public override string ToString()
        {
            return Name;
        }
    }
}
