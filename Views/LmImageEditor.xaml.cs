using CommonPluginsPlaynite;
using CommonPluginsShared;
using CroppingImageLibrary;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Logique d'interaction pour LmImageEditor.xaml
    /// </summary>
    public partial class LmImageEditor : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private IPlayniteAPI _PlayniteApi;
        private Game _GameMenu;

        public LmImageTools LmImageToolsIcon;
        public LmImageTools LmImageToolsCover;
        public LmImageTools LmImageToolsBackground;

        private LmImageTools LmImageToolsSelected;

        private CropToolControl cropToolControl;

        public static List<FileSize> fileSizes = new List<FileSize>
        {
            new FileSize { Name = "Icon", Width = 128, Height = 128 },
            new FileSize { Name = "Icon", Width = 256, Height = 256 },
            new FileSize { Name = "Icon", Width = 512, Height = 512 },

            new FileSize { Name = "Grid - Steam", Width = 460, Height = 215 },
            new FileSize { Name = "Grid - Steam", Width = 920, Height = 430 },
            new FileSize { Name = "Grid - Steam", Width = 600, Height = 900 },
            new FileSize { Name = "Grid - Galaxy 2.0", Width = 342, Height = 482 },
            new FileSize { Name = "Grid - Galaxy 2.0", Width = 660, Height = 930 },

            new FileSize { Name = "Heroes - Steam", Width = 1920, Height = 620 },
            new FileSize { Name = "Heroes - Steam", Width = 3840, Height = 1240 },
            new FileSize { Name = "Heroes - Galaxy 2.0", Width = 1600, Height = 650 }
        };

        public static List<FileSize> cropSizes = new List<FileSize>
        {
            new FileSize { Name = resources.GetString("LOCGameIconTitle"), Width = 1, Height = 1 },

            new FileSize { Name = "Heroes - Steam", Width = 96, Height = 31 },
            new FileSize { Name = "Heroes - GOG", Width = 32, Height = 13 },

            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectDVD"), Width = 27, Height = 38 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectEpicGamesStore"), Width = 3, Height = 4 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectGogGalaxy2"), Width = 22, Height = 31 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectIgdb"), Width = 3, Height = 4 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectSteam"), Width = 92, Height = 43 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectSteamVertical"), Width = 2, Height = 3 },
            new FileSize { Name = "Grid - " + resources.GetString("LOCSettingsCovertAspectTwitch"), Width = 3, Height = 4 }
        };


        public LmImageEditor(IPlayniteAPI PlayniteApi, Game GameMenu)
        {
            _PlayniteApi = PlayniteApi;
            _GameMenu = GameMenu;


            InitializeComponent();


            List<string> ListMedia = new List<string>();

            if (!_GameMenu.Icon.IsNullOrEmpty())
            {
                LmImageToolsIcon = new LmImageTools(_PlayniteApi.Database.GetFullFilePath(_GameMenu.Icon));
                ListMedia.Add(resources.GetString("LOCGameIconTitle"));
            }
            if (!_GameMenu.CoverImage.IsNullOrEmpty())
            {
                LmImageToolsCover = new LmImageTools(_PlayniteApi.Database.GetFullFilePath(_GameMenu.CoverImage));
                ListMedia.Add(resources.GetString("LOCGameCoverImageTitle"));
            }
            if (!_GameMenu.BackgroundImage.IsNullOrEmpty())
            {
                LmImageToolsBackground = new LmImageTools(_PlayniteApi.Database.GetFullFilePath(_GameMenu.BackgroundImage));
                ListMedia.Add(resources.GetString("LOCGameBackgroundTitle"));
            }


            PART_ComboBoxMedia.ItemsSource = ListMedia;

            cropSizes.Sort((x, y) => x.Name.CompareTo(y.Name));
            PART_ComboBoxCropSize.ItemsSource = cropSizes;

            fileSizes.Sort((x, y) => x.Name.CompareTo(y.Name));
            PART_ComboBoxFileSize.ItemsSource = fileSizes;
        }


        private void PART_BtSetNewImage_Click(object sender, RoutedEventArgs e)
        {
            if (LmImageToolsSelected != null)
            {
                BitmapImage bitmapImage = null;

                // Crop
                if (!Directory.Exists(PlaynitePaths.ImagesCachePath)) 
                {
                    Directory.CreateDirectory(PlaynitePaths.ImagesCachePath);
                }

                string FileTempPath = System.IO.Path.Combine(PlaynitePaths.ImagesCachePath, "lm_temp.png");
                if (cropToolControl != null)
                {
                    var temp = cropToolControl.GetBitmapCrop();                  
                    temp.Save(FileTempPath);
                }

                // Resize
                int.TryParse(PART_SizeWishedWidth.Text, out int Width);
                int.TryParse(PART_SizeWishedHeight.Text, out int Height);

                bool Cropping = (bool)PART_CheckBoxCropping.IsChecked;

                if (!File.Exists(FileTempPath))
                {
                    bitmapImage = LmImageToolsSelected.ApplyImageOptions(Width, Height, Cropping);
                }
                else
                {
                    bitmapImage = LmImageToolsSelected.ApplyImageOptions(FileTempPath, Width, Height, Cropping);
                }


                if (bitmapImage != null)
                {
                    PART_ImageEdited.Source = bitmapImage;
                    PART_ImageEditedSize.Content = bitmapImage.PixelWidth + " x " + (int)bitmapImage.PixelHeight;
                }
            }
        }


        private void PART_ComboBoxFileSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_ComboBoxFileSize.SelectedItem != null)
            {
                PART_SizeWishedWidth.Text = ((FileSize)PART_ComboBoxFileSize.SelectedItem).Width.ToString();
                PART_SizeWishedHeight.Text = ((FileSize)PART_ComboBoxFileSize.SelectedItem).Height.ToString();
            }
            else
            {
                PART_SizeWishedWidth.Text = string.Empty;
                PART_SizeWishedHeight.Text = string.Empty;
            }
        }


        private void PART_ComboBoxMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)PART_ComboBoxMedia.SelectedItem == resources.GetString("LOCGameIconTitle"))
            {
                LmImageToolsSelected = LmImageToolsIcon;
            }
            if ((string)PART_ComboBoxMedia.SelectedItem == resources.GetString("LOCGameCoverImageTitle"))
            {
                LmImageToolsSelected = LmImageToolsCover;
            }
            if ((string)PART_ComboBoxMedia.SelectedItem == resources.GetString("LOCGameBackgroundTitle"))
            {
                LmImageToolsSelected = LmImageToolsBackground;
            }

            ImageProperty imageProperty = LmImageToolsSelected.GetOriginalImageProperty();

            PART_ImageOriginalSize.Content = imageProperty.Width + " x " + imageProperty.Height;

            PART_GridOriginalContener.Children.Clear();
            cropToolControl = new CropToolControl();
            cropToolControl.Name = "PART_ImageOriginal";
            cropToolControl.MaxWidth = PART_GridOriginalContener.ActualWidth;
            cropToolControl.MaxHeight = PART_GridOriginalContener.ActualHeight;
            
            cropToolControl.SetImage(LmImageToolsSelected.GetOriginalBitmapImage());
            PART_GridOriginalContener.Children.Add(cropToolControl);

            cropToolControl.ShowText(false);

            if (LmImageToolsSelected.GetEditedBitmapImage() != null)
            {
                PART_ImageEditedSize.Content = LmImageToolsSelected.GetEditedBitmapImage().PixelWidth + " x " + LmImageToolsSelected.GetEditedBitmapImage().PixelHeight;
                PART_ImageEdited.Source = LmImageToolsSelected.GetEditedBitmapImage();
            } 
            else
            {
                PART_ImageEditedSize.Content = string.Empty;
                PART_ImageEdited.Source = null;
            }


            // Reset crop area
            PART_CropWishedWidth.Text = string.Empty;
            PART_CropWishedHeight.Text = string.Empty;
        }


        private void PART_ComboBoxCropSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_ComboBoxCropSize.SelectedItem != null)
            {
                PART_CropWishedWidth.Text = ((FileSize)PART_ComboBoxCropSize.SelectedItem).Width.ToString();
                PART_CropWishedHeight.Text = ((FileSize)PART_ComboBoxCropSize.SelectedItem).Height.ToString();

                SetCrop();
            }
            else
            {
                PART_CropWishedWidth.Text = string.Empty;
                PART_CropWishedHeight.Text = string.Empty;
                
                if (cropToolControl != null)
                {
                    cropToolControl.SetCropArea(0, 0);
                }

                PART_CheckBoxKeppRatio.IsChecked = false;
            }
        }


        private void PART_BtSave_Click(object sender, RoutedEventArgs e)
        {
            string FilePath = string.Empty;

            if (LmImageToolsIcon != null && LmImageToolsIcon.GetEditedImage() != null && !_GameMenu.Icon.IsNullOrEmpty())
            {
                FilePath = _PlayniteApi.Database.GetFullFilePath(_GameMenu.Icon);
                LmImageToolsIcon.GetEditedImage().Save(FilePath);
            }

            if (LmImageToolsCover != null && LmImageToolsCover.GetEditedImage() != null && !_GameMenu.CoverImage.IsNullOrEmpty())
            {
                FilePath = _PlayniteApi.Database.GetFullFilePath(_GameMenu.CoverImage);
                LmImageToolsCover.GetEditedImage().Save(FilePath);
            }

            if (LmImageToolsBackground != null && LmImageToolsBackground.GetEditedImage() != null && !_GameMenu.BackgroundImage.IsNullOrEmpty())
            {
                FilePath = _PlayniteApi.Database.GetFullFilePath(_GameMenu.BackgroundImage);
                LmImageToolsBackground.GetEditedImage().Save(FilePath);
            }

            ((Window)this.Parent).Close();
        }

        private void PART_BtCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }


        private void PART_BtReset_Click(object sender, RoutedEventArgs e)
        {
            LmImageToolsSelected.RemoveEditedImage();
            PART_ImageEditedSize.Content = string.Empty;
            PART_ImageEdited.Source = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cropToolControl != null)
            {
                PART_ImageEdited.Source = ImageTools.ConvertBitmapToBitmapImage(cropToolControl.GetBitmapCrop());
            }
        }


        private void PART_CheckBoxKeppRatio_Click(object sender, RoutedEventArgs e)
        {
            if (cropToolControl != null)
            {
                cropToolControl.SetKeepRatio((bool)PART_CheckBoxKeppRatio.IsChecked);
            }
        }


        private void PART_CropWishedWidth_KeyUp(object sender, KeyEventArgs e)
        {
            SetCrop();
        }

        private void PART_CropWishedHeight_KeyUp(object sender, KeyEventArgs e)
        {
            SetCrop();
        }


        private void SetCrop()
        {
            int.TryParse(PART_CropWishedWidth.Text, out int rWidth);
            int.TryParse(PART_CropWishedHeight.Text, out int rHeight);

            if (rWidth != 0 && rHeight != 0 && LmImageToolsSelected != null)
            {
                if (cropToolControl != null)
                {
                    cropToolControl.SetCropAreaRatio(rWidth, rHeight);
                }
            }
        }
    }
}
