using System.Linq;
using Microsoft.Web.Media.SmoothStreaming;

namespace Microsoft.PlayerFramework.Adaptive
{
    public class CaptionStreamWrapper : Caption
    {
        internal CaptionStreamWrapper(StreamInfo adaptiveCaptionStream)
        {
            AdaptiveCaptionStream = adaptiveCaptionStream;
            base.Description = adaptiveCaptionStream.GetLanguage() ?? adaptiveCaptionStream.GetName();
        }

        /// <summary>
        /// Gets the underlying smooth streaming StreamInfo instance.
        /// </summary>
        public StreamInfo AdaptiveCaptionStream { get; private set; }

    }
}
