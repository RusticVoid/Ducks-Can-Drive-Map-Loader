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
    public class breadScoreCounter
    {
        public static int BreadDeliveredScore = 0;

        public static void resetBreadScore() 
        {
            BreadDeliveredScore = 0;
            MelonLogger.Msg("Bread Score Reset!");
        }
    }

    // Harmony patch targeting the OnTriggerEnter method of the House class
    [HarmonyPatch(typeof(House), "OnTriggerEnter")]
    public class Patch_HouseDelivery
    {
        // Keep track of which houses have already received a delivery
        // This prevents multiple logs from multiple colliders or repeated triggers
        static readonly System.Collections.Generic.HashSet<House> deliveredHouses = new System.Collections.Generic.HashSet<House>();

        // Postfix method runs **after** the original OnTriggerEnter method
        // __instance is the House instance that was triggered
        // other is the Collider that entered the trigger
        static void Postfix(House __instance, Collider other)
        {
            // Ignore collisions that are not from the player
            if (!other.CompareTag("Player")) return;

            // Only continue if the collider belongs to the main car object
            // Many cars have multiple colliders (wheels, body), we only want one
            var car = other.GetComponent<Car>();
            if (car == null) return;

            // In multiplayer, ignore other players’ cars
            var pv = other.GetComponent<PhotonView>();
            if (pv != null && !pv.IsMine) return;

            // Only log the delivery once per house
            if (deliveredHouses.Contains(__instance)) return;
            deliveredHouses.Add(__instance);

            // Increase bread score when bread is delivered
            breadScoreCounter.BreadDeliveredScore++;

            MelonLogger.Msg($"Bread delivered! Bread Score: {breadScoreCounter.BreadDeliveredScore}");
        }
    }
}