﻿
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace PolyamorySweetLove
{
    public class PolyamorySweetLoveAPI
    {
        public void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            ModEntry.PlaceSpousesInFarmhouse(farmHouse);
        }
        public Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all = false)
        {
            return ModEntry.GetSpouses(farmer, all);
        }
        public Dictionary<string, NPC> GetSpouses(Farmer farmer, int all = -1)
        {
            return ModEntry.GetSpouses(farmer, all != 0);
        }
        public void SetLastPregnantSpouse(string name)
        {
            ModEntry.lastPregnantSpouse = Game1.getCharacterFromName(name);
        }




    }
}