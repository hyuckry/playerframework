using Microsoft.VideoAdvertising;
using System;

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// Provides a view model specific to nonlinear VPAID ad players.
    /// This helps the control panel properly display and function during a nonlinear ad.
    /// </summary>
    public sealed partial class VpaidNonLinearAdViewModel
    {
        /// <summary>
        /// HACK: Allows an instance to be created from Xaml. Without this, xamltypeinfo is not generated and binding will not work.
        /// </summary>
        public VpaidNonLinearAdViewModel()
        {
            SkipPreviousThreshold = TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// The VPAID player playing a nonlinear ad.
        /// </summary>
        public IVpaid Vpaid { get; private set; }

        internal VpaidNonLinearAdViewModel(IVpaid vpaid, MediaPlayer mediaPlayer)
            : this()
        {
            MediaPlayer = mediaPlayer;
            Vpaid = vpaid;
        }

        /// <inheritdoc /> 
        public void Pause()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.Pause();
            Vpaid.PauseAd();
        }

        /// <inheritdoc /> 
        public void PlayResume()
        {
            OnInteracting(InteractionType.Hard);
            MediaPlayer.PlayResume();
            Vpaid.ResumeAd();
        }
    }
}
