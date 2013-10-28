using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace ResourceViewerPlugin
{
    /// <summary>
    ///     Interaction logic for PreviewImageView.xaml
    /// </summary>
    public partial class PreviewImageView : Window, INotifyPropertyChanged
    {
        private BitmapContainer _image;

        public PreviewImageView()
        {
            InitializeComponent();
        }


        public BitmapContainer Image
        {
            get { return _image; }
            set { Set("Image", ref _image, value); }
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

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Image.ShowExportDialog();
        }
    }
}