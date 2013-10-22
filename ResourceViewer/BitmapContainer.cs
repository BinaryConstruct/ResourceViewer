using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ResourceViewerPlugin
{
    public class BitmapContainer : INotifyPropertyChanged
    {
        private BitmapSource _bitmap;
        private string _name;


        public BitmapSource Bitmap
        {
            get { return _bitmap; }
            set { Set("Bitmap", ref _bitmap, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(string name, ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        #endregion
    }
}