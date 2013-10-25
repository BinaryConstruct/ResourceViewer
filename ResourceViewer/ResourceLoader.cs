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
using JustDecompile.API.Core;

namespace ResourceViewerPlugin
{
    public class ResourceLoader
    {

		private static ICollection<IResource> CollectResources(IAssemblyDefinition assembly)
		{
			List<IResource> result = new List<IResource>();

			foreach (IResource resource in assembly.MainModule.Resources)
			{
				result.Add(resource);
			}

			return result;
		}

        public static Task<IList<BitmapContainer>> LoadBitmapsFromAssembly(IAssemblyDefinition assembly, Action<int> progressChangedCallback = null)
        {
            return Task.Factory.StartNew<IList<BitmapContainer>>(() =>
            {
                var bitmaps = new List<BitmapContainer>();

				ICollection<IResource> resources = CollectResources(assembly);

                int total = resources.Count;

				int progressCount = 0;
                //for (int i = 0; i < resources.Count; i++)

				foreach (IResource resource in resources)
                {
                    if (progressChangedCallback != null)
                        progressChangedCallback.Invoke((int)(100.0 * progressCount / total));

                    if (string.IsNullOrWhiteSpace(resource.Name)) continue;

					if (resource.ResourceType != ResourceType.Embedded) continue;

					IEmbeddedResource embeddedResource = resource as IEmbeddedResource;
					if (embeddedResource == null) continue;

                    if (resource.Name.EndsWith(".resources"))
                    {
                        using (ResourceReader resourceReader = new ResourceReader(embeddedResource.GetResourceStream()))
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
									BitmapSource bitmap = LoadBitmapImage(embeddedResource.GetResourceStream());

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
						var bitmap = LoadBitmapImage(embeddedResource.GetResourceStream());

                        bitmaps.Add(new BitmapContainer
                        {
                            Name = resource.Name,
                            Bitmap = bitmap
                        });
                    }
                    catch
                    {
                        /*not a bitmap */
                    }

					progressCount++;
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