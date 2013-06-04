using System;
#if SILVERLIGHT
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a static class used to create commands that can be bound to media player buttons
    /// </summary>
    public static class ViewModelCommandFactory
    {
        /// <summary>
        /// Creates a command used to bind to a pause button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreatePauseCommand()
        {
            return new ViewModelCommand(
                vm => vm.Pause(),
                vm => vm.IsPauseEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsPauseEnabledChanged -= eh, (vm, eh) => vm.IsPauseEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsPauseEnabledChanged -= eh, (vm, eh) => vm.IsPauseEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a play button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreatePlayResumeCommand()
        {
            return new ViewModelCommand(
                vm => vm.PlayResume(),
                vm => vm.IsPlayResumeEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsPlayResumeEnabledChanged -= eh, (vm, eh) => vm.IsPlayResumeEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsPlayResumeEnabledChanged -= eh, (vm, eh) => vm.IsPlayResumeEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a stop button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateStopCommand()
        {
            return new ViewModelCommand(
                vm => vm.Stop(),
                vm => vm.IsStopEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsStopEnabledChanged -= eh, (vm, eh) => vm.IsStopEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsStopEnabledChanged -= eh, (vm, eh) => vm.IsStopEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to an instant replay button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateReplayCommand()
        {
            return new ViewModelCommand(
                vm => vm.Replay(),
                vm => vm.IsReplayEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsReplayEnabledChanged -= eh, (vm, eh) => vm.IsReplayEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsReplayEnabledChanged -= eh, (vm, eh) => vm.IsReplayEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a rewind button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateRewindCommand()
        {
            return new ViewModelCommand(
                vm => vm.DecreasePlaybackRate(),
                vm => vm.IsRewindEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsRewindEnabledChanged -= eh, (vm, eh) => vm.IsRewindEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsRewindEnabledChanged -= eh, (vm, eh) => vm.IsRewindEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a fast forward button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateFastForwardCommand()
        {
            return new ViewModelCommand(
                vm => vm.IncreasePlaybackRate(),
                vm => vm.IsFastForwardEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsFastForwardEnabledChanged -= eh, (vm, eh) => vm.IsFastForwardEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsFastForwardEnabledChanged -= eh, (vm, eh) => vm.IsFastForwardEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a slow motion button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSlowMotionCommand()
        {
            return new ViewModelCommand(
                vm => vm.IsSlowMotion = !vm.IsSlowMotion,
                vm => vm.IsSlowMotionEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSlowMotionEnabledChanged -= eh, (vm, eh) => vm.IsSlowMotionEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSlowMotionEnabledChanged -= eh, (vm, eh) => vm.IsSlowMotionEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a volume slider. Note: the volume value is expected to be passed in as a CommandParameter.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateVolumeCommand()
        {
            return new TypedViewModelCommand<double>(
                (vm, volume) => vm.Volume = volume,
                (vm, volume) => true
                );
        }

        /// <summary>
        /// Creates a command used to bind to a mute button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateMuteCommand()
        {
            return new ViewModelCommand(
                vm => vm.IsMuted = !vm.IsMuted,
                vm => true
                );
        }

        /// <summary>
        /// Creates a command used to bind to a caption selection button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateCaptionsCommand()
        {
            return new ViewModelCommand(
                vm => vm.InvokeCaptionSelection(),
                vm => vm.IsCaptionSelectionEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsCaptionSelectionEnabledChanged -= eh, (vm, eh) => vm.IsCaptionSelectionEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsCaptionSelectionEnabledChanged -= eh, (vm, eh) => vm.IsCaptionSelectionEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a audio stream selection button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateAudioSelectionCommand()
        {
            return new ViewModelCommand(
                vm => vm.InvokeAudioSelection(),
                vm => vm.IsAudioSelectionEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsAudioSelectionEnabledChanged -= eh, (vm, eh) => vm.IsAudioSelectionEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsAudioSelectionEnabledChanged -= eh, (vm, eh) => vm.IsAudioSelectionEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a fullscreen button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateFullScreenCommand()
        {
            return new ViewModelCommand(
                vm => vm.IsFullScreen = !vm.IsFullScreen,
                vm => true
                );
        }

        /// <summary>
        /// Creates a command used to bind to a seek button. Note: the new position is expected to be passed in as a CommandParameter.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSeekCommand()
        {
            return new TypedViewModelCommand<TimeSpan>(
                (vm, position) => { bool canceled; vm.Seek(position, out canceled); },
                (vm, position) => vm.IsSeekEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSeekEnabledChanged -= eh, (vm, eh) => vm.IsSeekEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSeekEnabledChanged -= eh, (vm, eh) => vm.IsSeekEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a skip previous marker/playlist item button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSkipPreviousCommand()
        {
            return new ViewModelCommand(
                (vm) => vm.SkipPrevious(),
                (vm) => vm.IsSkipPreviousEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSkipPreviousEnabledChanged -= eh, (vm, eh) => vm.IsSkipPreviousEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSkipPreviousEnabledChanged -= eh, (vm, eh) => vm.IsSkipPreviousEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a skip next marker/playlist item button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSkipNextCommand()
        {
            return new ViewModelCommand(
                (vm) => vm.SkipNext(),
                (vm) => vm.IsSkipNextEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSkipNextEnabledChanged -= eh, (vm, eh) => vm.IsSkipNextEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSkipNextEnabledChanged -= eh, (vm, eh) => vm.IsSkipNextEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a skip back button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSkipBackCommand()
        {
            return new ViewModelCommand(
                vm => vm.SkipBack(),
                vm => vm.IsSkipBackEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSkipBackEnabledChanged -= eh, (vm, eh) => vm.IsSkipBackEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSkipBackEnabledChanged -= eh, (vm, eh) => vm.IsSkipBackEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a skip ahead button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateSkipAheadCommand()
        {
            return new ViewModelCommand(
                vm => vm.SkipAhead(),
                vm => vm.IsSkipAheadEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsSkipAheadEnabledChanged -= eh, (vm, eh) => vm.IsSkipAheadEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsSkipAheadEnabledChanged -= eh, (vm, eh) => vm.IsSkipAheadEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a go live button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreateGoLiveCommand()
        {
            return new ViewModelCommand(
                vm => vm.GoLive(),
                vm => vm.IsGoLiveEnabled,
#if SILVERLIGHT
                new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsGoLiveEnabledChanged -= eh, (vm, eh) => vm.IsGoLiveEnabledChanged += eh)
#else
                new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsGoLiveEnabledChanged -= eh, (vm, eh) => vm.IsGoLiveEnabledChanged += eh)
#endif
                );
        }

        /// <summary>
        /// Creates a command used to bind to a play/pause button.
        /// </summary>
        /// <returns>A special ICommand object expected to be wired to a ViewModel.</returns>
        public static IViewModelCommand CreatePlayPauseCommand()
        {
            return new ViewModelCommand(
                vm =>
                {
                    if (vm.IsPlayResumeEnabled)
                    {
                        vm.PlayResume();
                    }
                    else
                    {
                        vm.Pause();
                    }
                },
            vm => vm.IsPauseEnabled || vm.IsPlayResumeEnabled,
#if SILVERLIGHT
            new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsPlayResumeEnabledChanged -= eh, (vm, eh) => vm.IsPlayResumeEnabledChanged += eh),
            new HandlerReference<IInteractiveViewModel, EventHandler>((vm, eh) => vm.IsPauseEnabledChanged -= eh, (vm, eh) => vm.IsPauseEnabledChanged += eh)
#else
            new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsPlayResumeEnabledChanged -= eh, (vm, eh) => vm.IsPlayResumeEnabledChanged += eh),
            new HandlerReference<IInteractiveViewModel, EventHandler<object>>((vm, eh) => vm.IsPauseEnabledChanged -= eh, (vm, eh) => vm.IsPauseEnabledChanged += eh)
#endif
);
        }
    }
}
