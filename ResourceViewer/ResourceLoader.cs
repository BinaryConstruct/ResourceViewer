using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ResourceViewerPlugin
{
    public class ResourceLoader
    {
        public static Task<IList<BitmapContainer>> LoadBitmapsFromAssembly(string assemblyFile, Action<int> progressChangedCallback = null)
        {
            return Task.Factory.StartNew<IList<BitmapContainer>>(() =>
            {
                var bitmaps = new List<BitmapContainer>();
                Assembly loadAssembly = Assembly.LoadFile(assemblyFile);

                var resourceNames = loadAssembly.GetManifestResourceNames();

                int total = resourceNames.Length;
                for (int i = 0; i < resourceNames.Length; i++)
                {
                    if (progressChangedCallback != null)
                        progressChangedCallback.Invoke((int)(100.0 * i / total));

                    string resource = resourceNames[i];

                    if (string.IsNullOrWhiteSpace(resource)) continue;

                    if (resource.EndsWith(".resources"))
                    {
                        using (ResourceReader resourceReader = new ResourceReader(loadAssembly.GetManifestResourceStream(resource)))
                        {
                            var iterator = resourceReader.GetEnumerator();
                            while (iterator.MoveNext())
                            {
                                string rkey = iterator.Key as string;

                                if (string.IsNullOrWhiteSpace(rkey)) continue;

                                //Type rType = iterator.Value.GetType();

                                if (iterator.Value is string) continue;

                                Bitmap value = iterator.Value as Bitmap;
                                if (value != null)
                                {
                                    bitmaps.Add(new BitmapContainer
                                    {
                                        Name = rkey,
                                        Bitmap = Bitmap2BitmapImage(value)
                                    });

                                    continue;
                                }

                                try
                                {
                                    BitmapSource bitmap = LoadBitmapImage(loadAssembly.GetManifestResourceStream(rkey));

                                    bitmaps.Add(new BitmapContainer
                                    {
                                        Name = rkey,
                                        Bitmap = bitmap
                                    });
                                }
                                catch
                                {
                                    /*not a bitmap */
                                }
                            }
                        }
                    }

                    try
                    {
                        var bitmap = LoadBitmapImage(loadAssembly.GetManifestResourceStream(resource));

                        bitmaps.Add(new BitmapContainer
                        {
                            Name = resource,
                            Bitmap = bitmap
                        });
                    }
                    catch
                    {
                        /*not a bitmap */
                    }
                }
                return bitmaps;
            });
        }

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource LoadBitmapImage(Stream bmpStream)
        {
            var bmp = new Bitmap(bmpStream);
            return Bitmap2BitmapImage(bmp);
        }

        public static BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            retval.Freeze();
            return retval;
        }

        public static BitmapSource BitmapImageFromStream(Stream stream)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            stream.Close();

            return bitmap;
        }
    }
}