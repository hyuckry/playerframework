#define CODE_ANALYSIS

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
#if SILVERLIGHT
#else
using System.Reflection;
using Windows.UI.Xaml;
using System.Threading.Tasks;
#endif

namespace Microsoft.PlayerFramework
{
    /// <summary>
    /// A factory class to help load MMP: Player Framework plugins.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
    public sealed class PluginsFactory
    {
        internal PluginsFactory()
        {
        }
       
        internal void ImportPlugins()
        {
            Plugins = new List<IPlugin>(new IPlugin[] { 
                new BufferingPlugin(),
                new CaptionSelectorPlugin(),
                new AudioSelectionPlugin(),
                new ChaptersPlugin(),
                new ErrorPlugin(),
                new LoaderPlugin(),
#if SILVERLIGHT
                new PosterPlugin(),
#elif NETFX_CORE
                new MediaControlPlugin(),
#endif
            });
        }

        /// <summary>
        /// The plugins to get imported.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Correctly named architectural pattern")]
        public IEnumerable<IPlugin> Plugins { get; private set; }
    }
}
