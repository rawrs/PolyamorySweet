﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PolyamorySweetLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            sc.RegisterSerializerType(typeof(PolyamoryLocation));

            sc.RegisterSerializerType(typeof(LantanaLagoon));

            sc.RegisterSerializerType(typeof(LantanaTemple));


            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Marry",
                getValue: () => Config.MinPointsToMarry,
                setValue: value => Config.MinPointsToMarry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Date",
                getValue: () => Config.MinPointsToDate,
                setValue: value => Config.MinPointsToDate = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Hostile Divorces",
                getValue: () => Config.PreventHostileDivorces,
                setValue: value => Config.PreventHostileDivorces = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Complex Divorces",
                getValue: () => Config.ComplexDivorce,
                setValue: value => Config.ComplexDivorce = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Roommate Romance",
                getValue: () => Config.RoommateRomance,
                setValue: value => Config.RoommateRomance = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max children",
                getValue: () => Config.MaxChildren,
                setValue: value => Config.MaxChildren = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Parent Names",
                getValue: () => Config.ShowParentNames,
                setValue: value => Config.ShowParentNames = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Buy Pendants Anytime",
                getValue: () => Config.BuyPendantsAnytime,
                setValue: value => Config.BuyPendantsAnytime = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pendant Price",
                getValue: () => Config.PendantPrice,
                setValue: value => Config.PendantPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Bed",
                getValue: () => Config.PercentChanceForSpouseInBed,
                setValue: value => Config.PercentChanceForSpouseInBed = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Kitchen",
                getValue: () => Config.PercentChanceForSpouseInKitchen,
                setValue: value => Config.PercentChanceForSpouseInKitchen = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For Spouse to Be In Patio",
                getValue: () => Config.PercentChanceForSpouseAtPatio,
                setValue: value => Config.PercentChanceForSpouseAtPatio = value,
                min: 0,
                max: 100
            );

            configMenu.AddNumberOption(
              mod: ModManifest,
              name: () => "Percent Chance For Pregnancy Question (0.0 - 1)",
              getValue: () => Config.PercentChanceForBirthingQuestion,
              setValue: value => Config.PercentChanceForBirthingQuestion = value,
               tooltip: () => "Sets the chance for a birthing question at night. 1 is 100 percent."
          );

            configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "Percent Chance For Birth Sex to be a Girl (0.0 - 1)",
            getValue: () => Config.PercentChanceForBirthSex,
            setValue: value => Config.PercentChanceForBirthSex = value,
             tooltip: () => "Sets whether the next baby will be a girl. Default is 0.6, a round number reflecting population norms."
        );


            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Impregnating Mother",
                getValue: () => Config.ImpregnatingMother,
                setValue: value => Config.ImpregnatingMother = value,
                tooltip: () => "Allows a female farmer to impregnate her wife. Must be set to false if Impregnating Femme NPC is set to true."
            );

            configMenu.AddBoolOption(
               mod: ModManifest,
               name: () => "Impregnating Femme NPC",
               getValue: () => Config.ImpregnatingFemmeNPC,
               setValue: value => Config.ImpregnatingFemmeNPC = value,
               tooltip: () => "Allows a female Farmer to get impregnated by her wife. Must be set to false if Impregnating Mother is set to true."
           );






            

        LoadModApis();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            currentSpouses.Clear();
            currentUnofficialSpouses.Clear();
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {


            SetAllNPCsDatable(); //What the hell have I done here? What is this???
            ResetSpouses(Game1.player);
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ResetDivorces();
            ResetSpouses(Game1.player);
            BabyTonight = false;
            BabyTonightSpouse = String.Empty;
            AphroditeFlowerGiven = false;
            PatioPlacement = false;
            PorchPlacement = false;


            if (Game1.IsMasterGame)
            {
                foreach (GameLocation location in GetAllLocations())
                {
                    if (location is FarmHouse fh)
                    {
                        PlaceSpousesInFarmhouse(fh);
                    }
                }

                Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                farmHelperSpouse = GetRandomSpouse(Game1.MasterPlayer);
            }
            foreach (Farmer f in Game1.getAllFarmers())
            {
                var spouses = GetSpouses(f, true).Keys;
                foreach (string s in spouses)
                {
                    SMonitor.Log($"{f.Name} is married to {s}");
                }
            }
        }


        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach (GameLocation location in GetAllLocations())
            {

                if (location is FarmHouse fh)
                {
                    if (fh.owner == null)
                        continue;

                    List<string> allSpouses = GetSpouses(fh.owner, true).Keys.ToList();
                    List<string> bedSpouses = ReorderSpousesForSleeping(allSpouses.FindAll((s) => Config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

                    using (IEnumerator<NPC> characters = fh.characters.GetEnumerator())
                    {
                        while (characters.MoveNext())
                        {
                            var character = characters.Current;
                            if (!(character.currentLocation == fh))
                            {
                                character.farmerPassesThrough = false;
                                character.HideShadow = false;
                                character.isSleeping.Value = false;
                                continue;
                            }

                            if (allSpouses.Contains(character.Name))
                            {

                                if (IsInBed(fh, character.GetBoundingBox()))
                                {
                                    character.farmerPassesThrough = true;

                                    if (!character.isMoving() && (kissingAPI == null || kissingAPI.LastKissed(character.Name) < 0 || kissingAPI.LastKissed(character.Name) > 2))
                                    {
                                        Vector2 bedPos = GetSpouseBedPosition(fh, character.Name);
                                        if (Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 600)
                                        {
                                            character.position.Value = bedPos;

                                            if (Game1.timeOfDay >= 2200)
                                            {
                                                character.ignoreScheduleToday = true;
                                            }
                                            if (!character.isSleeping.Value)
                                            {
                                                character.isSleeping.Value = true;

                                            }
                                            if (character.Sprite.CurrentAnimation == null)
                                            {
                                                if (!HasSleepingAnimation(character.Name))
                                                {
                                                    character.Sprite.StopAnimation();
                                                    character.faceDirection(0);
                                                }
                                                else
                                                {
                                                    character.playSleepingAnimation();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            character.faceDirection(3);
                                            character.isSleeping.Value = false;
                                        }
                                    }
                                    else
                                    {
                                        character.isSleeping.Value = false;
                                    }
                                    character.HideShadow = true;
                                }
                                else if (Game1.timeOfDay < 2000 && Game1.timeOfDay > 600)
                                {
                                    character.farmerPassesThrough = false;
                                    character.HideShadow = false;
                                    character.isSleeping.Value = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}