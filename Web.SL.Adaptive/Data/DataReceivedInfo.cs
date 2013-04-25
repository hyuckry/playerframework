using System;
using System.Collections.Generic;
using Microsoft.Web.Media.SmoothStreaming;

namespace Microsoft.PlayerFramework.Adaptive
{
    public class DataReceivedInfo : EventArgs
    {
        internal DataReceivedInfo(byte[] Data, ChunkInfo ChunkInfo, TrackInfo TrackInfo, StreamInfo StreamInfo) 
        {
            this.Data = Data;
            this.ChunkInfo = ChunkInfo;
            this.TrackInfo = TrackInfo;
            this.StreamInfo = StreamInfo;
        }

        public byte[] Data { get; private set; }
        public ChunkInfo ChunkInfo { get; private set; }
        public TrackInfo TrackInfo { get; private set; }
        public StreamInfo StreamInfo { get; private set; }
    }
}
