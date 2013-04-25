using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Microsoft.PlayerFramework.Captions
{
    public partial class CaptionsSelectionPanel : UserControl
    {
        public event EventHandler Closed;
        public event EventHandler Selected;

        public CaptionsSelectionPanel()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CaptionsList.Items.IndexOf(e.AddedItems[0]) == CaptionsList.Items.Count - 1)
            {
                if (Closed != null) Closed(this, EventArgs.Empty);
            }
            else
            {
                if (Selected != null) Selected(this, EventArgs.Empty);
            }
        }
    }
}
