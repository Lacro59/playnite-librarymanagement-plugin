using LibraryManagement.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagement.Services
{
    public class IcoFeatures
    {
        public static List<ItemFeature> GetAvailableItemFeatures(LibraryManagementSettingsViewModel PluginSettings, Game GameContext)
        {
            List<ItemFeature> Result = new List<ItemFeature>();

            if (GameContext != null && GameContext.Features != null)
            {
                Result = PluginSettings.Settings.ItemFeatures.Where(
                    x => GameContext.Features.Any(y => y.Name.ToLower() == x.NameAssociated.ToLower())
                ).ToList();
            }

            return Result;
        }
    }
}
