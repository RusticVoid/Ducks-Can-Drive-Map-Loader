using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(DCDMapLoader.MapLoader), "DCDMapLoader", "1.0.2", "RusticVoid")]
[assembly: MelonGame("Joseph Cook", "Ducks Can Drive")]

/*
    Version 1.0.2
*/

namespace DCDMapLoader
{
    public class MapLoader : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Expanded loaded!");

            string mapsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
            if (!Directory.Exists(mapsPath))
            {
                MelonLogger.Msg("No maps folder found! Creating one now.");
                Directory.CreateDirectory(mapsPath);
            }

            customTrackLoader.InitCustomMaps();

            // Create a Harmony instance for patching game methods
            var harmony = new HarmonyLib.Harmony("com.rusticvoid.dcdmaploader");
            // Apply all [HarmonyPatch] attributes in this assembly
            harmony.PatchAll();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonEvents.OnGUI.Unsubscribe(customTrackMenu.menu);

            customTrackLoader.initCustomMapObjects(buildIndex, sceneName);
        }

        public override void OnUpdate() {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                customTrackLoader.LoadRace(0); // 0 is the MapID (its position in the customTrackLoader.customTracks array)
            }
        }
    }


    [HarmonyPatch(typeof(RoundTimer), "LoadRace")]
    public class Patch_LoadRace
    {

        static bool Prefix(RoundTimer __instance, ref IEnumerator __result)
        {
            if (customTrackMenu.CustomMapsOnly == true) {
                __result = CustomLoadRace(__instance);
                return false; // Skip original method
            } else {
                return true;
            }
        }

        static IEnumerator CustomLoadRace(RoundTimer instance)
        {
            var loadingRaceField = AccessTools.Field(typeof(RoundTimer), "loadingRace");
            loadingRaceField.SetValue(instance, true);

            yield return new WaitForSeconds(2.5f);

            int chosenMap = UnityEngine.Random.Range(0, customTrackLoader.customTracks.Count);
            customTrackLoader.LoadRace(chosenMap);
        }
    }
}