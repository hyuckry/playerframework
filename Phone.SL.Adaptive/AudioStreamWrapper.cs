﻿using System;
using System.Linq;
using Microsoft.Web.Media.SmoothStreaming;
using System.Globalization;

namespace Microsoft.PlayerFramework.Adaptive
{
    /// <summary>
    /// Wraps a smooth streaming StreamInfo class to allow it to inherit AudioStream and participate in the player framework's audio selection APIs.
    /// </summary>
    public class AudioStreamWrapper : IAudioStream
    {
        internal AudioStreamWrapper(StreamInfo adaptiveAudioStream)
        {
            AdaptiveAudioStream = adaptiveAudioStream;
            Name = adaptiveAudioStream.GetName();
            Language = adaptiveAudioStream.GetLanguage();
            //Language = new CultureInfo(adaptiveAudioStream.GetLanguage()).DisplayName;
        }

        /// <summary>
        /// Gets the underlying smooth streaming StreamInfo instance.
        /// </summary>
        public StreamInfo AdaptiveAudioStream { get; private set; }

        public string Language { get; private set; }

        public string Name { get; private set; }
    }
}
