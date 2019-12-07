using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FarmTypeDisplay
{
    class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += ReturnedToTitle;
        }

        private void ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
                if (TitleMenu.subMenu is LoadGameMenu)
                {
                    List<LoadGameMenu.MenuSlot> slots = this.Helper.Reflection.GetField<List<LoadGameMenu.MenuSlot>>(TitleMenu.subMenu, "menuSlots").GetValue();
                    if (slots.Count < 1)
                    {
                        return;
                    }
                    foreach (LoadGameMenu.MenuSlot slot in slots)
                    {
                        LoadGameMenu.SaveFileSlot saveSlot = slot as LoadGameMenu.SaveFileSlot;
                        Farmer farmer = saveSlot.Farmer;

                        int whichFarm = ReadFarmTypeFromSaveFile(farmer.slotName);

                        string farmType = "";

                        switch (whichFarm)
                        {
                            case 0:
                                farmType = "Reg. ";
                                break;
                            case 1:
                                farmType = "Riv. ";
                                break;
                            case 2:
                                farmType = "For. ";
                                break;
                            case 3:
                                farmType = "Mtn. ";
                                break;
                            case 4:
                                farmType = "Com. ";
                                break;
                            case 5:
                                farmType = "4Co. ";
                                break;
                            default:
                                break;
                        }

                        string farmName = this.Helper.Reflection.GetField<NetString>(farmer, "farmName").GetValue().Value;

                        this.Helper.Reflection.GetField<NetString>(farmer, "farmName").SetValue(new NetString(farmType + farmName));
                    }

                this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
                return;                
            }      
        }

        private int ReadFarmTypeFromSaveFile(string fileName)
        {
            Stream stream = (Stream)null;
            string fullFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves", fileName, fileName);
            if (!File.Exists(fullFilePath))
            {
                fullFilePath += ".xml";
            }
            stream = (Stream)File.Open(fullFilePath, FileMode.Open);

            SaveGame pendingSaveGame = (SaveGame)null;
            Task deserializeTask = new Task((Action)(() =>
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                pendingSaveGame = (SaveGame)SaveGame.serializer.Deserialize(stream);
            }));
            deserializeTask.Start();
            deserializeTask.Wait();
            stream.Dispose();
            return pendingSaveGame.whichFarm;
        }
    }
}
