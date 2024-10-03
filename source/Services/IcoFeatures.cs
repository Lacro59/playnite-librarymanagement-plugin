using CommonPluginsShared.Extensions;
using LibraryManagement.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryManagement.Services
{
    public class IcoFeatures
    {
        public static List<ItemFeature> GetAvailableItemFeatures(LibraryManagementSettingsViewModel pluginSettings, Game gameContext)
        {
            List<ItemFeature> result = new List<ItemFeature>();
            if (gameContext != null && gameContext.Features != null)
            {
                result = pluginSettings.Settings.ItemFeatures.Where(
                    x => 
                    {
                        GameFeature feature = gameContext.Features.FirstOrDefault(y => y.Name.IsEqual(x.NameAssociated)); 
                        if (feature != null)
                        {
                            x.Feature = feature;
                            return true;
                        }
                        x.Feature = null;
                        return false;
                    }
                ).ToList();

            }
            return result;
        }
    }
}
