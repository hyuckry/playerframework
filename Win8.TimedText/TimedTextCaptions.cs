using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Microsoft.TimedText
{
    public sealed class TimedTextCaptions : Control
    {
        const long DefaultMaximumCaptionSeekSearchWindowMillis = 60000; //1 minutes
        readonly Dictionary<CaptionRegion, CaptionBlockRegion> regions = new Dictionary<CaptionRegion, CaptionBlockRegion>();
        Panel captionsPresenterElement;
        Size lastSize;
        TimeSpan? lastPosition;
        bool isTemplateApplied;

        /// <summary>
        /// Occurs when a caption region is reached.
        /// </summary>
        internal event EventHandler<CaptionParsedEventArgs> CaptionParsed;

        /// <summary>
        /// Occurs when a caption region is reached.
        /// </summary>
        internal event EventHandler<CaptionRegionEventArgs> CaptionReached;

        /// <summary>
        /// Occurs when a caption region is left.
        /// </summary>
        internal event EventHandler<CaptionRegionEventArgs> CaptionLeft;

        readonly CaptionMarkerFactory factory = new CaptionMarkerFactory();
        readonly Func<MediaMarkerCollection<TimedTextElement>, IMarkerManager<TimedTextElement>> regionManagerFactory;
        readonly IMarkerManager<CaptionRegion> captionManager;

        public TimedTextCaptions()
            : this(null, null)
        { }

        internal TimedTextCaptions(IMarkerManager<CaptionRegion> CaptionManager, Func<MediaMarkerCollection<TimedTextElement>, IMarkerManager<TimedTextElement>> RegionManagerFactory)
        {
            DefaultStyleKey = typeof(TimedTextCaptions);

            VisibleCaptions = new MediaMarkerCollection<CaptionRegion>();

            factory.NewMarkers += NewMarkers;
            factory.MarkersRemoved += MarkersRemoved;

            this.SizeChanged += this_SizeChanged;

            captionManager = CaptionManager ?? new MediaMarkerManager<CaptionRegion>();
            regionManagerFactory = RegionManagerFactory ?? (m => new MediaMarkerManager<TimedTextElement>() { Markers = m });

            Captions = new MediaMarkerCollection<CaptionRegion>();

            captionManager.MarkerLeft += captionManager_MarkerLeft;
            captionManager.MarkerReached += captionManager_MarkerReached;
        }

#if SILVERLIGHT
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            captionsPresenterElement = this.GetTemplateChild("CaptionsPresenterElement") as Panel;
            isTemplateApplied = true;
            if (lastPosition.HasValue)
            {
                UpdateCaptions(lastPosition.Value);
            }
        }

#if NETFX_CORE
        public IAsyncAction AugmentTtml(string ttml, TimeSpan startTime, TimeSpan endTime)
        { 
            return AsyncInfo.Run(c => augmentTtml(ttml, startTime, endTime));
        }

        private async Task augmentTtml(string ttml, TimeSpan startTime, TimeSpan endTime)
#else
        public async Task AugmentTtml(string ttml, TimeSpan startTime, TimeSpan endTime)
#endif
        {
            // parse on a background thread
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
            var markers = await TaskEx.Run(() => factory.ParseTtml(ttml, startTime, endTime));       
#else
            var markers = await Task.Run(() => factory.ParseTtml(ttml, startTime, endTime));
#endif
            if (CaptionParsed != null)
            {
                foreach (var marker in markers)
                {
                    CaptionParsed(this, new CaptionParsedEventArgs(marker));
                }
            }

            factory.MergeMarkers(markers);
        }
        
#if NETFX_CORE
        public IAsyncAction ParseTtml(string ttml, bool forceRefresh)
        {
            return AsyncInfo.Run(c => parseTtml(ttml, forceRefresh));
        }

        private async Task parseTtml(string ttml, bool forceRefresh)
#else
        public async Task ParseTtml(string ttml, bool forceRefresh)
#endif
        {
            // parse on a background thread
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
            var markers = await TaskEx.Run(() => factory.ParseTtml(ttml, TimeSpan.Zero, TimeSpan.MaxValue));       
#else
            var markers = await Task.Run(() => factory.ParseTtml(ttml, TimeSpan.Zero, TimeSpan.MaxValue));
#endif
            if (CaptionParsed != null)
            {
                foreach (var marker in markers)
                {
                    CaptionParsed(this, new CaptionParsedEventArgs(marker));
                }
            }

            factory.UpdateMarkers(markers, forceRefresh);
        }

        void captionManager_MarkerReached(IMarkerManager<CaptionRegion> markerManager, CaptionRegion region)
        {
            OnCaptionRegionReached(region);
            if (!regions.ContainsKey(region))
            {
#if HACK_XAMLTYPEINFO
                var children = region.Children as MediaMarkerCollection<TimedTextElement>;
#else
                var children = region.Children;
#endif
                var regionBlock = new CaptionBlockRegion()
                {
                    CaptionRegion = region,
                    CaptionManager = regionManagerFactory(children),
                };
                regions.Add(region, regionBlock);
                captionsPresenterElement.Children.Add(regionBlock);
                regionBlock.ApplyTemplate();
                VisibleCaptions.Add(region);
            }
        }

        void captionManager_MarkerLeft(IMarkerManager<CaptionRegion> markerManager, CaptionRegion region)
        {
            if (regions.ContainsKey(region))
            {
                var presenter = regions[region];
                captionsPresenterElement.Children.Remove(presenter);
                VisibleCaptions.Remove(region);
                regions.Remove(region);
            }
            OnCaptionRegionLeft(region);
        }

        internal MediaMarkerCollection<CaptionRegion> VisibleCaptions { get; private set; }

        //#region VisibleCaptions
        ///// <summary>
        ///// VisibleCaptions DependencyProperty definition.
        ///// </summary>
        //public static readonly DependencyProperty VisibleCaptionsProperty = DependencyProperty.Register("VisibleCaptions", typeof(MediaMarkerCollection<CaptionRegion>), typeof(TimedTextCaptions), new PropertyMetadata(new MediaMarkerCollection<CaptionRegion>()));

        ///// <summary>
        ///// Gets the current caption markers.
        ///// </summary>
        //public MediaMarkerCollection<CaptionRegion> VisibleCaptions
        //{
        //    get { return (MediaMarkerCollection<CaptionRegion>)GetValue(VisibleCaptionsProperty); }
        //    private set { SetValue(VisibleCaptionsProperty, value); }
        //}

        //#endregion

        public void UpdateCaptions(TimeSpan position)
        {
            lastPosition = position;

            if (isTemplateApplied)
            {
                captionManager.CheckMarkerPositions(position);

                foreach (var region in regions)
                {
                    var regionBlock = (CaptionBlockRegion)region.Value;
                    regionBlock.UpdateAnimations(position);
                    regionBlock.CaptionManager.CheckMarkerPositions(position);
                }
            }
        }

        #region Captions
        ///// <summary>
        ///// Captions DependencyProperty definition.
        ///// </summary>
        //public static readonly DependencyProperty CaptionsProperty = DependencyProperty.Register("Captions", typeof(MediaMarkerCollection<CaptionRegion>), typeof(TimedTextCaptions), new PropertyMetadata(null, OnCaptionsPropertyChanged));

        ///// <summary>
        ///// Gets the current caption markers.
        ///// </summary>
        //public MediaMarkerCollection<CaptionRegion> Captions
        //{
        //    get { return (MediaMarkerCollection<CaptionRegion>)GetValue(CaptionsProperty); }
        //    set { SetValue(CaptionsProperty, value); }
        //}

        //private static void OnCaptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        //{
        //    var captionHost = d as TimedTextCaptions;
        //    var oldValue = args.OldValue as MediaMarkerCollection<CaptionRegion>;
        //    var newValue = args.NewValue as MediaMarkerCollection<CaptionRegion>;

        //    captionHost.OnCaptionsChanged(newValue);
        //}

        //private void OnCaptionsChanged(MediaMarkerCollection<CaptionRegion> newValue)
        //{
        //    if (captionManager != null)
        //    {
        //        captionManager.Markers = newValue;
        //    }
        //}


        MediaMarkerCollection<CaptionRegion> captions;
        internal MediaMarkerCollection<CaptionRegion> Captions
        {
            get { return captions; }
            set
            {
                captions = value;
                if (captionManager != null)
                {
                    captionManager.Markers = captions;
                }
            }
        }

        #endregion

        /// <summary>
        /// Raises the CaptionLeft event.
        /// </summary>
        void OnCaptionRegionLeft(CaptionRegion region)
        {
            CaptionLeft.IfNotNull(i => i(this, new CaptionRegionEventArgs(region)));
        }

        /// <summary>
        /// Raises the CaptionReached event.
        /// </summary>
        void OnCaptionRegionReached(CaptionRegion region)
        {
            CaptionReached.IfNotNull(i => i(this, new CaptionRegionEventArgs(region)));
        }

        void NewMarkers(IEnumerable<MediaMarker> newMarkers)
        {
            foreach (MediaMarker marker in newMarkers)
            {
                Captions.Add(marker as CaptionRegion);
            }
        }

        void MarkersRemoved(IEnumerable<MediaMarker> removedMarkers)
        {
            foreach (MediaMarker marker in removedMarkers)
            {
                Captions.Remove(marker as CaptionRegion);
            }
        }

        public void Clear()
        {
            factory.Clear();
            Captions.Clear();
            captionManager.Clear();
            if (captionsPresenterElement != null)
            {
                captionsPresenterElement.Children.Clear();
            }
        }

        void this_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConfigureCaptionPresenterSize(e.NewSize);
        }

        Size naturalVideoSize;
        public Size NaturalVideoSize
        {
            get { return naturalVideoSize; }
            set
            {
                naturalVideoSize = value;
                ConfigureCaptionPresenterSize(lastSize);
            }
        }

        void ConfigureCaptionPresenterSize(Size newSize)
        {
            lastSize = newSize;
            if (captionsPresenterElement != null)
            {
                var aspectRatio = NaturalVideoSize.Width / NaturalVideoSize.Height;
                var aspectPresentationWidth = newSize.Height * aspectRatio;

                if (aspectPresentationWidth > newSize.Width)
                {
                    //Video will have black bars on top and bottom
                    captionsPresenterElement.Width = newSize.Width;
                    captionsPresenterElement.Height = newSize.Width / aspectRatio;
                }
                else if (aspectPresentationWidth < newSize.Width)
                {
                    //Video will have black bars on the sides
                    captionsPresenterElement.Height = newSize.Height;
                    captionsPresenterElement.Width = newSize.Height * aspectRatio;
                }
                else
                {
                    captionsPresenterElement.Width = newSize.Width;
                    captionsPresenterElement.Height = newSize.Height;
                }
            }
        }
    }

    internal sealed class CaptionRegionEventArgs : EventArgs
    {
        public CaptionRegionEventArgs(CaptionRegion captionRegion)
        {
            CaptionRegion = captionRegion;
        }

        public CaptionRegion CaptionRegion { get; private set; }
    }

    internal sealed class CaptionParsedEventArgs : EventArgs
    {
        public CaptionParsedEventArgs(MediaMarker captionMarker)
        {
            CaptionMarker = captionMarker;
        }

        public MediaMarker CaptionMarker { get; private set; }
    }
}
