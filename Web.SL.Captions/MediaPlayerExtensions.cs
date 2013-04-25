using System.Linq;
using Microsoft.PlayerFramework;
using Microsoft.PlayerFramework.Captions;
using System.Windows.Media;
using System;

namespace Microsoft.PlayerFramework
{
    public static class MediaPlayerExtensions
    {
        public static CaptionsPlugin GetCaptionsPlugin(this MediaPlayer source)
        {
            return source.Plugins.OfType<CaptionsPlugin>().FirstOrDefault();
        }

        public static CaptionMarker ToCaptionMarker(this TimelineMarker marker)
        {
            var result = new CaptionMarker();
            result.StartTime = marker.Time;
            result.EndTime = marker.Time.Add(TimeSpan.FromSeconds(2));
            result.Text = marker.Text;
            return result;
        }
    }
}
