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

namespace DCDMapLoader
{
    public class customTrackMenu
    {
        public static void menu()
        {
            int ButtonHeight = 75;
            int menuWidth = 365;
            Vector2 MenuPos = new Vector2(Screen.width - menuWidth - 20f, 20f);

            GUI.Box(new Rect(MenuPos.x, MenuPos.y, menuWidth, 500), "Custom Tracks");

            for (int i = 0; i < customTrackLoader.customTracks.Count; i++) {
                var track = customTrackLoader.customTracks[i];

                Rect buttonRect = new Rect(MenuPos.x + 10, MenuPos.y + 30 + (ButtonHeight * i), menuWidth - 20, ButtonHeight);

                if (GUI.Button(buttonRect, ""))
                {
                    MelonLogger.Msg($"Loading track {i}: {track.name}");
                    customTrackLoader.LoadRace(i);
                }

                Rect iconRect = new Rect(buttonRect.x, buttonRect.y, ButtonHeight, ButtonHeight);
                GUI.DrawTexture(iconRect, track.icon);

                Rect textRect = new Rect(buttonRect.x + iconRect.width + 2, buttonRect.y, buttonRect.width - ButtonHeight, ButtonHeight);
                GUI.Label(textRect, track.name+"\n"+track.desc);

            }
        }
    }

    [HarmonyPatch(typeof(Launcher), "OnJoinedRoom")]
    public class Patch_lobbyCreated
    {
        static void Postfix()
        {
            if (SceneManager.GetActiveScene().name == "Lobby" && PhotonNetwork.IsMasterClient)
            {
                MelonEvents.OnGUI.Subscribe(customTrackMenu.menu, 100);
            }
        }
    }

    [HarmonyPatch(typeof(Launcher), "LeaveRoom")]
    public class Patch_lobbyLeft
    {
        static void Postfix()
        {
            MelonEvents.OnGUI.Unsubscribe(customTrackMenu.menu);
        }
    }
}