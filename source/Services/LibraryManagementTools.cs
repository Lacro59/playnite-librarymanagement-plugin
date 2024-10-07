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
using System.Threading;
using System.Windows;

namespace LibraryManagement.Services
{
    public class LibraryManagementTools
    {
        private static ILogger Logger => LogManager.GetLogger();

        private LibraryManagement Plugin { get; set; }
        private LibraryManagementSettings PluginSettings { get; set; }


        public LibraryManagementTools(LibraryManagement plugin, LibraryManagementSettings pluginSettings)
        {
            Plugin = plugin;
            PluginSettings = pluginSettings;
        }


        #region Genres
        public void SetGenres(bool onlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListGenreEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetGenres: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckGenre();
                _ = UpdateGenre(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckGenre();
                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateGenre",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCGameGenresTitle")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmGenresUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetGenres(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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
            List<Genre> PlayniteGenres = API.Instance.Database.Genres.ToList();
            foreach (LmGenreEquivalences lmGenreEquivalences in PluginSettings.ListGenreEquivalences)
            {
                Genre findedById = PlayniteGenres.Find(x => x.Id == lmGenreEquivalences.Id);
                if (lmGenreEquivalences.Id == null || findedById == null)
                {
                    Genre findedByName = PlayniteGenres.Find(x => x.Name.IsEqual(lmGenreEquivalences.NewName));
                    if (findedByName != null)
                    {
                        lmGenreEquivalences.Id = findedByName.Id;
                    }
                    else
                    {
                        Genre genre = new Genre(lmGenreEquivalences.NewName);
                        lmGenreEquivalences.Id = genre.Id;
                    }
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() => Plugin.SavePluginSettings(PluginSettings));
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
                        _ = game.GenreIds.Remove(genre.Id);
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


                // Remove same
                List<Genre> AllGenresSame = GameGenres.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.NewName.IsEqual(x.Name) && x.Id != y.Id));

                if (AllGenresSame.Count > 0)
                {
                    // Remove all
                    foreach (Genre genre in AllGenresSame)
                    {
                        _ = game.GenreIds.Remove(genre.Id);
                        IsUpdated = true;
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
                            _ = game.GenreIds.Remove(genreDelete.Id);
                            IsUpdated = true;
                        }
                    }
                }
            }

            if (IsUpdated)
            {
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }

        public static void RenameGenre(Guid id, string newName)
        {
            Genre genre = API.Instance.Database.Genres.Get(id);
            if (genre != null)
            {
                genre.Name = newName;
                API.Instance.Database.Genres.Update(genre);
            }
            else
            {
                Logger.Warn($"Genre doesn't exist - {id}");
            }
        }
        #endregion


        #region Features
        public void SetFeatures(bool onlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListFeatureEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetFeatures: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckFeature();
                _ = UpdateFeature(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckFeature();

                    // Replace features
                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb.ToList())
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateFeature",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCFeaturesLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmFeaturesUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetFeatures(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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
            List<GameFeature> PlayniteFeatures = API.Instance.Database.Features.ToList();
            foreach (LmFeatureEquivalences lmFeatureEquivalences in PluginSettings.ListFeatureEquivalences)
            {
                GameFeature findedById = PlayniteFeatures.Find(x => x.Id == lmFeatureEquivalences.Id);
                if (lmFeatureEquivalences.Id == null || findedById == null)
                {
                    GameFeature findedByName = PlayniteFeatures.Find(x => x.Name.IsEqual(lmFeatureEquivalences.NewName));
                    if (findedByName != null)
                    {
                        lmFeatureEquivalences.Id = findedByName.Id;
                    }
                    else
                    {
                        GameFeature feature = new GameFeature(lmFeatureEquivalences.NewName);
                        lmFeatureEquivalences.Id = feature.Id;
                    }
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() => Plugin.SavePluginSettings(PluginSettings));
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
                        _ = game.FeatureIds.Remove(feature.Id);
                        IsUpdated = true;
                    }

                    // Set all
                    foreach (LmFeatureEquivalences item in PluginSettings.ListFeatureEquivalences.FindAll(x => x.OldNames.Any(y => AllFeaturesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            _ = game.FeatureIds.AddMissing((Guid)item.Id);
                            IsUpdated = true;
                        }
                    }

                    if (IsUpdated)
                    {
                        API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
                    }
                }


                // Remove same
                List<GameFeature> AllFeaturesSame = gameFeatures.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.NewName.IsEqual(x.Name) && x.Id != y.Id));

                if (AllFeaturesSame.Count > 0)
                {
                    // Remove all
                    foreach (GameFeature feature in AllFeaturesSame)
                    {
                        _ = game.FeatureIds.Remove(feature.Id);
                        IsUpdated = true;
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
                            _ = game.FeatureIds.Remove(featureDelete.Id);
                            IsUpdated = true;
                        }
                    }

                    if (IsUpdated)
                    {
                        API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
                    }
                }
            }

            return IsUpdated;
        }

        public static void RenameFeature(Guid id, string newName)
        {
            GameFeature gameFeature = API.Instance.Database.Features.Get(id);
            if (gameFeature != null)
            {
                gameFeature.Name = newName;
                API.Instance.Database.Features.Update(gameFeature);
            }
            else
            {
                Logger.Warn($"Feature doesn't exist - {id}");
            }
        }
        #endregion


        #region Tags
        public void SetTags(bool onlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListTagsEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetTags: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckTags();
                _ = UpdateTags(gameUpdated);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckTags();

                    // Replace tags
                    var PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTag",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCTagsLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsUpdated"), listDataUpdated);
                                 windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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
            List<Tag> PlayniteTags = API.Instance.Database.Tags.ToList();
            foreach (LmTagsEquivalences lmTagsEquivalences in PluginSettings.ListTagsEquivalences)
            {
                var findedById = PlayniteTags.Find(x => x.Id == lmTagsEquivalences.Id);
                if (lmTagsEquivalences.Id == null || findedById == null)
                {
                    var findedByName = PlayniteTags.Find(x => x.Name.IsEqual(lmTagsEquivalences.NewName));
                    if (findedByName != null)
                    {
                        lmTagsEquivalences.Id = findedByName.Id;
                    }
                    else
                    {
                        Tag tag = new Tag(lmTagsEquivalences.NewName);
                        lmTagsEquivalences.Id = tag.Id;
                    }
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() => Plugin.SavePluginSettings(PluginSettings));
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


                // Remove same
                List<Tag> AllTagsSame = Tags.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.NewName.IsEqual(x.Name) && x.Id != y.Id));

                if (AllTagsSame.Count > 0)
                {
                    // Remove all
                    foreach (Tag tag in AllTagsSame)
                    {
                        game.TagIds.Remove(tag.Id);
                        IsUpdated = true;
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
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }

        public static void RenameTags(Guid id, string newName)
        {
            Tag tag = API.Instance.Database.Tags.Get(id);
            if (tag != null)
            {
                tag.Name = newName;
                API.Instance.Database.Tags.Update(tag);
            }
            else
            {
                Logger.Warn($"Tag doesn't exist - {id}");
            }
        }
        #endregion


        #region Companies
        public void SetCompanies(bool onlyToDay = false, Game gameUpdated = null)
        {
            if (PluginSettings.ListCompaniesEquivalences?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                    $"LibraryManagement-{new Guid()}",
                    $"LibraryManagement" + Environment.NewLine + "SetCompanies: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                    NotificationType.Error
                ));
                return;
            }


            if (gameUpdated != null)
            {
                CheckCompanies();
                _ = UpdateCompanies(gameUpdated);
                _ = UpdateCompanies(gameUpdated, true);
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    CheckCompanies();

                    // Replace Companies
                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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
                                _ = gamesUpdated.AddMissing(game);
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "LibraryManagement");
                        }

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateCompany",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCCompaniesLabel")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmCompaniesUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetCompanies(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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
            List<Company> PlayniteCompanies = API.Instance.Database.Companies.ToList();
            foreach (LmCompaniesEquivalences lmCompaniesEquivalences in PluginSettings.ListCompaniesEquivalences)
            {
                Company findedById = PlayniteCompanies.Find(x => x.Id == lmCompaniesEquivalences.Id);
                if (lmCompaniesEquivalences.Id == null || findedById == null)
                {
                    Company findedByName = PlayniteCompanies.Find(x => x.Name.IsEqual(lmCompaniesEquivalences.NewName));
                    if (findedByName != null)
                    {
                        lmCompaniesEquivalences.Id = findedByName.Id;
                    }
                    else
                    {
                        Company company = new Company(lmCompaniesEquivalences.NewName);
                        lmCompaniesEquivalences.Id = company.Id;
                    }
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() => Plugin.SavePluginSettings(PluginSettings));
        }

        private bool UpdateCompanies(Game game, bool isPublishers = false)
        {
            bool IsUpdated = false;
            List<Company> Companies = new List<Company>();
            Companies = isPublishers ? game.Publishers : game.Developers;

            if (Companies != null && Companies.Count > 0)
            {
                // Rename
                List<Company> AllCompaniesOld = Companies.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.OldNames.Any(z => z.IsEqual(x.Name))));

                if (AllCompaniesOld.Count > 0)
                {
                    // Remove all
                    foreach (Company company in AllCompaniesOld)
                    {
                        if (isPublishers)
                        {
                            _ = game.PublisherIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                        else
                        {
                            _ = game.DeveloperIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                    }

                    // Set all
                    foreach (LmCompaniesEquivalences item in PluginSettings.ListCompaniesEquivalences.FindAll(x => x.OldNames.Any(y => AllCompaniesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            if (isPublishers)
                            {
                                _ = game.PublisherIds.AddMissing((Guid)item.Id);
                                IsUpdated = true;
                            }
                            else
                            {
                                _ = game.DeveloperIds.AddMissing((Guid)item.Id);
                                IsUpdated = true;
                            }
                        }
                    }
                }


                // Remove same
                List<Company> AllCompaniesSame = Companies.FindAll(x => PluginSettings.ListCompaniesEquivalences.Any(y => y.NewName.IsEqual(x.Name) && x.Id != y.Id));

                if (AllCompaniesSame.Count > 0)
                {
                    // Remove all
                    foreach (Company company in AllCompaniesSame)
                    {
                        if (isPublishers)
                        {
                            _ = game.PublisherIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                        else
                        {
                            _ = game.DeveloperIds.Remove(company.Id);
                            IsUpdated = true;
                        }
                    }
                }


                // Exclusion
                if (PluginSettings.ListCompaniesExclusion.Count > 0)
                {
                    foreach (string CompanyName in PluginSettings.ListCompaniesExclusion)
                    {
                        if (isPublishers)
                        {
                            Company CompanyDelete = game.Publishers.Find(x => x.Name.ToLower() == CompanyName.ToLower());
                            if (CompanyDelete != null)
                            {
                                _ = game.PublisherIds.Remove(CompanyDelete.Id);
                                IsUpdated = true;
                            }
                        }
                        else
                        {
                            Company CompanyDelete = game.Developers.Find(x => x.Name.ToLower() == CompanyName.ToLower());
                            if (CompanyDelete != null)
                            {
                                _ = game.DeveloperIds.Remove(CompanyDelete.Id);
                                IsUpdated = true;
                            }
                        }
                    }
                }
            }

            if (IsUpdated)
            {
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }

        public static void RenameCompanies(Guid id, string newName)
        {
            Company company = API.Instance.Database.Companies.Get(id);
            if (company != null)
            {
                company.Name = newName;
                API.Instance.Database.Companies.Update(company);
            }
            else
            {
                Logger.Warn($"Company doesn't exist - {id}");
            }
        }
        #endregion


        #region TagsToFeatures
        public void SetTagsToFeatures(bool onlyToDay = false)
        {
            if (PluginSettings.ListTagsToFeatures?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetTagsToFeatures: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToFeatures",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCLmTagsToFeatures")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToFeaturesUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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

            game?.Tags?.ForEach(y =>
            {
                LmTagToFeature finded = PluginSettings.ListTagsToFeatures.FirstOrDefault(x => x.TagId == y.Id);
                if (finded != null)
                {
                    IsUpdated = true;
                    _ = game.TagIds.Remove(y.Id);
                    if (game.FeatureIds != null)
                    {
                        _ = game.FeatureIds.AddMissing(finded.FeatureId);
                    }
                    else
                    {
                        game.FeatureIds = new List<Guid> { finded.FeatureId };
                    }
                }
            });

            if (IsUpdated)
            {
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }
        #endregion


        #region TagsToGenres
        public void SetTagsToGenres(bool onlyToDay = false)
        {
            if (PluginSettings.ListTagsToGenres?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetTagsToGenres: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToGenres",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCLmTagsToGenres")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToGenresUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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

            game?.Tags?.ForEach(y =>
            {
                LmTagToGenre finded = PluginSettings.ListTagsToGenres.FirstOrDefault(x => x.TagId == y.Id);
                if (finded != null)
                {
                    IsUpdated = true;
                    _ = game.TagIds.Remove(y.Id);
                    if (game.GenreIds != null)
                    {
                        _ = game.GenreIds.AddMissing(finded.GenreId);
                    }
                    else
                    {
                        game.GenreIds = new List<Guid> { finded.GenreId };
                    }
                }
                else
                {

                }
            });

            if (IsUpdated)
            {
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }
        #endregion


        #region TagsToCategories
        public void SetTagsToCategories(bool onlyToDay = false)
        {
            if (PluginSettings.ListTagsToGenres?.Count == 0)
            {
                API.Instance.Notifications.Add(new NotificationMessage(
                     $"LibraryManagement-{new Guid()}",
                     $"LibraryManagement" + Environment.NewLine + "SetTagsToCategories: " + ResourceProvider.GetString("LOCLmNoEquivalence"),
                     NotificationType.Error
                 ));
                return;
            }


            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}")
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    ICollection<Game> PlayniteDb = GetGamesToUpdate(onlyToDay);

                    a.ProgressMaxValue = PlayniteDb.Count;

                    string CancelText = string.Empty;
                    List<Game> gamesUpdated = new List<Game>();

                    foreach (Game game in PlayniteDb)
                    {
                        a.Text = $"LibraryManagement - {ResourceProvider.GetString("LOCLmActionInProgress")}"
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game.Name + (game.Source == null ? string.Empty : $" ({game.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
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

                        a.CurrentProgressValue++;
                    }


                    if (gamesUpdated.Count > 0 && PluginSettings.NotifitcationAfterUpdate)
                    {
                        API.Instance.Notifications.Add(new NotificationMessage(
                             $"LibraryManagement-UpdateTagsToGenres",
                             $"LibraryManagement" + Environment.NewLine + string.Format(ResourceProvider.GetString("LOCLmNotificationsUpdate"), gamesUpdated.Count, ResourceProvider.GetString("LOCLmTagsToGenres")),
                             NotificationType.Info,
                             () => {
                                 ListDataUpdated listDataUpdated = new ListDataUpdated(gamesUpdated);
                                 Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCLmTagsToGenresUpdated"), listDataUpdated);
                                 _ = windowExtension.ShowDialog();
                             }
                         ));
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");

                    API.Instance.Database.EndBufferUpdate();
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

            game?.Tags?.ForEach(y =>
            {
                LmTagToCategory finded = PluginSettings.ListTagsToCategories.FirstOrDefault(x => x.TagId == y.Id);
                if (finded != null)
                {
                    IsUpdated = true;
                    _ = game.TagIds.Remove(y.Id);
                    if (game.CategoryIds != null)
                    {
                        _ = game.CategoryIds.AddMissing(finded.CategoryId);
                    }
                    else
                    {
                        game.CategoryIds = new List<Guid> { finded.CategoryId };
                    }
                }
            });

            if (IsUpdated)
            {
                API.Instance.MainView.UIDispatcher?.Invoke(() => API.Instance.Database.Games.Update(game));
            }

            return IsUpdated;
        }
        #endregion


        private ICollection<Game> GetGamesToUpdate(bool onlyRecentlyUpdated)
        {
            return onlyRecentlyUpdated
                ? API.Instance.Database.Games.Where(g => g.Added > PluginSettings.LastAutoLibUpdateAssetsDownload || g.Modified > PluginSettings.LastAutoLibUpdateAssetsDownload).ToList()
                : (ICollection<Game>)API.Instance.Database.Games;
        }
    }
}
