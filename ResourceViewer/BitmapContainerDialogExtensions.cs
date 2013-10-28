using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ResourceViewerPlugin
{
    public static class BitmapContainerDialogExtensions
    {
        public static void ShowPreviewDialog(this BitmapContainer image)
        {
            PreviewImageView pview = new PreviewImageView();

            pview.Width = Math.Min(800, image.Bitmap.Width);
            pview.Height = Math.Min(640, image.Bitmap.Height) + 24;
            pview.Owner = Application.Current.MainWindow;
            pview.Image = image;
            pview.ShowDialog();
        }

        public static void ShowExportMultipleDialog(this IEnumerable<BitmapContainer> images)
        {
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

                foreach (var bmp in images)
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
            }
        }


        public static void ShowExportDialog(this BitmapContainer image)
        {
            if (image == null) return;

            SaveFileDialog saveImg = new SaveFileDialog();
            saveImg.Filter = "Jpeg Image|*.jpg|Png Image|*.png|Bitmap Image|*.bmp|Tiff Image|*.tiff|Windows Media Bitmap|*.wmb";
            saveImg.Title = "Export Single Image";
            saveImg.FileName = image.Name;
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


                encoder.Frames.Add(BitmapFrame.Create(image.Bitmap));
                using (FileStream fs = new FileStream(saveImg.FileName, FileMode.OpenOrCreate))
                {
                    encoder.Save(fs);
                    fs.Close();
                }
            }
        }
    }
}