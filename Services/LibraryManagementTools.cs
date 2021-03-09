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

namespace LibraryManagement.Services
{
    public class LibraryManagementTools
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static IResourceProvider resources = new ResourceProvider();

        private LibraryManagement _plugin;
        private LibraryManagementSettings _settings;
        private IPlayniteAPI _PlayniteApi;


        public LibraryManagementTools(LibraryManagement plugin, IPlayniteAPI PlayniteApi, LibraryManagementSettings settings)
        {
            _plugin = plugin;
            _settings = settings;
            _PlayniteApi = PlayniteApi;
        }


        #region Genres
        public void SetGenres(bool OnlyToDay = false)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"LibraryManagement - {resources.GetString("LOCLmActionInProgress")}",
                true
            );
            globalProgressOptions.IsIndeterminate = false;

            _PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();


                    // Add new genres that not exist
                    List<Genre> PlayniteGenres = _PlayniteApi.Database.Genres.ToList();
                    foreach (LmGenreEquivalences lmGenreEquivalences in _settings.ListGenreEquivalences)
                    {
                        if (lmGenreEquivalences.Id == null)
                        {
                            Genre genre = new Genre(lmGenreEquivalences.NewName);
                            lmGenreEquivalences.Id = genre.Id;

                            _PlayniteApi.Database.Genres.Add(genre);
                            _plugin.SavePluginSettings(_settings);
                        }
                    }


                    // Remplace genres
                    var PlayniteDb = _PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = _PlayniteApi.Database.Games
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
                            List<Genre> AllGenresOld = GameGenres.FindAll(x => _settings.ListGenreEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                            if (AllGenresOld.Count > 0)
                            {
                                // Remove all
                                foreach (Genre genre in AllGenresOld)
                                {
                                    game.GenreIds.Remove(genre.Id);
                                }  

                                // Set all
                                foreach(LmGenreEquivalences item in _settings.ListGenreEquivalences.FindAll(x => x.OldNames.Any(y => AllGenresOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                                {
                                    if (item.Id != null)
                                    {
                                        game.GenreIds.Add((Guid)item.Id);
                                    }
                                }
                                
                                _PlayniteApi.Database.Games.Update(game);                   
                            }

                            // Exclusion
                            if (_settings.ListGenreExclusion.Count > 0)
                            {
                                foreach (string GenreName in _settings.ListGenreExclusion)
                                {
                                    Genre genreDelete = game.Genres.Find(x => x.Name.ToLower() == GenreName.ToLower());
                                    if (genreDelete != null)
                                    {
                                        game.GenreIds.Remove(genreDelete.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
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

            _PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();


                    // Add new genres that not exist
                    List<GameFeature> PlayniteFeatures = _PlayniteApi.Database.Features.ToList();
                    foreach (LmFeatureEquivalences lmFeatureEquivalences in _settings.ListFeatureEquivalences)
                    {
                        if (lmFeatureEquivalences.Id == null)
                        {
                            GameFeature feature = new GameFeature(lmFeatureEquivalences.NewName);
                            lmFeatureEquivalences.Id = feature.Id;

                            _PlayniteApi.Database.Features.Add(feature);
                            _plugin.SavePluginSettings(_settings);
                        }
                    }


                    // Remplace genres
                    var PlayniteDb = _PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = (IItemCollection<Game>)_PlayniteApi.Database.Games
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

                        List<GameFeature> gameFeatures = game.Features;

                        if (gameFeatures != null && gameFeatures.Count > 0)
                        {
                            // Rename
                            List<GameFeature> AllFeaturesOld = gameFeatures.FindAll(x => _settings.ListFeatureEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                            if (AllFeaturesOld.Count > 0)
                            {
                                // Remove all
                                foreach (GameFeature feature in AllFeaturesOld)
                                {
                                    game.FeatureIds.Remove(feature.Id);
                                }

                                // Set all
                                foreach (LmFeatureEquivalences item in _settings.ListFeatureEquivalences.FindAll(x => x.OldNames.Any(y => AllFeaturesOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                                {
                                    if (item.Id != null)
                                    {
                                        game.FeatureIds.Add((Guid)item.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
                            }

                            // Exclusion
                            if (_settings.ListFeatureExclusion.Count > 0)
                            {
                                foreach (string FeatureName in _settings.ListFeatureExclusion)
                                {
                                    GameFeature featureDelete = game.Features.Find(x => x.Name.ToLower() == FeatureName.ToLower());
                                    if (featureDelete != null)
                                    {
                                        game.FeatureIds.Remove(featureDelete.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
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

            _PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();


                    // Add new tags that not exist
                    List<Tag> PlayniteTags = _PlayniteApi.Database.Tags.ToList();
                    foreach (LmTagsEquivalences lmTagsEquivalences in _settings.ListTagsEquivalences)
                    {
                        if (lmTagsEquivalences.Id == null)
                        {
                            Tag tag = new Tag(lmTagsEquivalences.NewName);
                            lmTagsEquivalences.Id = tag.Id;

                            _PlayniteApi.Database.Tags.Add(tag);
                            _plugin.SavePluginSettings(_settings);
                        }
                    }


                    // Remplace tags
                    var PlayniteDb = _PlayniteApi.Database.Games.Where(x => x.Hidden == true || x.Hidden == false);
                    if (OnlyToDay)
                    {
                        PlayniteDb = (IItemCollection<Game>)_PlayniteApi.Database.Games
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
                            List<Tag> AllTagsOld = Tags.FindAll(x => _settings.ListTagsEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                            if (AllTagsOld.Count > 0)
                            {
                                // Remove all
                                foreach (Tag tag in AllTagsOld)
                                {
                                    game.TagIds.Remove(tag.Id);
                                }

                                // Set all
                                foreach (LmTagsEquivalences item in _settings.ListTagsEquivalences.FindAll(x => x.OldNames.Any(y => AllTagsOld.Any(z => z.Name.ToLower() == y.ToLower()))))
                                {
                                    if (item.Id != null)
                                    {
                                        game.TagIds.Add((Guid)item.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
                            }

                            // Exclusion
                            if (_settings.ListTagsExclusion.Count > 0)
                            {
                                foreach (string TagName in _settings.ListTagsExclusion)
                                {
                                    Tag TagDelete = game.Tags.Find(x => x.Name.ToLower() == TagName.ToLower());
                                    if (TagDelete != null)
                                    {
                                        game.TagIds.Remove(TagDelete.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
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
