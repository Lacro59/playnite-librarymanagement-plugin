using CommonPluginsControls.Views;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace LibraryManagement.Services
{
    public class LibraryManagementTools
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement Plugin;
        private LibraryManagementSettings PluginSettings;
        private IPlayniteAPI PlayniteApi;


        public LibraryManagementTools(LibraryManagement Plugin, IPlayniteAPI PlayniteApi, LibraryManagementSettings PluginSettings)
        {
            this.Plugin = Plugin;
            this.PluginSettings = PluginSettings;
            this.PlayniteApi = PlayniteApi;
        }


        #region Genres
        public void SetGenres(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListGenreEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetGenres: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckGenre();
                UpdateGenre(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckGenre();

                    // Remplace genres
                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateGenre(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateGenre",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCGameGenresTitle")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmGenresUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    } 


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetGenres(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }

        private void CheckGenre()
        {
            // Add new genres that not exist
            List<Genre> PlayniteGenres = PlayniteApi.Database.Genres.ToList();
            foreach (LmGenreEquivalences lmGenreEquivalences in PluginSettings.ListGenreEquivalences)
            {
                if (lmGenreEquivalences.Id == null)
                {
                    Genre genre = new Genre(lmGenreEquivalences.NewName);
                    lmGenreEquivalences.Id = genre.Id;

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Genres.Add(genre);
                        Plugin.SavePluginSettings(PluginSettings);
                    }).Wait();
                }
            }
        }

        private bool UpdateGenre(Game game)
        {
            bool IsUpdated = false;
            List<Genre> GameGenres = game.Genres;

            if (GameGenres != null && GameGenres.Count > 0)
            {
                // Rename
                List<Genre> AllGenresOld = GameGenres.FindAll(x => PluginSettings.ListGenreEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                if (AllGenresOld.Count > 0)
                {
                    // Remove all
                    foreach (Genre genre in AllGenresOld)
                    {
                        game.GenreIds.Remove(genre.Id);
                        IsUpdated = true;
                    }

                    // Set all
                    foreach (LmGenreEquivalences item in PluginSettings.ListGenreEquivalences.FindAll(x => x.OldNames.Any(y => AllGenresOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            game.GenreIds.AddMissing((Guid)item.Id);
                            IsUpdated = true;
                        }
                    }
                }

                // Exclusion
                if (PluginSettings.ListGenreExclusion.Count > 0)
                {
                    foreach (string GenreName in PluginSettings.ListGenreExclusion)
                    {
                        Genre genreDelete = game.Genres.Find(x => x.Name.ToLower() == GenreName.ToLower());
                        if (genreDelete != null)
                        {
                            game.GenreIds.Remove(genreDelete.Id);
                            IsUpdated = true;
                        }
                    }
                }
            }

            if (IsUpdated)
            {
                Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                {
                    PlayniteApi.Database.Games.Update(game);
                }).Wait();
            }

            return IsUpdated;
        }

        public static void RenameGenre(IPlayniteAPI PlayniteApi, Guid Id, string NewName)
        {
            Genre genre = PlayniteApi.Database.Genres.Get(Id);
            if (genre != null)
            {
                genre.Name = NewName;
                PlayniteApi.Database.Genres.Update(genre);
            }
            else
            {
                logger.Warn($"Genre doesn't exist - {Id}");
            }
        }
        #endregion


        #region Features
        public void SetFeatures(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListFeatureEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetFeatures: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckFeature();
                UpdateFeature(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckFeature();

                    // Remplace features
                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb.ToList())
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateFeature(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateFeature",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCFeaturesLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmFeaturesUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetFeatures(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }

        private void CheckFeature()
        {
            // Add new feature that not exist
            List<GameFeature> PlayniteFeatures = PlayniteApi.Database.Features.ToList();
            foreach (LmFeatureEquivalences lmFeatureEquivalences in PluginSettings.ListFeatureEquivalences)
            {
                if (lmFeatureEquivalences.Id == null)
                {
                    GameFeature feature = new GameFeature(lmFeatureEquivalences.NewName);
                    lmFeatureEquivalences.Id = feature.Id;

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Features.Add(feature);
                        Plugin.SavePluginSettings(PluginSettings);
                    }).Wait();
                }
            }
        }

        private bool UpdateFeature(Game game)
        {
            bool IsUpdated = false;
            List<GameFeature> gameFeatures = game.Features;

            if (gameFeatures != null && gameFeatures.Count > 0)
            {
                // Rename
                List<GameFeature> AllFeaturesOld = gameFeatures.FindAll(x => PluginSettings.ListFeatureEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                if (AllFeaturesOld.Count > 0)
                {
                    // Remove all
                    foreach (GameFeature feature in AllFeaturesOld)
                    {
                        game.FeatureIds.Remove(feature.Id);
                        IsUpdated = true;
                    }

                    // Set all
                    foreach (LmFeatureEquivalences item in PluginSettings.ListFeatureEquivalences.FindAll(x => x.OldNames.Any(y => AllFeaturesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            game.FeatureIds.AddMissing((Guid)item.Id);
                            IsUpdated = true;
                        }
                    }

                    if (IsUpdated)
                    {
                        Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                        {
                            PlayniteApi.Database.Games.Update(game);
                        }).Wait();
                    }
                }

                // Exclusion
                if (PluginSettings.ListFeatureExclusion.Count > 0)
                {
                    foreach (string FeatureName in PluginSettings.ListFeatureExclusion)
                    {
                        GameFeature featureDelete = game.Features.Find(x => x.Name.ToLower() == FeatureName.ToLower());
                        if (featureDelete != null)
                        {
                            game.FeatureIds.Remove(featureDelete.Id);
                            IsUpdated = true;
                        }
                    }

                    if (IsUpdated)
                    {
                        Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                        {
                            PlayniteApi.Database.Games.Update(game);
                        }).Wait();
                    }
                }
            }

            return IsUpdated;
        }

        public static void RenameFeature(IPlayniteAPI PlayniteApi, Guid Id, string NewName)
        {
            GameFeature gameFeature = PlayniteApi.Database.Features.Get(Id);
            if (gameFeature != null)
            {
                gameFeature.Name = NewName;
                PlayniteApi.Database.Features.Update(gameFeature);
            }
            else
            {
                logger.Warn($"Feature doesn't exist - {Id}");
            }
        }
        #endregion


        #region Tags
        public void SetTags(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListTagsEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetTags: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckTags();
                UpdateTags(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckTags();

                    // Remplace tags
                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateTags(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTag",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCTagsLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmTagsUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }

        private void CheckTags()
        {
            // Add new tags that not exist
            List<Tag> PlayniteTags = PlayniteApi.Database.Tags.ToList();
            foreach (LmTagsEquivalences lmTagsEquivalences in PluginSettings.ListTagsEquivalences)
            {
                if (lmTagsEquivalences.Id == null)
                {
                    Tag tag = new Tag(lmTagsEquivalences.NewName);
                    lmTagsEquivalences.Id = tag.Id;

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Tags.Add(tag);
                        Plugin.SavePluginSettings(PluginSettings);
                    }).Wait();
                }
            }
        }

        private bool UpdateTags(Game game)
        {
            bool IsUpdated = false;
            List<Tag> Tags = game?.Tags;

            if (Tags != null && Tags.Count > 0)
            {
                // Rename
                List<Tag> AllTagsOld = Tags.FindAll(x => PluginSettings.ListTagsEquivalences.Any(y => y.OldNames.Any(z => z.IsEqual(x.Name))));

                if (AllTagsOld?.Count > 0)
                {
                    // Remove all
                    foreach (Tag tag in AllTagsOld)
                    {
                        game.TagIds.Remove(tag.Id);
                        IsUpdated = true;
                    }

                    // Set all
                    foreach (LmTagsEquivalences item in PluginSettings.ListTagsEquivalences.FindAll(x => x.OldNames.Any(y => AllTagsOld.Any(z => z.Name.IsEqual(y)))))
                    {
                        if (item.Id != null)
                        {
                            game.TagIds.AddMissing((Guid)item.Id);
                            IsUpdated = true;
                        }
                    }
                }

                // Exclusion
                if (PluginSettings.ListTagsExclusion.Count > 0)
                {
                    foreach (string TagName in PluginSettings.ListTagsExclusion)
                    {
                        Tag TagDelete = game?.Tags?.Find(x => x.Name.IsEqual(TagName));
                        if (TagDelete != null)
                        {
                            game.TagIds.Remove(TagDelete.Id);
                            IsUpdated = true;
                        }
                    }
                }
            }

            if (IsUpdated)
            {
                Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                {
                    PlayniteApi.Database.Games.Update(game);
                }).Wait();
            }

            return IsUpdated;
        }

        public static void RenameTags(IPlayniteAPI PlayniteApi, Guid Id, string NewName)
        {
            Tag tag = PlayniteApi.Database.Tags.Get(Id);
            if (tag != null)
            {
                tag.Name = NewName;
                PlayniteApi.Database.Tags.Update(tag);
            }
            else
            {
                logger.Warn($"Tag doesn't exist - {Id}");
            }
        }
        #endregion


        #region Companies
        public void SetCompanies(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListCompaniesEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                    $"LibraryManagement-{new Guid()}",
                    $"LibraryManagement" + System.Environment.NewLine + "SetCompanies: " + resources.GetString("LOCLmNoEquivalence"),
                    NotificationType.Error
                ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckCompanies();
                UpdateCompanies(gameUpdated);
                UpdateCompanies(gameUpdated, true);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckCompanies();

                    // Remplace Companies
                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);
                        try
                        {
                            if (UpdateCompanies(game))
                            {
                                gamesUpdated.Add(game);
                            }
                            if (UpdateCompanies(game, true))
                            {
                                gamesUpdated.AddMissing(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateCompany",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCCompaniesLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmCompaniesUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetCompanies(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }

        private void CheckCompanies()
        {
            // Add new Companies that not exist
            List<Company> PlayniteCompanies = PlayniteApi.Database.Companies.ToList();
            foreach (LmCompaniesEquivalences lmCompaniesEquivalences in PluginSettings.ListCompaniesEquivalences)
            {
                if (lmCompaniesEquivalences.Id == null)
                {
                    Company company = new Company(lmCompaniesEquivalences.NewName);
                    lmCompaniesEquivalences.Id = company.Id;

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Companies.Add(company);
                        Plugin.SavePluginSettings(PluginSettings);
                    }).Wait();
                }
            }
        }

        private bool UpdateCompanies(Game game, bool IsPublishers = false)
        {
            bool IsUpdated = false;
            List<Company> Companies = new List<Company>();
            if (IsPublishers)
            {
                Companies = game.Publishers;
            }
            else
            {
                Companies = game.Developers;
            }

            if (Companies != null && Companies.Count > 0)
            {
                // Rename
                List<Company> AllCompaniesOld = Companies.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                if (AllCompaniesOld.Count > 0)
                {
                    // Remove all
                    foreach (Company company in AllCompaniesOld)
                    {
                        if (IsPublishers)
                        {
                            game.PublisherIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                        else
                        {
                            game.DeveloperIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                    }

                    // Set all
                    foreach (LmCompaniesEquivalences item in PluginSettings.ListCompaniesEquivalences.FindAll(x => x.OldNames.Any(y => AllCompaniesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            if (IsPublishers)
                            {
                                game.PublisherIds.AddMissing((Guid)item.Id);
                                IsUpdated = true;
                            }
                            else
                            {
                                game.DeveloperIds.AddMissing((Guid)item.Id);
                                IsUpdated = true;
                            }
                        }
                    }
                }

                // Exclusion
                if (PluginSettings.ListCompaniesExclusion.Count > 0)
                {
                    foreach (string CompanyName in PluginSettings.ListCompaniesExclusion)
                    {
                        if (IsPublishers)
                        {
                            Company CompanyDelete = game.Publishers.Find(x => x.Name.ToLower() == CompanyName.ToLower());
                            if (CompanyDelete != null)
                            {
                                game.PublisherIds.Remove(CompanyDelete.Id);
                                IsUpdated = true;
                            }
                        }
                        else
                        {
                            Company CompanyDelete = game.Developers.Find(x => x.Name.ToLower() == CompanyName.ToLower());
                            if (CompanyDelete != null)
                            {
                                game.DeveloperIds.Remove(CompanyDelete.Id);
                                IsUpdated = true;
                            }
                        }
                    }
                }
            }

            if (IsUpdated)
            {
                Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                {
                    PlayniteApi.Database.Games.Update(game);
                }).Wait();
            }

            return IsUpdated;
        }

        public static void RenameCompanies(IPlayniteAPI PlayniteApi, Guid Id, string NewName)
        {
            Company company = PlayniteApi.Database.Companies.Get(Id);
            if (company != null)
            {
                company.Name = NewName;
                PlayniteApi.Database.Companies.Update(company);
            }
            else
            {
                logger.Warn($"Company doesn't exist - {Id}");
            }
        }
        #endregion


        #region TagsToFeatures
        public void SetTagsToFeatures(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListTagsToFeatures?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetTagsToFeatures: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateTagsToFeatures(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToFeatures",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCLmTagsToFeatures")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmTagsToFeaturesUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }


        private bool UpdateTagsToFeatures(Game game)
        {
            bool IsUpdated = false;
            List<Tag> Tags = Serialization.GetClone(game?.Tags);

            game?.Tags?.ForEach(y =>
            {
                var finded = PluginSettings.ListTagsToFeatures.Where(x => x.TagId == y.Id).FirstOrDefault();
                if (finded != null)
                {
                    IsUpdated = true;
                    game.TagIds.Remove(y.Id);
                    if (game.FeatureIds != null)
                    {
                        game.FeatureIds.AddMissing(finded.FeatureId);
                    }
                    else
                    {
                        game.FeatureIds = new List<Guid> { finded.FeatureId };
                    }
                }
            });

            if (IsUpdated)
            {
                Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                {
                    PlayniteApi.Database.Games.Update(game);
                }).Wait();
            }

            return IsUpdated;
        }
        #endregion


        #region TagsToGenres
        public void SetTagsToGenres(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListTagsToGenres?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetTagsToGenres: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateTagsToGenres(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToGenres",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCLmTagsToGenres")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmTagsToGenresUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }


        private bool UpdateTagsToGenres(Game game)
        {
            bool IsUpdated = false;
            List<Tag> Tags = Serialization.GetClone(game?.Tags);

            game?.Tags?.ForEach(y =>
            {
                var finded = PluginSettings.ListTagsToGenres.Where(x => x.TagId == y.Id).FirstOrDefault();
                if (finded != null)
                {
                    IsUpdated = true;
                    game.TagIds.Remove(y.Id);
                    if (game.GenreIds != null)
                    {
                        game.GenreIds.AddMissing(finded.GenreId);
                    }
                    else
                    {
                        game.GenreIds = new List<Guid> { finded.GenreId };
                    }
                }
            });

            return IsUpdated;
        }
        #endregion


        #region TagsToCategories
        public void SetTagsToCategories(bool OnlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListTagsToGenres?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + System.Environment.NewLine + "SetTagsToCategories: " + resources.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    IEnumerable<Game> PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null && x.Added > PluginSettings.LastAutoLibUpdateAssetsDownload);
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        try
                        {
                            Thread.Sleep(10);
                            if (UpdateTagsToCategories(game))
                            {
                                gamesUpdated.Add(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToGenres",
                             $"LibraryManagement" + System.Environment.NewLine + string.Format(resources.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, resources.GetString("LOCLmTagsToGenres")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCLmTagsToGenresUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, "LibraryManagement");
                }
            }, globalProgressOptions);
        }


        private bool UpdateTagsToCategories(Game game)
        {
            bool IsUpdated = false;
            List<Tag> Tags = Serialization.GetClone(game?.Tags);

            game?.Tags?.ForEach(y =>
            {
                var finded = PluginSettings.ListTagsToCategories.Where(x => x.TagId == y.Id).FirstOrDefault();
                if (finded != null)
                {
                    IsUpdated = true;
                    game.TagIds.Remove(y.Id);
                    if (game.CategoryIds != null)
                    {
                        game.CategoryIds.AddMissing(finded.CategoryId);
                    }
                    else
                    {
                        game.CategoryIds = new List<Guid> { finded.CategoryId };
                    }
                }
            });

            return IsUpdated;
        }
        #endregion
    }
}
