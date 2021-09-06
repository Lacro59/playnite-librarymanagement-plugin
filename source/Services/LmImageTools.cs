using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LazZiya.ImageResize;
using CommonPluginsPlaynite.Common;
using CommonPluginsShared;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media;
using Playnite.SDK;

namespace LibraryManagement.Services
{
    public class LmImageTools
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly BitmapImage BitmapImageOriginal;
        private Image ImageOriginal;
        private Image ImageEdited;


        public LmImageTools(string ImagePath)
        {
            try
            {
                using (var stream = FileSystem.OpenReadFileStreamSafe(ImagePath))
                {
                    BitmapImageOriginal = BitmapExtensions.BitmapFromStream(stream);
                    ImageOriginal = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
        

        public BitmapImage GetOriginalBitmapImage()
        {
            return BitmapImageOriginal;
        }

        public ImageProperty GetOriginalImageProperty()
        {
            if (ImageOriginal == null)
            {
                return null;
            }

            return ImageTools.GetImapeProperty(ImageOriginal);
        }


        public BitmapImage ApplyImageOptions(int? Width, int? Height, bool? Cropping)
        {
            Width = (Width == null) ? 0 : Width;
            Height = (Height == null) ? 0 : Height;
            Cropping = (Cropping == null) ? false : Cropping;


            if (Width == 0 && Height == 0)
            {
                Width = ImageOriginal.Width;
                Height = ImageOriginal.Height;
            }


            ImageEdited = null;


            if ((bool)Cropping)
            {
                if (Width == 0 || Height == 0)
                {
                    return null;
                }

                ImageEdited = ImageOriginal.ScaleAndCrop((int)Width, (int)Height);
            }
            else
            {
                // Resize
                if (Width == 0)
                {
                    ImageEdited = ImageOriginal.ScaleByHeight((int)Height);
                }
                else if (Height == 0)
                {
                    ImageEdited = ImageOriginal.ScaleByWidth((int)Width);
                }
                else
                {
                    ImageEdited = ImageOriginal.Scale((int)Width, (int)Height);
                }
            }

            if (ImageEdited == null)
            {
                return null;
            }           
            return ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }

        public BitmapImage ApplyImageOptions(string FileTempPath, int? Width, int? Height, bool? Cropping)
        {
            Width = (Width == null) ? 0 : Width;
            Height = (Height == null) ? 0 : Height;
            Cropping = (Cropping == null) ? false : Cropping;

            Image ImageScale = null;
            try
            {
                using (var stream = FileSystem.OpenReadFileStreamSafe(FileTempPath))
                {
                    ImageScale = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }


            if (Width == 0 && Height == 0)
            {
                Width = ImageScale.Width;
                Height = ImageScale.Height;
            }


            ImageEdited = null;


            if ((bool)Cropping)
            {
                if (Width == 0 || Height == 0)
                {
                    return null;
                }

                ImageEdited = ImageScale.ScaleAndCrop((int)Width, (int)Height);
            }
            else
            {
                // Resize
                if (Width == 0)
                {
                    ImageEdited = ImageScale.ScaleByHeight((int)Height);
                }
                else if (Height == 0)
                {
                    ImageEdited = ImageScale.ScaleByWidth((int)Width);
                }
                else
                {
                    ImageEdited = ImageScale.Scale((int)Width, (int)Height);
                }
            }

            if (ImageEdited == null)
            {
                return null;
            }
            return ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }


        public BitmapImage GetEditedBitmapImage()
        {
            if (ImageEdited == null)
            {
                return null;
            }

            return ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }

        public Image GetEditedImage()
        {
            if (ImageEdited == null)
            {
                return null;
            }

            return ImageEdited;
        }

        public void RemoveEditedImage()
        {
            ImageEdited = null;
        }
    }


    public class FileSize
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
