using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using LazZiya.ImageResize;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using Playnite.SDK;

namespace LibraryManagement.Services
{
    public class LmImageTools
    {
        private static ILogger Logger => LogManager.GetLogger();

        private BitmapImage BitmapImageOriginal { get; }
        private Image ImageOriginal { get; set; }
        private Image ImageEdited { get; set; }


        public LmImageTools(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    using (Stream stream = FileSystem.OpenReadFileStreamSafe(imagePath))
                    {
                        BitmapImageOriginal = BitmapExtensions.BitmapFromStream(stream);
                        ImageOriginal = Image.FromStream(stream);
                    }
                }
                else
                {
                    Logger.Warn($"File is not exists : {imagePath}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "LibraryManagement");
            }
        }


        public BitmapImage GetOriginalBitmapImage()
        {
            return BitmapImageOriginal;
        }

        public ImageProperty GetOriginalImageProperty()
        {
            return ImageOriginal == null ? null : ImageTools.GetImapeProperty(ImageOriginal);
        }


        public BitmapImage ApplyImageOptions(int? width, int? height, bool? cropping, bool? flip)
        {
            width = (width == null) ? 0 : width;
            height = (height == null) ? 0 : height;
            cropping = (cropping == null) ? false : cropping;


            if (width == 0 && height == 0)
            {
                width = ImageOriginal.Width;
                height = ImageOriginal.Height;
            }


            ImageEdited = ImageOriginal;

            if ((bool)cropping)
            {
                if (width == 0 || height == 0)
                {
                    return null;
                }

                ImageEdited = ImageOriginal.ScaleAndCrop((int)width, (int)height);
            }
            else
            {
                // Resize
                if (width == 0)
                {
                    ImageEdited = ImageOriginal.ScaleByHeight((int)height);
                }
                else if (height == 0)
                {
                    ImageEdited = ImageOriginal.ScaleByWidth((int)width);
                }
                else
                {
                    ImageEdited = ImageOriginal.Scale((int)width, (int)height);
                }
            }

            if (ImageEdited != null && (bool)flip)
            {
                ImageEdited.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            if (ImageEdited == null)
            {
                return null;
            }           
            return ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }

        public BitmapImage ApplyImageOptions(string fileTempPath, int? width, int? height, bool? cropping, bool? flip)
        {
            width = (width == null) ? 0 : width;
            height = (height == null) ? 0 : height;
            cropping = (cropping == null) ? false : cropping;

            Image ImageScale = null;
            try
            {
                using (Stream stream = FileSystem.OpenReadFileStreamSafe(fileTempPath))
                {
                    ImageScale = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "LibraryManagement");
                return null;
            }


            if (width == 0 && height == 0)
            {
                width = ImageScale.Width;
                height = ImageScale.Height;
            }


            ImageEdited = null;

            if ((bool)cropping)
            {
                if (width == 0 || height == 0)
                {
                    return null;
                }

                ImageEdited = ImageScale.ScaleAndCrop((int)width, (int)height);
            }
            else
            {
                // Resize
                if (width == 0)
                {
                    ImageEdited = ImageScale.ScaleByHeight((int)height);
                }
                else if (height == 0)
                {
                    ImageEdited = ImageScale.ScaleByWidth((int)width);
                }
                else
                {
                    ImageEdited = ImageScale.Scale((int)width, (int)height);
                }
            }

            if (ImageEdited != null && (bool)flip)
            {
                ImageEdited.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            if (ImageEdited == null)
            {
                return null;
            }
            return ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }


        public BitmapImage GetEditedBitmapImage()
        {
            return ImageEdited == null ? null : ImageTools.ConvertImageToBitmapImage(ImageEdited);
        }

        public Image GetEditedImage()
        {
            return ImageEdited == null ? null : ImageEdited;
        }

        public void RemoveEditedImage()
        {
            ImageEdited = null;
        }

        public void SetImageEdited(Image imageEdited)
        {
            ImageEdited = imageEdited;
        }
    }


    public class FileSize
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
