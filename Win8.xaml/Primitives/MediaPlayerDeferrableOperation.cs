﻿using System;
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
