using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.TimedText
{
    /// <summary>
    /// Defines the functionality a class must implement to be a MarkerParser.
    /// </summary>
    internal interface IMarkerParser
    {
        /// <summary>
        /// Accepts an XML representation of markers and returns an IEnumerable collection of MediaMarkers.
        /// </summary>
        /// <param name="markerXml">An XML document defining a DFXP data chunk.</param>
        /// <param name="timeOffset">The default start time of the DFXP container.</param>
        /// <param name="defaultEndTime">The default end time of the DFXP container.</param>
        /// <returns>A collection of MediaMarker objects.</returns>
        IEnumerable<CaptionRegion> ParseMarkerCollection(XDocument markerXml, TimeSpan timeOffset, TimeSpan defaultEndTime);
    }
}