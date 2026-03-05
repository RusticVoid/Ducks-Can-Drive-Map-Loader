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

[assembly: MelonInfo(typeof(DCDMapLoader.MapLoader), "DCDMapLoader", "1.0.9", "RusticVoid")]
[assembly: MelonGame("Joseph Cook", "Ducks Can Drive")]

/*
    Version 1.0.9
*/

namespace DCDMapLoader
{
    public class MapLoader : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("DCDMapLoader loaded!");

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
            if (SceneManager.GetActiveScene().name == "Lobby"
             && PhotonNetwork.InRoom
             && PhotonNetwork.IsMasterClient
             && PhotonNetwork.CurrentRoom.IsVisible == false)
            {
                MelonEvents.OnGUI.Subscribe(customTrackMenu.menu, 100);
            } else {
                    MelonEvents.OnGUI.Unsubscribe(customTrackMenu.menu);
            }
        }
    }

    [HarmonyPatch(typeof(RoundTimer), "LoadRace")]
    public class Patch_LoadRace
    {

        static bool Prefix(RoundTimer __instance, ref IEnumerator __result)
        {
            if (customTrackMenu.BaseGameMapsOnly == true)
                return true;
            
            __result = CustomLoadRace(__instance);
            return false; // Skip original method
        }

        static IEnumerator CustomLoadRace(RoundTimer instance)
        {
            var loadingRaceField = AccessTools.Field(typeof(RoundTimer), "loadingRace");
            loadingRaceField.SetValue(instance, true);

            yield return new WaitForSeconds(2.5f);

            // Get base maps from RoundTimer
            var tracksField = AccessTools.Field(typeof(RoundTimer), "tracks");
            string[] baseTracks = (string[])tracksField.GetValue(instance);

            List<System.Action> loadActions = new List<System.Action>();

            // Add base maps if allowed
            if (!customTrackMenu.CustomMapsOnly)
            {
                for (int i = 0; i < baseTracks.Length; i++)
                {
                    int index = i;
                    loadActions.Add(() =>
                    {
                        PhotonNetwork.LoadLevel(baseTracks[index]);
                    });
                }
            }

            // Add custom maps if allowed
            if (!customTrackMenu.BaseGameMapsOnly)
            {
                for (int i = 0; i < customTrackLoader.customTracks.Count; i++)
                {
                    int index = i;
                    if (customTrackLoader.customTracks[i].isCity) {
                        continue;
                    }
                    loadActions.Add(() =>
                    {
                        customTrackLoader.LoadRace(index);
                    });
                }
            }

            if (loadActions.Count == 0)
                yield break;

            int chosen = UnityEngine.Random.Range(0, loadActions.Count);
            loadActions[chosen].Invoke();
        }
    }
}