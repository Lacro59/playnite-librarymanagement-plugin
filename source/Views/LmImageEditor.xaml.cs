using CommonPlayniteShared;
using CommonPlayniteShared.Common;
using CommonPluginsShared;
using CroppingImageLibrary;
using LibraryManagement.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LibraryManagement.Views
{
    /// <summary>
    /// Logique d'interaction pour LmImageEditor.xaml
    /// </summary>
    public partial class LmImageEditor : UserControl
    {
        private Game GameMenu { get; set; }

        public LmImageTools LmImageToolsIcon { get; set; }
        public LmImageTools LmImageToolsCover { get; set; }
        public LmImageTools LmImageToolsBackground { get; set; }

        private LmImageTools LmImageToolsSelected { get; set; }

        private CropToolControl CropToolControl { get; set; }

        public static List<FileSize> FileSizes { get; set; } = new List<FileSize>
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

        public static List<FileSize> CropSizes { get; set; } = new List<FileSize>
        {
            new FileSize { Name = ResourceProvider.GetString("LOCGameIconTitle"), Width = 1, Height = 1 },

            new FileSize { Name = "Heroes - Steam", Width = 96, Height = 31 },
            new FileSize { Name = "Heroes - GOG", Width = 32, Height = 13 },

            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectDVD"), Width = 27, Height = 38 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectEpicGamesStore"), Width = 3, Height = 4 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectGogGalaxy2"), Width = 22, Height = 31 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectIgdb"), Width = 3, Height = 4 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectSteam"), Width = 92, Height = 43 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectSteamVertical"), Width = 2, Height = 3 },
            new FileSize { Name = "Grid - " + ResourceProvider.GetString("LOCSettingsCovertAspectTwitch"), Width = 3, Height = 4 }
        };


        public LmImageEditor(Game gameMenu)
        {
            GameMenu = gameMenu;

            InitializeComponent();

            ObservableCollection<string> ListMedia = new ObservableCollection<string>();

            if (!GameMenu.Icon.IsNullOrEmpty())
            {
                LmImageToolsIcon = new LmImageTools(API.Instance.Database.GetFullFilePath(GameMenu.Icon));
                ListMedia.Add(ResourceProvider.GetString("LOCGameIconTitle"));
            }
            if (!GameMenu.CoverImage.IsNullOrEmpty())
            {
                LmImageToolsCover = new LmImageTools(API.Instance.Database.GetFullFilePath(GameMenu.CoverImage));
                ListMedia.Add(ResourceProvider.GetString("LOCGameCoverImageTitle"));
            }
            if (!GameMenu.BackgroundImage.IsNullOrEmpty())
            {
                LmImageToolsBackground = new LmImageTools(API.Instance.Database.GetFullFilePath(GameMenu.BackgroundImage));
                ListMedia.Add(ResourceProvider.GetString("LOCGameBackgroundTitle"));
            }

            PART_ComboBoxMedia.ItemsSource = ListMedia;

            CropSizes.Sort((x, y) => x.Name.CompareTo(y.Name));
            PART_ComboBoxCropSize.ItemsSource = CropSizes;

            FileSizes.Sort((x, y) => x.Name.CompareTo(y.Name));
            PART_ComboBoxFileSize.ItemsSource = FileSizes;
        }


        private void PART_BtSetNewImage_Click(object sender, RoutedEventArgs e)
        {
            if (LmImageToolsSelected != null)
            {
                BitmapImage bitmapImage = null;

                // Crop
                FileSystem.CreateDirectory(PlaynitePaths.ImagesCachePath);

                string FileTempPath = Path.Combine(PlaynitePaths.ImagesCachePath, "lm_temp.png");
                if (CropToolControl != null)
                {
                    Bitmap temp = CropToolControl.GetBitmapCrop();
                    temp.Save(FileTempPath);
                }

                // Resize
                _ = int.TryParse(PART_SizeWishedWidth.Text, out int Width);
                _ = int.TryParse(PART_SizeWishedHeight.Text, out int Height);

                bool Cropping = (bool)PART_CheckBoxCropping.IsChecked;
                bool Flip = (bool)PART_Flip.IsChecked;

                bitmapImage = !File.Exists(FileTempPath)
                    ? LmImageToolsSelected.ApplyImageOptions(Width, Height, Cropping, Flip)
                    : LmImageToolsSelected.ApplyImageOptions(FileTempPath, Width, Height, Cropping, Flip);


                if (bitmapImage != null)
                {
                    PART_ImageEdited.Source = bitmapImage;
                    PART_ImageEditedSize.Content = bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight;

                    PART_BtSetIcon.IsEnabled = (string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameIconTitle")
                        ? false
                        : !((string)PART_ComboBoxMedia.SelectedItem).IsNullOrEmpty();
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
            if ((string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameIconTitle"))
            {
                LmImageToolsSelected = LmImageToolsIcon;
            }
            if ((string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameCoverImageTitle"))
            {
                LmImageToolsSelected = LmImageToolsCover;
            }
            if ((string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameBackgroundTitle"))
            {
                LmImageToolsSelected = LmImageToolsBackground;
            }

            PART_ComboBoxCropSize.SelectedIndex = -1;

            ImageProperty imageProperty = LmImageToolsSelected.GetOriginalImageProperty();
            if (imageProperty != null)
            {
                PART_ImageOriginalSize.Content = imageProperty.Width + " x " + imageProperty.Height;

                PART_GridOriginalContener.Children.Clear();
                CropToolControl = new CropToolControl
                {
                    Name = "PART_ImageOriginal",
                    MaxWidth = PART_GridOriginalContener.ActualWidth,
                    MaxHeight = PART_GridOriginalContener.ActualHeight
                };

                CropToolControl.SetImage(LmImageToolsSelected.GetOriginalBitmapImage());
                _ = PART_GridOriginalContener.Children.Add(CropToolControl);

                CropToolControl.ShowText(false);
            }

            if (LmImageToolsSelected.GetEditedBitmapImage() != null)
            {
                PART_ImageEditedSize.Content = LmImageToolsSelected.GetEditedBitmapImage().PixelWidth + " x " + LmImageToolsSelected.GetEditedBitmapImage().PixelHeight;
                PART_ImageEdited.Source = LmImageToolsSelected.GetEditedBitmapImage();

                PART_BtSetIcon.IsEnabled = (string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameIconTitle")
                    ? false
                    : !((string)PART_ComboBoxMedia.SelectedItem).IsNullOrEmpty();
            }
            else
            {
                PART_ImageEditedSize.Content = string.Empty;
                PART_ImageEdited.Source = null;

                PART_BtSetIcon.IsEnabled = false;
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
                
                if (CropToolControl != null)
                {
                    CropToolControl.SetCropArea(0, 0);
                }

                PART_CheckBoxKeppRatio.IsChecked = false;
            }
        }


        private void PART_BtSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string FilePath = string.Empty;
                string FilePathOld = string.Empty;
                string PluginCachePath = Path.Combine(PlaynitePaths.DataCachePath, "LibraryManagement");

                FileSystem.CreateDirectory(PluginCachePath);

                API.Instance.Database.Games.BeginBufferUpdate();

                if (LmImageToolsIcon != null && LmImageToolsIcon.GetEditedImage() != null)
                {
                    if (GameMenu.Icon.IsNullOrEmpty())
                    {
                        GameMenu.Icon = Path.Combine(GameMenu.Id.ToString(), Guid.NewGuid() + ".png");
                    }
                    string NewIcon = Path.Combine(GameMenu.Id.ToString(), Guid.NewGuid() + (Path.GetExtension(GameMenu.Icon).Contains("ico", StringComparison.OrdinalIgnoreCase) ? ".png" : Path.GetExtension(GameMenu.Icon)));

                    FilePath = Path.Combine(PluginCachePath, Path.GetFileName(GameMenu.Icon));
                    FilePathOld = API.Instance.Database.GetFullFilePath(GameMenu.Icon);
                    LmImageToolsIcon.GetEditedImage().Save(FilePath);

                    FileSystem.CopyFile(FilePath, API.Instance.Database.GetFullFilePath(NewIcon));
                    FileSystem.DeleteFileSafe(FilePathOld);

                    GameMenu.Icon = NewIcon;
                }

                if (LmImageToolsCover != null && LmImageToolsCover.GetEditedImage() != null && !GameMenu.CoverImage.IsNullOrEmpty())
                {
                    string NewIcon = Path.Combine(GameMenu.Id.ToString(), Guid.NewGuid() +  Path.GetExtension(GameMenu.CoverImage));

                    FilePath = Path.Combine(PluginCachePath, Path.GetFileName(GameMenu.CoverImage));
                    FilePathOld = API.Instance.Database.GetFullFilePath(GameMenu.CoverImage);
                    LmImageToolsCover.GetEditedImage().Save(FilePath);

                    FileSystem.CopyFile(FilePath, API.Instance.Database.GetFullFilePath(NewIcon));
                    FileSystem.DeleteFileSafe(FilePathOld);

                    GameMenu.CoverImage = NewIcon;
                }

                if (LmImageToolsBackground != null && LmImageToolsBackground.GetEditedImage() != null && !GameMenu.BackgroundImage.IsNullOrEmpty())
                {
                    string NewIcon = Path.Combine(GameMenu.Id.ToString(), Guid.NewGuid() + Path.GetExtension(GameMenu.BackgroundImage));

                    FilePath = Path.Combine(PluginCachePath, Path.GetFileName(GameMenu.BackgroundImage));
                    FilePathOld = API.Instance.Database.GetFullFilePath(GameMenu.BackgroundImage);
                    LmImageToolsBackground.GetEditedImage().Save(FilePath);

                    FileSystem.CopyFile(FilePath, API.Instance.Database.GetFullFilePath(NewIcon));
                    FileSystem.DeleteFileSafe(FilePathOld);

                    GameMenu.BackgroundImage = NewIcon;
                }

                API.Instance.Database.Games.Update(GameMenu);
                API.Instance.Database.Games.EndBufferUpdate();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "LibraryManagement");
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

            PART_BtSetIcon.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CropToolControl != null)
            {
                PART_ImageEdited.Source = ImageTools.ConvertBitmapToBitmapImage(CropToolControl.GetBitmapCrop());

                PART_BtSetIcon.IsEnabled = (string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameIconTitle")
                    ? false
                    : !((string)PART_ComboBoxMedia.SelectedItem).IsNullOrEmpty();
            }
        }


        private void PART_CheckBoxKeppRatio_Click(object sender, RoutedEventArgs e)
        {
            if (CropToolControl != null)
            {
                CropToolControl.SetKeepRatio((bool)PART_CheckBoxKeppRatio.IsChecked);
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
            _ = int.TryParse(PART_CropWishedWidth.Text, out int rWidth);
            _ = int.TryParse(PART_CropWishedHeight.Text, out int rHeight);

            if (rWidth != 0 && rHeight != 0 && LmImageToolsSelected != null)
            {
                if (CropToolControl != null)
                {
                    CropToolControl.SetCropAreaRatio(rWidth, rHeight);
                }
            }
        }
        

        private void PART_BtSetIcon_Click(object sender, RoutedEventArgs e)
        {
            if (LmImageToolsIcon == null)
            {
                if ((string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameCoverImageTitle"))
                {
                    LmImageToolsIcon = new LmImageTools(API.Instance.Database.GetFullFilePath(GameMenu.CoverImage));
                }
                if ((string)PART_ComboBoxMedia.SelectedItem == ResourceProvider.GetString("LOCGameBackgroundTitle"))
                {
                    LmImageToolsIcon = new LmImageTools(API.Instance.Database.GetFullFilePath(GameMenu.BackgroundImage));
                }

                ((ObservableCollection<string>)PART_ComboBoxMedia.ItemsSource).Add(ResourceProvider.GetString("LOCGameIconTitle"));
            }

            LmImageToolsIcon.SetImageEdited(LmImageToolsSelected.GetEditedImage());
            PART_BtReset_Click(null, null);
        }
    }
}
