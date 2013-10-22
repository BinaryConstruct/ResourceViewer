using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ResourceViewerPlugin
{
    /// <summary>
    ///     Interaction logic for ImageBrowserView.xaml
    /// </summary>
    public partial class ImageBrowserView : UserControl, INotifyPropertyChanged
    {
        private string _assemblyName;
        private ObservableCollection<BitmapContainer> _images = new ObservableCollection<BitmapContainer>();
        private Visibility _progressOverlayVisibility;
        private int _progressPercentage;
        private BitmapContainer _selectedImage;
        private string _status;

        public ImageBrowserView()
        {
            InitializeComponent();
        }

        public BitmapContainer SelectedImage
        {
            get { return _selectedImage; }
            set { Set("SelectedImage", ref _selectedImage, value); }
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
            set { Set("AssemblyName", ref _assemblyName, value); }
        }

        public Visibility ProgressOverlayVisibility
        {
            get { return _progressOverlayVisibility; }
            set { Set("ProgressOverlayVisibility", ref _progressOverlayVisibility, value); }
        }

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set { Set("ProgressPercentage", ref _progressPercentage, value); }
        }

        public string Status
        {
            get { return _status; }
            set { Set("Status", ref _status, value); }
        }

        public ObservableCollection<BitmapContainer> Images
        {
            get { return _images; }
            set { Set("Images", ref _images, value); }
        }

        public event EventHandler Closing;


        protected virtual void OnClosing(object sender, EventArgs e)
        {
            if (Closing != null) Closing(sender, e);
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            OnClosing(this, EventArgs.Empty);
        }

        private void ViewFullSizeClick(object sender, RoutedEventArgs e)
        {
            PreviewImageView pview = new PreviewImageView();
            pview.Image = SelectedImage;
            pview.ShowDialog();
        }

        private void ExportSelectedClick(object sender, RoutedEventArgs e)
        {
            if (SelectedImage == null) return;

            Status = string.Empty;
            SaveFileDialog saveImg = new SaveFileDialog();
            saveImg.Filter = "Jpeg Image|*.jpg|Png Image|*.png|Bitmap Image|*.bmp|Tiff Image|*.tiff|Windows Media Bitmap|*.wmb";
            saveImg.Title = "Export Single Image";
            saveImg.FileName = SelectedImage.Name;
            saveImg.AddExtension = true;
            saveImg.DefaultExt = "*.png";

            saveImg.CheckPathExists = true;

            BitmapEncoder encoder = null;
            if (saveImg.ShowDialog() == true)
            {
                string ext = Path.GetExtension(saveImg.FileName);
                switch (ext.ToLower())
                {
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".jpeg":
                    case ".jpg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ".wmb":
                        encoder = new WmpBitmapEncoder();
                        break;
                    default:
                        encoder = new BmpBitmapEncoder();
                        saveImg.FileName += ".bmp";
                        break;
                }

                try
                {
                    encoder.Frames.Add(BitmapFrame.Create(SelectedImage.Bitmap));
                    using (FileStream fs = new FileStream(saveImg.FileName, FileMode.OpenOrCreate))
                    {
                        encoder.Save(fs);
                        fs.Close();
                    }
                }
                catch (Exception err)
                {
                    Status = err.Message;
                }
            }
        }

        private void ExportAllClick(object sender, RoutedEventArgs e)
        {
            Status = string.Empty;
            SaveFileDialog saveImg = new SaveFileDialog();
            saveImg.Filter = "Png Image|*.png|Jpeg Image|*.jpg|Bitmap Image|*.bmp|Tiff Image|*.tiff|Windows Media Bitmap|*.wmb";
            saveImg.Title = "Export All Image";
            saveImg.FileName = "[Multiple Files - Choose File Type]";
            saveImg.AddExtension = false;
            saveImg.FilterIndex = 0;

            saveImg.CheckPathExists = true;


            if (saveImg.ShowDialog() == true)
            {
                string ext = saveImg.Filter.Split('|')[saveImg.FilterIndex / 2 + 1].TrimStart('*');
                string path = Path.GetDirectoryName(saveImg.FileName);
                foreach (var bmp in Images)
                {
                    try
                    {
                        BitmapEncoder encoder = null;

                        switch (ext.ToLower())
                        {
                            case ".png":
                                encoder = new PngBitmapEncoder();
                                break;
                            case ".bmp":
                                encoder = new BmpBitmapEncoder();
                                break;
                            case ".jpeg":
                            case ".jpg":
                                encoder = new JpegBitmapEncoder();
                                break;
                            case ".tiff":
                                encoder = new TiffBitmapEncoder();
                                break;
                            case ".wmb":
                                encoder = new WmpBitmapEncoder();
                                break;
                            default:
                                encoder = new BmpBitmapEncoder();
                                saveImg.FileName += ".bmp";
                                break;
                        }

                        encoder.Frames.Add(BitmapFrame.Create(bmp.Bitmap));
                        using (FileStream fs = new FileStream(Path.Combine(path, bmp.Name + ext), FileMode.OpenOrCreate))
                        {
                            encoder.Save(fs);
                            fs.Close();
                        }
                    }
                    catch (Exception err)
                    {
                        Status = err.Message;
                    }
                }
            }
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