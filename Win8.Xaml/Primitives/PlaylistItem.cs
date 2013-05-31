using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Media;
using System.IO;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Media.Protection;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;
#endif

namespace Microsoft.PlayerFramework
{
    // TODO: Bug in Win8 that doesn't allow us to set Source Uri in Xaml unless object is "Control" instead of "DependencyObject".
    /// <summary>
    /// Represents a media item in a playlist.
    /// </summary>
    public sealed class PlaylistItem : DependencyObject, IMediaSource
    {
        /// <summary>
        /// Creates a new instance of the PlaylistItem class.
        /// </summary>
        public PlaylistItem()
        {
            SetValue(AvailableAudioStreamsProperty, new List<IAudioStream>());
            SetValue(AvailableCaptionsProperty, new List<ICaption>());
            SetValue(VisualMarkersProperty, new List<VisualMarker>());
        }

#if SILVERLIGHT
        /// <inheritdoc /> 
        public LicenseAcquirer LicenseAcquirer { get; set; }

#else

        /// <summary>
        /// Identifies the ProtectionManager dependency property.
        /// </summary>
        public static DependencyProperty ProtectionManagerProperty { get { return protectionManagerProperty; } }
        static readonly DependencyProperty protectionManagerProperty = DependencyProperty.Register("ProtectionManager", typeof(MediaProtectionManager), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the dedicated object for media content protection that is associated with this PlaylistItem.
        /// </summary>
        public MediaProtectionManager ProtectionManager
        {
            get { return GetValue(ProtectionManagerProperty) as MediaProtectionManager; }
            set { SetValue(ProtectionManagerProperty, value); }
        }

        /// <summary>
        /// Identifies the Stereo3DVideoPackingMode dependency property.
        /// </summary>
        public static DependencyProperty Stereo3DVideoPackingModeProperty { get { return stereo3DVideoPackingModeProperty; } }
        static readonly DependencyProperty stereo3DVideoPackingModeProperty = DependencyProperty.Register("Stereo3DVideoPackingMode", typeof(Stereo3DVideoPackingMode), typeof(PlaylistItem), new PropertyMetadata(Stereo3DVideoPackingMode.None));

        /// <summary>
        /// Gets or sets the stereo video mode to use for display.
        /// </summary>
        public Stereo3DVideoPackingMode Stereo3DVideoPackingMode
        {
            get { return (Stereo3DVideoPackingMode)GetValue(Stereo3DVideoPackingModeProperty); }
            set { SetValue(Stereo3DVideoPackingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Stereo3DVideoRenderMode dependency property.
        /// </summary>
        public static DependencyProperty Stereo3DVideoRenderModeProperty { get { return stereo3DVideoRenderModeProperty; } }
        static readonly DependencyProperty stereo3DVideoRenderModeProperty = DependencyProperty.Register("Stereo3DVideoRenderMode", typeof(Stereo3DVideoRenderMode), typeof(PlaylistItem), new PropertyMetadata(Stereo3DVideoRenderMode.Mono));

        /// <summary>
        /// Gets or sets the stereo video mode to use for display.
        /// </summary>
        public Stereo3DVideoRenderMode Stereo3DVideoRenderMode
        {
            get { return (Stereo3DVideoRenderMode)GetValue(Stereo3DVideoRenderModeProperty); }
            set { SetValue(Stereo3DVideoRenderModeProperty, value); }
        }
#endif
        /// <summary>
        /// Identifies the PosterSource dependency property.
        /// </summary>
        public static DependencyProperty PosterSourceProperty { get { return posterSourceProperty; } }
        static readonly DependencyProperty posterSourceProperty = DependencyProperty.Register("PosterSource", typeof(ImageSource), typeof(PlaylistItem), null);

        /// <inheritdoc /> 
        public ImageSource PosterSource
        {
            get { return GetValue(PosterSourceProperty) as ImageSource; }
            set { SetValue(PosterSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the AutoLoad dependency property.
        /// </summary>
        public static DependencyProperty AutoLoadProperty { get { return autoLoadProperty; } }
        static readonly DependencyProperty autoLoadProperty = DependencyProperty.Register("AutoLoad", typeof(bool), typeof(PlaylistItem), new PropertyMetadata(true));

        /// <inheritdoc /> 
        public bool AutoLoad
        {
            get { return (bool)GetValue(AutoLoadProperty); }
            set { SetValue(AutoLoadProperty, value); }
        }

        /// <summary>
        /// Identifies the AutoPlay dependency property.
        /// </summary>
        public static DependencyProperty AutoPlayProperty { get { return autoPlayProperty; } }
        static readonly DependencyProperty autoPlayProperty = DependencyProperty.Register("AutoPlay", typeof(bool), typeof(PlaylistItem), new PropertyMetadata(true));

        /// <inheritdoc /> 
        public bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        /// <summary>
        /// Identifies the StartupPosition dependency property.
        /// </summary>
        public static DependencyProperty StartupPositionProperty { get { return startupPositionProperty; } }
        static readonly DependencyProperty startupPositionProperty = DependencyProperty.Register("StartupPosition", typeof(TimeSpan?), typeof(PlaylistItem), new PropertyMetadata((TimeSpan?)null));

        /// <inheritdoc /> 
        public TimeSpan? StartupPosition
        {
            get { return (TimeSpan?)GetValue(StartupPositionProperty); }
            set { SetValue(StartupPositionProperty, value); }
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceProperty { get { return sourceProperty; } }
        static readonly DependencyProperty sourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(PlaylistItem), new PropertyMetadata(null, (d, e) => ((PlaylistItem)d).OnSourceChanged(e.OldValue as Uri, e.NewValue as Uri)));

        void OnSourceChanged(Uri oldSource, Uri newSource)
        {
            SourceUri = newSource.OriginalString;
        }

        /// <inheritdoc /> 
        public Uri Source
        {
            get { return GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static DependencyProperty SourceUriProperty { get { return sourceUriProperty; } }
        static readonly DependencyProperty sourceUriProperty = DependencyProperty.Register("SourceUri", typeof(string), typeof(PlaylistItem), new PropertyMetadata(null, (d, e) => ((PlaylistItem)d).OnSourceUriChanged(e.OldValue as string, e.NewValue as string)));

        void OnSourceUriChanged(string oldSourceUri, string newSourceUri)
        {
            Source = new Uri(newSourceUri);
        }

        /// <inheritdoc /> 
        public string SourceUri
        {
            get { return GetValue(SourceUriProperty) as string; }
            set { SetValue(SourceUriProperty, value); }
        }

#if NETFX_CORE
        /// <summary>
        /// Identifies the SourceStream dependency property.
        /// </summary>
        public static DependencyProperty SourceStreamProperty { get { return sourceStreamProperty; } }
        static readonly DependencyProperty sourceStreamProperty = DependencyProperty.Register("SourceStream", typeof(IRandomAccessStream), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the source stream for the playlistitem
        /// </summary>
        public IRandomAccessStream SourceStream
        {
            get { return GetValue(SourceStreamProperty) as IRandomAccessStream; }
            set { SetValue(SourceStreamProperty, value); }
        }

        /// <summary>
        /// Identifies the MimeType dependency property.
        /// </summary>
        public static DependencyProperty MimeTypeProperty { get { return mimeTypeProperty; } }
        static readonly DependencyProperty mimeTypeProperty = DependencyProperty.Register("MimeType", typeof(string), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the mime type for the playlistitem. Only applicable when SourceStream is set.
        /// </summary>
        public string MimeType
        {
            get { return GetValue(MimeTypeProperty) as string; }
            set { SetValue(MimeTypeProperty, value); }
        }
#else
        /// <summary>
        /// Identifies the SourceStream dependency property.
        /// </summary>
        public static DependencyProperty SourceStreamProperty { get { return sourceStreamProperty; } }
        static readonly DependencyProperty sourceStreamProperty = DependencyProperty.Register("SourceStream", typeof(Stream), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the source stream for the playlistitem
        /// </summary>
        public Stream SourceStream
        {
            get { return GetValue(SourceStreamProperty) as Stream; }
            set { SetValue(SourceStreamProperty, value); }
        }
        
        /// <summary>
        /// Identifies the MediaStreamSource dependency property.
        /// </summary>
        public static DependencyProperty MediaStreamSourceProperty { get { return mediaStreamSourceProperty; } }
        static readonly DependencyProperty mediaStreamSourceProperty = DependencyProperty.Register("MediaStreamSource", typeof(MediaStreamSource), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the source MediaStreamSource for the playlistitem
        /// </summary>
        public MediaStreamSource MediaStreamSource
        {
            get { return GetValue(MediaStreamSourceProperty) as MediaStreamSource; }
            set { SetValue(MediaStreamSourceProperty, value); }
        }
#endif

        /// <summary>
        /// Identifies the AudioStreamNames dependency property.
        /// </summary>
        public static DependencyProperty AvailableAudioStreamsProperty { get { return availableAudioStreamsProperty; } }
        static readonly DependencyProperty availableAudioStreamsProperty = DependencyProperty.Register("AvailableAudioStreams", typeof(IList<IAudioStream>), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the list of audio stream names.
        /// </summary>
        public IList<IAudioStream> AvailableAudioStreams
        {
            get { return GetValue(AvailableAudioStreamsProperty) as IList<IAudioStream>; }
        }

        /// <summary>
        /// Identifies the VisualMarkers dependency property.
        /// </summary>
        public static DependencyProperty VisualMarkersProperty { get { return visualMarkersProperty; } }
        static readonly DependencyProperty visualMarkersProperty = DependencyProperty.Register("VisualMarkers", typeof(IList<VisualMarker>), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the list of timeline markers to display in the timeline.
        /// </summary>
        public IList<VisualMarker> VisualMarkers
        {
            get { return GetValue(VisualMarkersProperty) as IList<VisualMarker>; }
        }

        #region AvailableCaptions
        /// <summary>
        /// Identifies the AvailableCaptions dependency property.
        /// </summary>
        public static DependencyProperty AvailableCaptionsProperty { get { return availableCaptionsProperty; } }
        static readonly DependencyProperty availableCaptionsProperty = DependencyProperty.Register("AvailableCaptions", typeof(IList<ICaption>), typeof(PlaylistItem), null);

        /// <summary>
        /// Gets or sets the list of captions that can be chosen by the user.
        /// </summary>
        public IList<ICaption> AvailableCaptions
        {
            get { return GetValue(AvailableCaptionsProperty) as IList<ICaption>; }
        }
        #endregion
    }
}
