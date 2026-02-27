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

[assembly: MelonInfo(typeof(DCDMapLoader.MapLoader), "DCDMapLoader", "1.0.0", "RusticVoid")]
[assembly: MelonGame("Joseph Cook", "Ducks Can Drive")]

namespace DCDMapLoader
{
    public class MapLoader : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Expanded loaded!");



            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Maps/"))
            {
            }
            else
            {
                MelonLogger.Msg("No maps folder found! Creating one now.");
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

            customTrackLoader.initCustomMapObjects(buildIndex);
        }

        public override void OnUpdate() {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                customTrackLoader.LoadRace(0); // 0 is the MapID (its position in the customTrackLoader.customTracks array)
            }
        }
    }
}