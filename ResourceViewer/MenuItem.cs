using System.Collections.Generic;
using System.Windows.Input;
using JustDecompile.API.Core;

namespace ResourceViewerPlugin
{
    internal class MenuItem : IMenuItem
    {
        public MenuItem()
        {
            MenuItems = new List<IMenuItem>();
        }

        public IList<IMenuItem> MenuItems { get; private set; }

        public ICommand Command { get; set; }

        public object Header { get; set; }

        public object Icon { get; set; }
    }
}