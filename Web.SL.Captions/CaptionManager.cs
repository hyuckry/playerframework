using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.PlayerFramework.Captions
{
    public class CaptionManager : MarkerManager<CaptionMarker>
    {
        public event EventHandler<CaptionCueEventArgs> ShowCaption;
        public event EventHandler<CaptionCueEventArgs> HideCaption;

        protected override void OnMarkerReached(CaptionMarker marker, bool seeked, int direction)
        {
            if (ShowCaption != null) ShowCaption(this, new CaptionCueEventArgs(marker));
        }

        protected override void OnMarkerLeft(CaptionMarker marker, bool seeked, int direction)
        {
            if (HideCaption != null) HideCaption(this, new CaptionCueEventArgs(marker));
        }

        protected override void OnMarkerSkipped(CaptionMarker marker, int direction)
        {

        }

        protected override void OnActiveMarkersChanged()
        {

        }
    }

    public class CaptionCueEventArgs : EventArgs
    {
        public CaptionCueEventArgs(CaptionMarker caption)
        {
            Caption = caption;
        }

        public CaptionMarker Caption { get; private set; }
    }
}
