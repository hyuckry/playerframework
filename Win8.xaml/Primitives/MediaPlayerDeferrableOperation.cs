using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.Storage.Streams;
#else
using System.IO;
using System.Windows.Media;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Provides data for a deferrable MediaLoading event.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaLoadingEventArgs : EventArgs
#else
    public sealed class MediaLoadingEventArgs
#endif
    {
        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, Uri source)
        {
            Source = source;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source Uri of the loading operation. This may be null if the source is a stream.
        /// </summary>
        public Uri Source { get; set; }

#if NETFX_CORE
        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, IRandomAccessStream sourceStream, string mimeType)
        {
            SourceStream = sourceStream;
            MimeType = mimeType;
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets or sets the source stream of the loading operation. This may be null if the source is a Uri.
        /// </summary>
        public IRandomAccessStream SourceStream { get; set; }

        /// <summary>
        /// Gets or sets the mime type of the loading operation. This is unused if the source is a Uri.
        /// </summary>
        public string MimeType { get; set; }
#else

        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, Stream sourceStream)
            : base(deferrableOperation)
        {
            SourceStream = sourceStream;
        }

        /// <summary>
        /// Gets or sets the source MediaStreamSource of the loading operation. This may be null if the source is a Uri or MediaStreamSource.
        /// </summary>
        public Stream SourceStream { get; set; }

        internal MediaLoadingEventArgs(MediaPlayerDeferrableOperation deferrableOperation, MediaStreamSource mediaStreamSource)
            : base(deferrableOperation)
        {
            MediaStreamSource = mediaStreamSource;
        }

        /// <summary>
        /// Gets or sets the source MediaStreamSource of the loading operation. This may be null if the source is a stream or Uri.
        /// </summary>
        public MediaStreamSource MediaStreamSource { get; set; }
#endif
        
        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Provides data for a deferrable event.
    /// </summary>
#if SILVERLIGHT
    public sealed class MediaPlayerDeferrableEventArgs : EventArgs
#else
    public sealed class MediaPlayerDeferrableEventArgs
#endif
    {
        internal MediaPlayerDeferrableEventArgs(MediaPlayerDeferrableOperation deferrableOperation)
        {
            DeferrableOperation = deferrableOperation;
        }

        /// <summary>
        /// Gets the deferrable operation.
        /// </summary>
        public MediaPlayerDeferrableOperation DeferrableOperation { get; private set; }
    }

    /// <summary>
    /// Provides info about a deferrable operation.
    /// </summary>
    public sealed class MediaPlayerDeferrableOperation
    {
        readonly CancellationTokenSource cts;
        readonly List<MediaPlayerDeferral> deferrals;

        internal MediaPlayerDeferrableOperation(CancellationTokenSource cancellationTokenSource)
        {
            cts = cancellationTokenSource;
            deferrals = new List<MediaPlayerDeferral>();
        }

        internal bool DeferralsExist
        {
            get
            {
                return deferrals.Any();
            }
        }

        internal Task<bool[]> Task
        {
            get
            {
#if SILVERLIGHT && !WINDOWS_PHONE || WINDOWS_PHONE7
                return System.Threading.Tasks.TaskEx.WhenAll(deferrals.Select(d => d.Task));
#else
                return System.Threading.Tasks.Task.WhenAll(deferrals.Select(d => d.Task));
#endif
            }
        }

        internal void Cancel()
        {
            cts.Cancel();
        }

        /// <summary>
        /// Requests that the deferrable operation be delayed.
        /// </summary>
        /// <returns>The deferral.</returns>
        public MediaPlayerDeferral GetDeferral()
        {
            var result = new MediaPlayerDeferral(cts.Token);
            deferrals.Add(result);
            return result;
        }
    }

    /// <summary>
    /// Manages a delayed app deferrable operation.
    /// </summary>
    public sealed class MediaPlayerDeferral
    {
        readonly TaskCompletionSource<bool> tcs;

        internal MediaPlayerDeferral(CancellationToken cancellationToken)
        {
            tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(OnCancel);
        }

        internal Task<bool> Task
        {
            get
            {
                return tcs.Task;
            }
        }

        /// <summary>
        /// Indicates that the deferral should be cancelled ASAP.
        /// </summary>
#if SILVERLIGHT
        public event EventHandler Canceled;
#else
        public event EventHandler<object> Canceled;
#endif

        void OnCancel()
        {
            if (Canceled != null) Canceled(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifies the MediaPlayer that the operation is complete.
        /// </summary>
        public void Complete()
        {
            tcs.TrySetResult(true);
        }

        /// <summary>
        /// Notifies the MediaPlayer that the operation should be cancelled.
        /// </summary>
        public void Cancel()
        {
            tcs.TrySetResult(false);
        }
    }
}
