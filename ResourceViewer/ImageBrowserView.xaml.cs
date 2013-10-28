﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            SelectedImage.ShowPreviewDialog();
        }

        private void ExportSelectedClick(object sender, RoutedEventArgs e)
        {
            Status = string.Empty;
            try
            {
                SelectedImage.ShowExportDialog();
            }
            catch (Exception err)
            {
                Status = err.Message;
            }
        }

        private void ExportAllClick(object sender, RoutedEventArgs e)
        {
            Status = string.Empty;
            try
            {
                Images.ShowExportMultipleDialog();
            }
            catch (Exception err)
            {
                Status = err.Message;
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