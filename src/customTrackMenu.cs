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
        private static Vector2 scrollPosition = Vector2.zero;

        public static bool CustomMapsOnly = false;

        public static void menu()
        {
            int ButtonHeight = 75;
            int menuWidth = 365;
            int menuHeight = 500;
            Vector2 MenuPos = new Vector2(Screen.width - menuWidth - 20f, 20f);

            GUI.Box(new Rect(MenuPos.x, MenuPos.y, menuWidth, menuHeight), "Custom Tracks");

            int contentHeight = customTrackLoader.customTracks.Count * ButtonHeight + 10;

            scrollPosition = GUI.BeginScrollView(
                new Rect(MenuPos.x, MenuPos.y + 25, menuWidth, menuHeight - 25),
                scrollPosition,
                new Rect(0, 0, menuWidth - 20, contentHeight)
            );

            for (int i = 0; i < customTrackLoader.customTracks.Count; i++) {
                var track = customTrackLoader.customTracks[i];

                Rect buttonRect = new Rect(10, 10 + (ButtonHeight * i), menuWidth - 20, ButtonHeight);

                if (GUI.Button(buttonRect, ""))
                {
                    customTrackLoader.LoadRace(i);
                }

                Rect iconRect = new Rect(buttonRect.x, buttonRect.y, ButtonHeight, ButtonHeight);
                GUI.DrawTexture(iconRect, track.icon);

                Rect textRect = new Rect(buttonRect.x + iconRect.width + 2, buttonRect.y, buttonRect.width - ButtonHeight, ButtonHeight);
                GUI.Label(textRect, track.name+"\n"+track.desc);
            }
            
            GUI.EndScrollView();

            Vector2 OptionsMenu = new Vector2(MenuPos.x, menuHeight+20);

            GUI.Box(new Rect(OptionsMenu.x, OptionsMenu.y, menuWidth, 150), "Custom Tracks Options");
            CustomMapsOnly = GUI.Toggle(new Rect(OptionsMenu.x+10, OptionsMenu.y+20, 200, 30), CustomMapsOnly, "Custom Maps Only");
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