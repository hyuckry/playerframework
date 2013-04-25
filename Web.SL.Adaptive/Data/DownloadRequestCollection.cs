using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.PlayerFramework.Adaptive
{
    internal class DownloadRequestCollection : OrderedObservableCollection<DownloadRequest>
    {
        public new void Add(DownloadRequest item)
        {
            var comparable = new DownloadRequestComparable(item.ChunkTimestamp);
            Add(item, comparable);
        }

        public IEnumerable<DownloadRequest> WhereAfterPosition(TimeSpan position, int? count = null)
        {
            var comparer = new DownloadRequestComparable(position);
            var startIndex = SearchForInsertIndex(comparer);

            return count.HasValue
                    ? this.Skip(startIndex).Take(count.Value).ToList()
                    : this.Skip(startIndex).ToList();
        }

        private class DownloadRequestComparable : IComparable<DownloadRequest>
        {
            private TimeSpan _searchTime;

            public DownloadRequestComparable(TimeSpan beginTime)
            {
                _searchTime = beginTime;
            }

            public int CompareTo(DownloadRequest other)
            {
                return _searchTime.CompareTo(other.ChunkTimestamp);
            }
        }
    }
}
