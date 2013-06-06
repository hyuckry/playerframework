using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
#else
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// Represents a play/pause toggle button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class PlayPauseButton : Button
    {
        /// <summary>
        /// Creates a new instance of PlayPauseButton.
        /// </summary>
        public PlayPauseButton()
        {
            this.DefaultStyleKey = typeof(PlayPauseButton);

            Command = ViewModelCommandFactory.CreatePlayPauseCommand();

            PausedName = MediaPlayer.GetResourceString("PlayButtonLabel");
            PlayingName = MediaPlayer.GetResourceString("PauseButtonLabel");
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(PlayPauseButton), new PropertyMetadata(null, (s, d) => ((PlayPauseButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
            this.SetBinding(PlayPauseButton.IsPausedProperty, new Binding() { Path = new PropertyPath("IsPlayResumeEnabled"), Source = newValue });
        }

        #region Content

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static DependencyProperty IsPausedProperty { get { return isPausedProperty; } }
        static readonly DependencyProperty isPausedProperty = DependencyProperty.Register("IsPaused", typeof(bool), typeof(PlayPauseButton), new PropertyMetadata(false, OnIsPausedChanged));

        static void OnIsPausedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayPauseButton;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                control.Content = control.PausedContent ?? control.Content;
                AutomationProperties.SetName(control, control.PausedName ?? AutomationProperties.GetName(control));
            }
            else
            {
                control.Content = control.PlayingContent ?? control.Content;
                AutomationProperties.SetName(control, control.PlayingName ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets whether the button is in a paused state. This can impact UI aspects of the control.
        /// </summary>
        public bool IsPaused
        {
            get { return (bool)GetValue(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }

        /// <summary>
        /// Identifies the PausedContent dependency property.
        /// </summary>
        public static DependencyProperty PausedContentProperty { get { return pausedContentProperty; } }
        static readonly DependencyProperty pausedContentProperty = DependencyProperty.Register("PausedContent", typeof(object), typeof(PlayPauseButton), new PropertyMetadata(null, OnPausedContentChanged));

        static void OnPausedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayPauseButton;
            if (control.IsPaused)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in a paused state (IsPaused=true).
        /// </summary>
        public object PausedContent
        {
            get { return GetValue(PausedContentProperty); }
            set { SetValue(PausedContentProperty, value); }
        }


        /// <summary>
        /// Identifies the PlayingContent dependency property.
        /// </summary>
        public static DependencyProperty PlayingContentProperty { get { return playingContentProperty; } }
        static readonly DependencyProperty playingContentProperty = DependencyProperty.Register("PlayingContent", typeof(object), typeof(PlayPauseButton), new PropertyMetadata(null, OnPlayingContentChanged));

        static void OnPlayingContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayPauseButton;
            if (!control.IsPaused)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in an playing state (IsPaused=false).
        /// </summary>
        public object PlayingContent
        {
            get { return GetValue(PlayingContentProperty); }
            set { SetValue(PlayingContentProperty, value); }
        }

        #endregion

        #region Name
        /// <summary>
        /// Identifies the PausedName dependency property.
        /// </summary>
        public static DependencyProperty PausedNameProperty { get { return pausedNameProperty; } }
        static readonly DependencyProperty pausedNameProperty = DependencyProperty.Register("PausedName", typeof(string), typeof(PlayPauseButton), new PropertyMetadata(null, OnPausedNameChanged));

        static void OnPausedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayPauseButton;
            var newValue = e.NewValue as string;
            if (control.IsPaused)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an paused state (IsPaused=true).
        /// </summary>
        public string PausedName
        {
            get { return GetValue(PausedNameProperty) as string; }
            set { SetValue(PausedNameProperty, value); }
        }

        /// <summary>
        /// Identifies the PlayingName dependency property.
        /// </summary>
        public static DependencyProperty PlayingNameProperty { get { return playingNameProperty; } }
        static readonly DependencyProperty playingNameProperty = DependencyProperty.Register("PlayingName", typeof(string), typeof(PlayPauseButton), new PropertyMetadata(null, OnPlayingNameChanged));

        static void OnPlayingNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PlayPauseButton;
            var newValue = e.NewValue as string;
            if (!control.IsPaused)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an playing state (IsPaused=false).
        /// </summary>
        public string PlayingName
        {
            get { return GetValue(PlayingNameProperty) as string; }
            set { SetValue(PlayingNameProperty, value); }
        }
        #endregion
    }

    /// <summary>
    /// Represents a fullscreen toggle button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class FullScreenButton : Button
    {
        /// <summary>
        /// Creates a new instance of FullScreenButton.
        /// </summary>
        public FullScreenButton()
        {
            this.DefaultStyleKey = typeof(FullScreenButton);

            Command = ViewModelCommandFactory.CreateFullScreenCommand();

            FullScreenName = MediaPlayer.GetResourceString("ExitFullScreenButtonLabel");
            NotFullScreenName = MediaPlayer.GetResourceString("FullScreenButtonLabel");
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(FullScreenButton), new PropertyMetadata(null, (s, d) => ((FullScreenButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));
        
        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
            this.SetBinding(FullScreenButton.IsFullScreenProperty, new Binding() { Path = new PropertyPath("IsFullScreen"), Source = newValue });
        }

        #region Content

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static DependencyProperty IsFullScreenProperty { get { return isFullScreenProperty; } }
        static readonly DependencyProperty isFullScreenProperty = DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(FullScreenButton), new PropertyMetadata(false, OnIsFullScreenChanged));

        static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FullScreenButton;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                control.Content = control.FullScreenContent ?? control.Content;
                AutomationProperties.SetName(control, control.FullScreenName ?? AutomationProperties.GetName(control));
            }
            else
            {
                control.Content = control.NotFullScreenContent ?? control.Content;
                AutomationProperties.SetName(control, control.NotFullScreenName ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets whether the button is in a selected state. This can impact UI aspects of the control.
        /// </summary>
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        /// <summary>
        /// Identifies the FullScreenContent dependency property.
        /// </summary>
        public static DependencyProperty FullScreenContentProperty { get { return fullScreenContentProperty; } }
        static readonly DependencyProperty fullScreenContentProperty = DependencyProperty.Register("FullScreenContent", typeof(object), typeof(FullScreenButton), new PropertyMetadata(null, OnFullScreenContentChanged));

        static void OnFullScreenContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FullScreenButton;
            if (control.IsFullScreen)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in a selected state (IsFullScreen=true).
        /// </summary>
        public object FullScreenContent
        {
            get { return GetValue(FullScreenContentProperty); }
            set { SetValue(FullScreenContentProperty, value); }
        }


        /// <summary>
        /// Identifies the NotFullScreenContent dependency property.
        /// </summary>
        public static DependencyProperty NotFullScreenContentProperty { get { return notFullScreenContentProperty; } }
        static readonly DependencyProperty notFullScreenContentProperty = DependencyProperty.Register("NotFullScreenContent", typeof(object), typeof(FullScreenButton), new PropertyMetadata(null, OnNotFullScreenContentChanged));

        static void OnNotFullScreenContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FullScreenButton;
            if (!control.IsFullScreen)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in an notFullScreen state (IsFullScreen=false).
        /// </summary>
        public object NotFullScreenContent
        {
            get { return GetValue(NotFullScreenContentProperty); }
            set { SetValue(NotFullScreenContentProperty, value); }
        }

        #endregion

        #region Name
        /// <summary>
        /// Identifies the FullScreenName dependency property.
        /// </summary>
        public static DependencyProperty FullScreenNameProperty { get { return fullScreenNameProperty; } }
        static readonly DependencyProperty fullScreenNameProperty = DependencyProperty.Register("FullScreenName", typeof(string), typeof(FullScreenButton), new PropertyMetadata(null, OnFullScreenNameChanged));

        static void OnFullScreenNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FullScreenButton;
            var newValue = e.NewValue as string;
            if (control.IsFullScreen)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an selected state (IsFullScreen=true).
        /// </summary>
        public string FullScreenName
        {
            get { return GetValue(FullScreenNameProperty) as string; }
            set { SetValue(FullScreenNameProperty, value); }
        }

        /// <summary>
        /// Identifies the NotFullScreenName dependency property.
        /// </summary>
        public static DependencyProperty NotFullScreenNameProperty { get { return notFullScreenNameProperty; } }
        static readonly DependencyProperty notFullScreenNameProperty = DependencyProperty.Register("NotFullScreenName", typeof(string), typeof(FullScreenButton), new PropertyMetadata(null, OnNotFullScreenNameChanged));

        static void OnNotFullScreenNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FullScreenButton;
            var newValue = e.NewValue as string;
            if (!control.IsFullScreen)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an notFullScreen state (IsFullScreen=false).
        /// </summary>
        public string NotFullScreenName
        {
            get { return GetValue(NotFullScreenNameProperty) as string; }
            set { SetValue(NotFullScreenNameProperty, value); }
        }
        #endregion
    }

    /// <summary>
    /// Represents a mute toggle button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class MuteButton : Button
    {
        /// <summary>
        /// Creates a new instance of MuteButton.
        /// </summary>
        public MuteButton()
        {
            this.DefaultStyleKey = typeof(MuteButton);

            Command = ViewModelCommandFactory.CreateMuteCommand();

            MutedName = MediaPlayer.GetResourceString("UnmuteButtonLabel");
            UnmutedName = MediaPlayer.GetResourceString("MuteButtonLabel");
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(MuteButton), new PropertyMetadata(null, (s, d) => ((MuteButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
            this.SetBinding(MuteButton.IsMutedProperty, new Binding() { Path = new PropertyPath("IsMuted"), Source = newValue });
        }

        #region Content

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static DependencyProperty IsMutedProperty { get { return isMutedProperty; } }
        static readonly DependencyProperty isMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(MuteButton), new PropertyMetadata(false, OnIsMutedChanged));

        static void OnIsMutedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MuteButton;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                control.Content = control.MutedContent ?? control.Content;
                AutomationProperties.SetName(control, control.MutedName ?? AutomationProperties.GetName(control));
            }
            else
            {
                control.Content = control.UnmutedContent ?? control.Content;
                AutomationProperties.SetName(control, control.UnmutedName ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets whether the button is in a Muted state. This can impact UI aspects of the control.
        /// </summary>
        public bool IsMuted
        {
            get { return (bool)GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
        }

        /// <summary>
        /// Identifies the MutedContent dependency property.
        /// </summary>
        public static DependencyProperty MutedContentProperty { get { return mutedContentProperty; } }
        static readonly DependencyProperty mutedContentProperty = DependencyProperty.Register("MutedContent", typeof(object), typeof(MuteButton), new PropertyMetadata(null, OnMutedContentChanged));

        static void OnMutedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MuteButton;
            if (control.IsMuted)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in a Muted state (IsMuted=true).
        /// </summary>
        public object MutedContent
        {
            get { return GetValue(MutedContentProperty); }
            set { SetValue(MutedContentProperty, value); }
        }


        /// <summary>
        /// Identifies the UnmutedContent dependency property.
        /// </summary>
        public static DependencyProperty UnmutedContentProperty { get { return unmutedContentProperty; } }
        static readonly DependencyProperty unmutedContentProperty = DependencyProperty.Register("UnmutedContent", typeof(object), typeof(MuteButton), new PropertyMetadata(null, OnUnmutedContentChanged));

        static void OnUnmutedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MuteButton;
            if (!control.IsMuted)
            {
                control.Content = e.NewValue ?? control.Content;
            }
        }

        /// <summary>
        /// Gets or sets the content of the button when in an Unmuted state (IsMuted=false).
        /// </summary>
        public object UnmutedContent
        {
            get { return GetValue(UnmutedContentProperty); }
            set { SetValue(UnmutedContentProperty, value); }
        }

        #endregion

        #region Name
        /// <summary>
        /// Identifies the MutedName dependency property.
        /// </summary>
        public static DependencyProperty MutedNameProperty { get { return mutedNameProperty; } }
        static readonly DependencyProperty mutedNameProperty = DependencyProperty.Register("MutedName", typeof(string), typeof(MuteButton), new PropertyMetadata(null, OnMutedNameChanged));

        static void OnMutedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MuteButton;
            var newValue = e.NewValue as string;
            if (control.IsMuted)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an Muted state (IsMuted=true).
        /// </summary>
        public string MutedName
        {
            get { return GetValue(MutedNameProperty) as string; }
            set { SetValue(MutedNameProperty, value); }
        }

        /// <summary>
        /// Identifies the UnmutedName dependency property.
        /// </summary>
        public static DependencyProperty UnmutedNameProperty { get { return unmutedNameProperty; } }
        static readonly DependencyProperty unmutedNameProperty = DependencyProperty.Register("UnmutedName", typeof(string), typeof(MuteButton), new PropertyMetadata(null, OnUnmutedNameChanged));

        static void OnUnmutedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MuteButton;
            var newValue = e.NewValue as string;
            if (!control.IsMuted)
            {
                AutomationProperties.SetName(control, newValue ?? AutomationProperties.GetName(control));
            }
        }

        /// <summary>
        /// Gets or sets the name of the button when in an Unmuted state (IsMuted=false).
        /// </summary>
        public string UnmutedName
        {
            get { return GetValue(UnmutedNameProperty) as string; }
            set { SetValue(UnmutedNameProperty, value); }
        }
        #endregion
    }

    /// <summary>
    /// Represents a slow motion toggle button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class SlowMotionButton : ToggleButton
    {
        /// <summary>
        /// Creates a new instance of SlowMotionButton.
        /// </summary>
        public SlowMotionButton()
        {
            this.DefaultStyleKey = typeof(SlowMotionButton);

            Command = ViewModelCommandFactory.CreateSlowMotionCommand();

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("SlowMotionButtonLabel"));
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(SlowMotionButton), new PropertyMetadata(null, (s, d) => ((SlowMotionButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
            this.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("IsSlowMotion"), Source = newValue });
        }
    }

    /// <summary>
    /// Represents a play button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class PlayButton : Button
    {
        /// <summary>
        /// Creates a new instance of PlayButton.
        /// </summary>
        public PlayButton()
        {
            this.DefaultStyleKey = typeof(PlayButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("PlayButtonLabel"));
            Command = ViewModelCommandFactory.CreatePlayResumeCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(PlayButton), new PropertyMetadata(null, (s, d) => ((PlayButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a pause button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class PauseButton : Button
    {
        /// <summary>
        /// Creates a new instance of PauseButton.
        /// </summary>
        public PauseButton()
        {
            this.DefaultStyleKey = typeof(PauseButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("PauseButtonLabel"));
            Command = ViewModelCommandFactory.CreatePauseCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(PauseButton), new PropertyMetadata(null, (s, d) => ((PauseButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a caption selection button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class CaptionSelectionButton : Button
    {
        /// <summary>
        /// Creates a new instance of CaptionSelectionButton.
        /// </summary>
        public CaptionSelectionButton()
        {
            this.DefaultStyleKey = typeof(CaptionSelectionButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("CaptionSelectionButtonLabel"));
            Command = ViewModelCommandFactory.CreateCaptionsCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(CaptionSelectionButton), new PropertyMetadata(null, (s, d) => ((CaptionSelectionButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a go live button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class GoLiveButton : Button
    {
        /// <summary>
        /// Creates a new instance of GoLiveButton.
        /// </summary>
        public GoLiveButton()
        {
            this.DefaultStyleKey = typeof(GoLiveButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("GoLiveButtonLabel"));
            Command = ViewModelCommandFactory.CreateGoLiveCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(GoLiveButton), new PropertyMetadata(null, (s, d) => ((GoLiveButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents an audio stream selection button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class AudioSelectionButton : Button
    {
        /// <summary>
        /// Creates a new instance of AudioSelectionButton.
        /// </summary>
        public AudioSelectionButton()
        {
            this.DefaultStyleKey = typeof(AudioSelectionButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("AudioSelectionButtonLabel"));
            Command = ViewModelCommandFactory.CreateAudioSelectionCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(AudioSelectionButton), new PropertyMetadata(null, (s, d) => ((AudioSelectionButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a skip back button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class SkipBackButton : Button
    {
        /// <summary>
        /// Creates a new instance of SkipBackButton.
        /// </summary>
        public SkipBackButton()
        {
            this.DefaultStyleKey = typeof(SkipBackButton);
            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("SkipBackButtonLabel"));

            Command = ViewModelCommandFactory.CreateSkipBackCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(SkipBackButton), new PropertyMetadata(null, (s, d) => ((SkipBackButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a skip ahead button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class SkipAheadButton : Button
    {
        /// <summary>
        /// Creates a new instance of SkipAheadButton.
        /// </summary>
        public SkipAheadButton()
        {
            this.DefaultStyleKey = typeof(SkipAheadButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("SkipAheadButtonLabel"));

            Command = ViewModelCommandFactory.CreateSkipAheadCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(SkipAheadButton), new PropertyMetadata(null, (s, d) => ((SkipAheadButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a time elapsed + skip back button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class TimeElapsedButton : Button
    {
        string skipBackPointerOverStringFormat;
        Binding contentHoverBinding;
        Binding contentUnhoverBinding;

        /// <summary>
        /// Creates a new instance of TimeElapsedButton.
        /// </summary>
        public TimeElapsedButton()
        {
            this.DefaultStyleKey = typeof(TimeElapsedButton);

            skipBackPointerOverStringFormat = MediaPlayer.GetResourceString("SkipBackPointerOverStringFormat");

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("TimeElapsedButtonLabel"));

            Command = ViewModelCommandFactory.CreateSkipBackCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(TimeElapsedButton), new PropertyMetadata(null, (s, d) => ((TimeElapsedButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;

            contentHoverBinding = new Binding() { Path = new PropertyPath("SkipBackInterval"), Source = newValue, Converter = new StringFormatConverter() { StringFormat = skipBackPointerOverStringFormat } };
            contentUnhoverBinding = new Binding() { Path = new PropertyPath("Position"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null };

#if SILVERLIGHT
            if (IsMouseOver)
#else
            if (IsPointerOver)
#endif
            {
                this.SetBinding(Button.ContentProperty, contentHoverBinding);
            }
            else
            {
                this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
            }
        }

#if SILVERLIGHT
        /// <inheritdoc />
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#else
        /// <inheritdoc />
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#endif
    }

    /// <summary>
    /// Represents a total duration + skip ahead button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class DurationButton : Button
    {
        string skipAheadPointerOverStringFormat;
        Binding contentHoverBinding;
        Binding contentUnhoverBinding;

        /// <summary>
        /// Creates a new instance of DurationButton.
        /// </summary>
        public DurationButton()
        {
            this.DefaultStyleKey = typeof(DurationButton);

            skipAheadPointerOverStringFormat = MediaPlayer.GetResourceString("SkipAheadPointerOverStringFormat");

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("DurationButtonLabel"));

            Command = ViewModelCommandFactory.CreateSkipAheadCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(DurationButton), new PropertyMetadata(null, (s, d) => ((DurationButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;

            contentHoverBinding = new Binding() { Path = new PropertyPath("SkipAheadInterval"), Source = newValue, Converter = new StringFormatConverter() { StringFormat = skipAheadPointerOverStringFormat } };
            contentUnhoverBinding = new Binding() { Path = new PropertyPath("Duration"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null };

#if SILVERLIGHT
            if (IsMouseOver)
#else
            if (IsPointerOver)
#endif
            {
                this.SetBinding(Button.ContentProperty, contentHoverBinding);
            }
            else
            {
                this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
            }
        }

#if SILVERLIGHT
        /// <inheritdoc />
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#else
        /// <inheritdoc />
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#endif
    }

    /// <summary>
    /// Represents a time remaining + skip ahead button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class TimeRemainingButton : Button
    {
        string skipAheadPointerOverStringFormat;
        Binding contentHoverBinding;
        Binding contentUnhoverBinding;

        /// <summary>
        /// Creates a new instance of TimeRemainingButton.
        /// </summary>
        public TimeRemainingButton()
        {
            this.DefaultStyleKey = typeof(TimeRemainingButton);

            skipAheadPointerOverStringFormat = MediaPlayer.GetResourceString("SkipAheadPointerOverStringFormat");

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("TimeRemainingButtonLabel"));

            Command = ViewModelCommandFactory.CreateSkipAheadCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(TimeRemainingButton), new PropertyMetadata(null, (s, d) => ((TimeRemainingButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;

            contentHoverBinding = new Binding() { Path = new PropertyPath("SkipAheadInterval"), Source = newValue, Converter = new StringFormatConverter() { StringFormat = skipAheadPointerOverStringFormat } };
            contentUnhoverBinding = new Binding() { Path = new PropertyPath("TimeRemaining"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null };

#if SILVERLIGHT
            if (IsMouseOver)
#else
            if (IsPointerOver)
#endif
            {
                this.SetBinding(Button.ContentProperty, contentHoverBinding);
            }
            else
            {
                this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
            }
        }

#if SILVERLIGHT
        /// <inheritdoc />
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#else
        /// <inheritdoc />
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            this.SetBinding(Button.ContentProperty, contentHoverBinding);
        }

        /// <inheritdoc />
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            this.SetBinding(Button.ContentProperty, contentUnhoverBinding);
        }
#endif
    }

    /// <summary>
    /// Represents a total duration textblock that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class TotalDuration : ContentControl
    {
        /// <summary>
        /// Creates a new instance of TotalDuration.
        /// </summary>
        public TotalDuration()
        {
            this.DefaultStyleKey = typeof(TotalDuration);
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(TotalDuration), new PropertyMetadata(null, (s, d) => ((TotalDuration)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            this.SetBinding(ContentControl.ContentProperty, new Binding() { Path = new PropertyPath("Duration"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null });
        }
    }

    /// <summary>
    /// Represents a time elapsed textblock that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class TimeElapsed : ContentControl
    {
        /// <summary>
        /// Creates a new instance of TimeElapsed.
        /// </summary>
        public TimeElapsed()
        {
            this.DefaultStyleKey = typeof(TimeElapsed);
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(TimeElapsed), new PropertyMetadata(null, (s, d) => ((TimeElapsed)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            this.SetBinding(ContentControl.ContentProperty, new Binding() { Path = new PropertyPath("Position"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null });
        }
    }

    /// <summary>
    /// Represents a time remaining textblock that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class TimeRemaining : ContentControl
    {
        /// <summary>
        /// Creates a new instance of TimeRemaining.
        /// </summary>
        public TimeRemaining()
        {
            this.DefaultStyleKey = typeof(TimeRemaining);
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(TimeRemaining), new PropertyMetadata(null, (s, d) => ((TimeRemaining)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            this.SetBinding(ContentControl.ContentProperty, new Binding() { Path = new PropertyPath("TimeRemaining"), Source = newValue, Converter = newValue != null ? newValue.TimeFormatConverter : null });
        }
    }

    /// <summary>
    /// Represents a skip to previous marker/playlist item button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class SkipPreviousButton : Button
    {
        /// <summary>
        /// Creates a new instance of SkipPreviousButton.
        /// </summary>
        public SkipPreviousButton()
        {
            this.DefaultStyleKey = typeof(SkipPreviousButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("SkipPreviousButtonLabel"));
            Command = ViewModelCommandFactory.CreateSkipPreviousCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(SkipPreviousButton), new PropertyMetadata(null, (s, d) => ((SkipPreviousButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a skip to next marker/playlist item button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class SkipNextButton : Button
    {
        /// <summary>
        /// Creates a new instance of SkipNextButton.
        /// </summary>
        public SkipNextButton()
        {
            this.DefaultStyleKey = typeof(SkipNextButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("SkipNextButtonLabel"));
            Command = ViewModelCommandFactory.CreateSkipNextCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(SkipNextButton), new PropertyMetadata(null, (s, d) => ((SkipNextButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a stop button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class StopButton : Button
    {
        /// <summary>
        /// Creates a new instance of StopButton.
        /// </summary>
        public StopButton()
        {
            this.DefaultStyleKey = typeof(StopButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("StopButtonLabel"));
            Command = ViewModelCommandFactory.CreateStopCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(StopButton), new PropertyMetadata(null, (s, d) => ((StopButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a rewind button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class RewindButton : Button
    {
        /// <summary>
        /// Creates a new instance of RewindButton.
        /// </summary>
        public RewindButton()
        {
            this.DefaultStyleKey = typeof(RewindButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("RewindButtonLabel"));
            Command = ViewModelCommandFactory.CreateRewindCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(RewindButton), new PropertyMetadata(null, (s, d) => ((RewindButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents a fast forward button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class FastForwardButton : Button
    {
        /// <summary>
        /// Creates a new instance of FastForwardButton.
        /// </summary>
        public FastForwardButton()
        {
            this.DefaultStyleKey = typeof(FastForwardButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("FastForwardButtonLabel"));
            Command = ViewModelCommandFactory.CreateFastForwardCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(FastForwardButton), new PropertyMetadata(null, (s, d) => ((FastForwardButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }

    /// <summary>
    /// Represents an instant replay button that can be bound to MediaPlayer.InteractiveViewModel
    /// </summary>
    public sealed class ReplayButton : Button
    {
        /// <summary>
        /// Creates a new instance of ReplayButton.
        /// </summary>
        public ReplayButton()
        {
            this.DefaultStyleKey = typeof(ReplayButton);

            AutomationProperties.SetName(this, MediaPlayer.GetResourceString("ReplayButtonLabel"));
            Command = ViewModelCommandFactory.CreateReplayCommand();
        }

        /// <summary>
        /// Identifies the ViewModel dependency property.
        /// </summary>
        public static DependencyProperty ViewModelProperty { get { return viewModelProperty; } }
        static readonly DependencyProperty viewModelProperty = DependencyProperty.Register("ViewModel", typeof(IInteractiveViewModel), typeof(ReplayButton), new PropertyMetadata(null, (s, d) => ((ReplayButton)s).OnViewModelChanged(d.OldValue as IInteractiveViewModel, d.NewValue as IInteractiveViewModel)));

        /// <summary>
        /// Gets or sets the InteractiveViewModel object used to provide state updates and serve user interaction requests.
        /// This is usually an instance of the MediaPlayer but could be a custom implementation to support unique interaction such as in the case of advertising.
        /// </summary>
        public IInteractiveViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as IInteractiveViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        void OnViewModelChanged(IInteractiveViewModel oldValue, IInteractiveViewModel newValue)
        {
            var vmCommand = Command as IViewModelCommand;
            if (vmCommand != null) vmCommand.ViewModel = newValue;
        }
    }
}
