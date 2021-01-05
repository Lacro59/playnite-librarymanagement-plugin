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


        public void SetGenres()
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
                    var PlayniteDb = _PlayniteApi.Database.Games;
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
                            List<Genre> AllGenresOld = GameGenres.FindAll(x => _settings.ListGenreEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                            if (AllGenresOld.Count > 0)
                            {
                                // Remove all
                                foreach (Genre genre in AllGenresOld)
                                {
                                    game.GenreIds.Remove(genre.Id);
                                }  

                                // Set all
                                foreach(LmGenreEquivalences item in _settings.ListGenreEquivalences.FindAll(x => x.OldNames.Any(y => y.ToLower() == x.Name.ToLower())))
                                {
                                    if (item.Id != null)
                                    {
                                        game.GenreIds.Add((Guid)item.Id);
                                    }
                                }
                                
                                _PlayniteApi.Database.Games.Update(game);                   
                            }
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"LibraryManagement - Task SetGenres(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "LibraryManagement");
                }
            }, globalProgressOptions);
        }


        public void SetFeatures()
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
                    var PlayniteDb = _PlayniteApi.Database.Games;
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
                            List<GameFeature> AllFeaturesOld = gameFeatures.FindAll(x => _settings.ListFeatureEquivalences.Any(y => y.OldNames.Any(z => z.ToLower() == x.Name.ToLower())));

                            if (AllFeaturesOld.Count > 0)
                            {
                                // Remove all
                                foreach (GameFeature feature in AllFeaturesOld)
                                {
                                    game.FeatureIds.Remove(feature.Id);
                                }

                                // Set all
                                foreach (LmFeatureEquivalences item in _settings.ListFeatureEquivalences.FindAll(x => x.OldNames.Any(y => y.ToLower() == x.Name.ToLower())))
                                {
                                    if (item.Id != null)
                                    {
                                        game.FeatureIds.Add((Guid)item.Id);
                                    }
                                }

                                _PlayniteApi.Database.Games.Update(game);
                            }
                        }

                        activateGlobalProgress.CurrentProgressValue++;
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    logger.Info($"LibraryManagement - Task SetFeatures(){CancelText} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {activateGlobalProgress.CurrentProgressValue}/{(double)PlayniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, "LibraryManagement");
                }
            }, globalProgressOptions);
        }
    }
}
