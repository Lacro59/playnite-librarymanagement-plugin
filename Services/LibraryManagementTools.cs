using CommonPluginsShared;
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
        public void SetGenres(bool OnlyToDay = false)
        {
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


                    // Add new genres that not exist
                    List<Genre> PlayniteGenres = PlayniteApi.Database.Genres.ToList();
                    foreach (LmGenreEquivalences lmGenreEquivalences in PluginSettings.ListGenreEquivalences)
                    {
                        if (lmGenreEquivalences.Id == null)
                        {
                            Genre genre = new Genre(lmGenreEquivalences.NewName);
                            lmGenreEquivalences.Id = genre.Id;

                            PlayniteApi.Database.Genres.Add(genre);
                            Plugin.SavePluginSettings(PluginSettings);
                        }
                    }


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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
                            }
                        }

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
        public void SetFeatures(bool OnlyToDay = false)
        {
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


                    // Add new genres that not exist
                    List<GameFeature> PlayniteFeatures = PlayniteApi.Database.Features.ToList();
                    foreach (LmFeatureEquivalences lmFeatureEquivalences in PluginSettings.ListFeatureEquivalences)
                    {
                        if (lmFeatureEquivalences.Id == null)
                        {
                            GameFeature feature = new GameFeature(lmFeatureEquivalences.NewName);
                            lmFeatureEquivalences.Id = feature.Id;

                            PlayniteApi.Database.Features.Add(feature);
                            Plugin.SavePluginSettings(PluginSettings);
                        }
                    }


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

                    foreach (Game game in PlayniteDb.ToList())
                    {
                        if (activateGlobalProgress.CancelToken.IsCancellationRequested)
                        {
                            CancelText = " canceled";
                            break;
                        }

                        Thread.Sleep(10);

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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
                            }
                        }

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
        public void SetTags(bool OnlyToDay = false)
        {
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


                    // Add new tags that not exist
                    List<Tag> PlayniteTags = PlayniteApi.Database.Tags.ToList();
                    foreach (LmTagsEquivalences lmTagsEquivalences in PluginSettings.ListTagsEquivalences)
                    {
                        if (lmTagsEquivalences.Id == null)
                        {
                            Tag tag = new Tag(lmTagsEquivalences.NewName);
                            lmTagsEquivalences.Id = tag.Id;

                            PlayniteApi.Database.Tags.Add(tag);
                            Plugin.SavePluginSettings(PluginSettings);
                        }
                    }


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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
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

                                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    PlayniteApi.Database.Games.Update(game);
                                });
                            }
                        }

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
    }
}
