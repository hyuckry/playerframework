using System;
using Microsoft.VideoAdvertising;

namespace Microsoft.PlayerFramework.Advertising
{
    // TODO: make DependencyObjects with DependencyProperties for better Xaml binding
    /// <summary>
    /// Represents a midroll ad to be played during the main content.
    /// </summary>
    public sealed class MidrollAdvertisement : IAdvertisement
    {
        /// <summary>
        /// Creates a new instance of MidrollAdvertisement
        /// </summary>
        public MidrollAdvertisement()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// The time when the midroll should be played.
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// The time (specified as a percentage of the media duration) when the midroll should be played.
        /// </summary>
        public double? TimePercentage { get; set; }

        /// <summary>
        /// This is is for advanced use cases to help indicate the duration of the ad slot (this doesn't necessarily have to be the duration of the ad).
        /// This helps in situations where the ad is inserted over the top of a blank spot in the main content and allows seeking the video past the blank spot when the ad is complete.
        /// Warning: Set to TimeSpan.Zero if your main content is not altered to contain blank video during ad slots.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <inheritdoc /> 
        public IAdSource Source { get; set; }

        /// <inheritdoc /> 
        public string Id { get; set; }
    }

    /// <summary>
    /// Represents a preroll ad to be played before the main content starts.
    /// </summary>
    public sealed class PrerollAdvertisement : IAdvertisement
    {
        /// <summary>
        /// Creates a new instance of PrerollAdvertisement
        /// </summary>
        public PrerollAdvertisement()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <inheritdoc /> 
        public IAdSource Source { get; set; }

        /// <inheritdoc /> 
        public string Id { get; set; }
    }

    /// <summary>
    /// Represents a postroll ad to be played after the main content finishes.
    /// </summary>
    public sealed class PostrollAdvertisement : IAdvertisement
    {
        /// <summary>
        /// Creates a new instance of PostrollAdvertisement
        /// </summary>
        public PostrollAdvertisement()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <inheritdoc /> 
        public IAdSource Source { get; set; }

        /// <inheritdoc /> 
        public string Id { get; set; }
    }
}
