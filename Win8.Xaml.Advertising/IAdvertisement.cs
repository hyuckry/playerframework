using Microsoft.VideoAdvertising;
using System;

namespace Microsoft.PlayerFramework.Advertising
{
    /// <summary>
    /// An interface to represent an advertisement.
    /// </summary>
    public interface IAdvertisement
    {
        /// <summary>
        /// Gets or sets the source for the ad.
        /// </summary>
        IAdSource Source { get; set; }

        /// <summary>
        /// Gets the ID for the ad. This is only used for internal purposes to help associate the ad with a marker.
        /// </summary>
        string Id { get; set; }
    }
}
