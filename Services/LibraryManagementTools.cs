﻿using CommonPluginsShared;
using LibraryManagement.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                    var PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null)
                            .Where(x => ((DateTime)x.Added).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);
                        UpdateGenre(game);

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetGenres(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
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

        private void UpdateGenre(Game game)
        {
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
                    }

                    // Set all
                    foreach (LmGenreEquivalences item in PluginSettings.ListGenreEquivalences.FindAll(x => x.OldNames.Any(y => AllGenresOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            game.GenreIds.Add((Guid)item.Id);
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait(); 
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
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait(); ;
                }
            }
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
                    var PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null)
                            .Where(x => ((DateTime)x.Added).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;

                    foreach (Game game in PlayniteDb.ToList())
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);
                        UpdateFeature(game);

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetFeatures(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
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

        private void UpdateFeature(Game game)
        {
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
                    }

                    // Set all
                    foreach (LmFeatureEquivalences item in PluginSettings.ListFeatureEquivalences.FindAll(x => x.OldNames.Any(y => AllFeaturesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            game.FeatureIds.Add((Guid)item.Id);
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait();
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
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait(); ;
                }
            }
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
                    var PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null)
                            .Where(x => ((DateTime)x.Added).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);
                        UpdateTags(game);

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetTags(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
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

        private void UpdateTags(Game game)
        {
            List<Tag> Tags = game.Tags;

            if (Tags != null && Tags.Count > 0)
            {
                // Rename
                List<Tag> AllTagsOld = Tags.FindAll(x => PluginSettings.ListTagsEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                if (AllTagsOld.Count > 0)
                {
                    // Remove all
                    foreach (Tag tag in AllTagsOld)
                    {
                        game.TagIds.Remove(tag.Id);
                    }

                    // Set all
                    foreach (LmTagsEquivalences item in PluginSettings.ListTagsEquivalences.FindAll(x => x.OldNames.Any(y => AllTagsOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            game.TagIds.Add((Guid)item.Id);
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait(); 
                }

                // Exclusion
                if (PluginSettings.ListTagsExclusion.Count > 0)
                {
                    foreach (string TagName in PluginSettings.ListTagsExclusion)
                    {
                        Tag TagDelete = game.Tags.Find(x => x.Name.ToLower() == TagName.ToLower());
                        if (TagDelete != null)
                        {
                            game.TagIds.Remove(TagDelete.Id);
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait(); 
                }
            }
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
                    var PlayniteDb = PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = PlayniteApi.Database.Games
                            .Where(x => x.Added != null)
                            .Where(x => ((DateTime)x.Added).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    activateGlobalProgress.ProgressMaxValue = (double)PlayniteDb.Count();

                    string CancelText = string.Empty;

                    foreach (Game game in PlayniteDb)
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);
                        UpdateCompanies(game);
                        UpdateCompanies(game, true);

                        activateGlobalProgress.CurrentProgressValue++;
                    }


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"Task SetCompanies(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
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

        private void UpdateCompanies(Game game, bool IsPublishers = false)
        {
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
                        }
                        else
                        {
                            game.DeveloperIds.Remove(company.Id);
                        }
                    }

                    // Set all
                    foreach (LmCompaniesEquivalences item in PluginSettings.ListCompaniesEquivalences.FindAll(x => x.OldNames.Any(y => AllCompaniesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                    {
                        if (item.Id != null)
                        {
                            if (IsPublishers)
                            {
                                game.PublisherIds.Add((Guid)item.Id);
                            }
                            else
                            {
                                game.DeveloperIds.Add((Guid)item.Id);
                            }
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait();
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
                            }
                        }
                        else
                        {
                            Company CompanyDelete = game.Developers.Find(x => x.Name.ToLower() == CompanyName.ToLower());
                            if (CompanyDelete != null)
                            {
                                game.DeveloperIds.Remove(CompanyDelete.Id);
                            }
                        }
                    }

                    Application.Current.Dispatcher?.BeginInvoke((Action)delegate
                    {
                        PlayniteApi.Database.Games.Update(game);
                    }).Wait();
                }
            }
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
    }
}
