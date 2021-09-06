using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CroppingImageLibrary.Services;

namespace CroppingImageLibrary
{
    /// <summary>
    /// Interaction logic for CropToolControl.xaml
    /// </summary>
    public partial class CropToolControl : UserControl
    {
        public CropService CropService { get; private set; }
        public BitmapImage SourceBitmapImage { get; private set; }

        private bool _TextVisible = true;

        public CropToolControl()
        {
            InitializeComponent();
        }

        private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CropService.Adorner.RaiseEvent(e);
        }

        private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CropService.Adorner.RaiseEvent(e);
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            CropService = new CropService(this);
            CropService.ShowText(_TextVisible);
        }

        public void ShowText(bool IsVisible)
        {
            _TextVisible = IsVisible;
        }

        public void SetKeepRatio(bool KeepRatio)
        {
            CropService.SetKeepRatio(KeepRatio);
        }

        public void SetImage(BitmapImage bitmapImage)
        {
            SourceBitmapImage = bitmapImage;
            SourceImage.Source = bitmapImage;
        }

        public BitmapImage GetImage()
        {
            return (BitmapImage)SourceImage.Source;
        }

        public void SetCropArea(double Width, double Height)
        {
            CropService.SetCropArea(Width, Height);
        }

        public void SetCropAreaRatio(int rWidth, int rHeight)
        {
            CropService.SetCropAreaRatio(SourceImage.ActualWidth, SourceImage.ActualHeight, rWidth, rHeight);
        }

        public Bitmap GetBitmapCrop()
        {
            var cropArea = CropService.GetCroppedArea();

            int X = (int)cropArea.CroppedRectAbsolute.X;
            int Y = (int)cropArea.CroppedRectAbsolute.Y;

            int Width = (int)cropArea.CroppedRectAbsolute.Width;
            int Height = (int)cropArea.CroppedRectAbsolute.Height;

            if (Width == 0 || Height == 0)
            {
                return BitmapImageToBitmap(SourceBitmapImage);
            }

            if (SourceBitmapImage.PixelHeight != SourceImage.ActualHeight || SourceBitmapImage.PixelWidth != SourceImage.ActualWidth)
            {
                float ratioH = (float)SourceImage.ActualHeight * 100 / (float)SourceBitmapImage.PixelHeight;
                float ratioW = (float)SourceImage.ActualWidth * 100 / (float)SourceBitmapImage.PixelWidth;

                Width = (int)(Width / ratioW * 100);
                Height = (int)(Height / ratioH * 100);

                X = (int)(X / ratioW * 100);
                Y = (int)(Y / ratioH * 100);
            }

            Rectangle cropRect = new Rectangle(X, Y, Width, Height);

            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height, PixelFormat.Format32bppArgb);
            Bitmap SourceBitmap = BitmapImageToBitmap(SourceBitmapImage);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(SourceBitmap, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                    cropRect,
                    GraphicsUnit.Pixel);
            }

            return target;
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
