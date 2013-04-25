using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace Microsoft.PlayerFramework.Adaptive
{
    public class InstreamTimedTextPlugin : IPlugin
    {
        AdaptiveStreamingManager manager;
        public AdaptiveStreamingManager Manager
        {
            get { return manager; }
            set
            {
                if (manager != null)
                {
                    manager.DataReceived -= Manager_DataReceived;
                    manager.ManifestReady -= Manager_ManifestReady;
                }
                manager = value;
                if (manager != null)
                {
                    manager.ManifestReady += Manager_ManifestReady;
                    manager.DataReceived += Manager_DataReceived;
                }
            }
        }

        void Manager_ManifestReady(object sender, System.EventArgs e)
        {
            MediaPlayer.AvailableCaptions.Clear();
            foreach (var captionStream in Manager.AvailableCaptionStreams)
            {
                var wrapper = new CaptionStreamWrapper(captionStream);
                MediaPlayer.AvailableCaptions.Add(wrapper);
                if (captionStream == Manager.SelectedCaptionStream)
                {
                    MediaPlayer.SelectedCaption = wrapper;
                }
            }
        }

        void MediaPlayer_SelectedCaptionChanged(object sender, RoutedPropertyChangedEventArgs<Caption> e)
        {
            var newCaptionStream = e.NewValue is CaptionStreamWrapper ? ((CaptionStreamWrapper)e.NewValue).AdaptiveCaptionStream : null;
            Manager.SelectedCaptionStream = newCaptionStream;
        }

        void Manager_DataReceived(AdaptiveStreamingManager manager, DataReceivedInfo dataReceived)
        {
            if (AdaptiveStreamingManager.IsCaptionStream(dataReceived.StreamInfo))
            {
                var caption = MediaPlayer.SelectedCaption as CaptionStreamWrapper;
                if (caption != null)
                {
                    var data = dataReceived.Data;
                    var startTime = dataReceived.ChunkInfo.TimeStamp;
                    var endTime = dataReceived.ChunkInfo.TimeStamp.Add(dataReceived.ChunkInfo.Duration);

                    // push data into the TTML parser via the CaptionWrapper object. This will notify the TTML plugin
                    var ttml = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    System.Diagnostics.Debug.WriteLine(ttml);
                    // TODO: merge and offset captions or wait until we're ready
                    caption.Payload = ttml;

                    //using (var stream = new MemoryStream(data))
                    //{
                    //    var document = XDocument.Load(stream);
                    //    var body = document.Root.Element("{http://www.w3.org/2006/10/ttaf1}body");
                    //    if (body != null && body.HasElements)
                    //    {
                    //        // push data into the TTML parser via the CaptionWrapper object. This will notify the TTML plugin
                    //        caption.Payload = document.ToString();

                    //        //var parser = new TimedTextMarkerParser();
                    //        //var markers = parser.ParseMarkerCollection(document, workload.startTime, workload.endTime);
                    //        //if (markers != null)
                    //        //{
                    //        //    UpdateCaptions(((SMFPlayer)_player).Captions, markers);
                    //        //}
                    //    }
                    //}
                }
            }
        }

        public void Load()
        {
            var adaptivePlugin = MediaPlayer.Plugins.OfType<AdaptivePlugin>().FirstOrDefault();
            if (adaptivePlugin != null)
            {
                Manager = adaptivePlugin.Manager;
            }

            MediaPlayer.SelectedCaptionChanged += MediaPlayer_SelectedCaptionChanged;
        }

        public void Update(IMediaSource mediaSource)
        {
        }

        public void Unload()
        {
            Manager = null;
            MediaPlayer.SelectedCaptionChanged -= MediaPlayer_SelectedCaptionChanged;
        }

        public MediaPlayer MediaPlayer { get; set; }
    }
}
