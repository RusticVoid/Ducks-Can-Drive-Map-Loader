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
using System.Linq;
using UnityEngine.SceneManagement;
using ArcadeVP;

namespace DCDMapLoader
{
    [HarmonyPatch(typeof(Car), "Start")]
    public class Patch_CarStart
    {

        static bool Prefix(Car __instance)
        {
            if (!customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity)
                return true;
            
            CustomStart(__instance);
            return false; // Skip original method
        }

        static void CustomStart(Car __instance)
        {
            // Access private fields using AccessTools
            var pv = AccessTools.Field(typeof(Car), "pv").GetValue(__instance) as PhotonView;
            var rb = AccessTools.Field(typeof(Car), "rb").GetValue(__instance) as Rigidbody;
            var arcadeVehicleController = AccessTools.Field(typeof(Car), "arcadeVehicleController").GetValue(__instance);
            var moneyText = AccessTools.Field(typeof(Car), "moneyText").GetValue(__instance) as UnityEngine.UI.Text;
            var rankBar = AccessTools.Field(typeof(Car), "rankBar").GetValue(__instance);
            var exitButton = AccessTools.Field(typeof(Car), "exitButton").GetValue(__instance) as GameObject;
            var playerCamera = AccessTools.Field(typeof(Car), "playerCamera").GetValue(__instance) as GameObject;

            // Access private methods
            var updateDarkModeMethod = AccessTools.Method(typeof(Car), "UpdateDarkMode");

            if (!pv.IsMine)
            {
                UnityEngine.Object.Destroy(rb);
                return;
            }

            // Check scene index
            if (customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity)
            {
                updateDarkModeMethod.Invoke(__instance, null);

                // Set vehicle stats
                var onRoadSpeedField = AccessTools.Field(arcadeVehicleController.GetType(), "onRoadSpeed");
                var accelerationField = AccessTools.Field(arcadeVehicleController.GetType(), "accelaration");
                var offRoadMultiplierField = AccessTools.Field(arcadeVehicleController.GetType(), "offRoadMultiplier");
                var boostForceField = AccessTools.Field(arcadeVehicleController.GetType(), "boostForce");
                var turnField = AccessTools.Field(arcadeVehicleController.GetType(), "turn");

                onRoadSpeedField.SetValue(arcadeVehicleController, 100f);
                accelerationField.SetValue(arcadeVehicleController, 20f);
                offRoadMultiplierField.SetValue(arcadeVehicleController, 0.5f);
                boostForceField.SetValue(arcadeVehicleController, 50f);
                turnField.SetValue(arcadeVehicleController, 5f);

                // Save car stats
                var saveSystemType = AccessTools.TypeByName("SaveSystem");
                var saveCarMethod = AccessTools.Method(saveSystemType, "SaveCar");
                saveCarMethod.Invoke(null, new object[] { arcadeVehicleController });

                moneyText.gameObject.SetActive(true);
            }
            else
            {
                updateDarkModeMethod.Invoke(__instance, null);

                // Set raceStarted = false
                AccessTools.Field(typeof(Car), "raceStarted").SetValue(__instance, false);

                // Load car data
                var saveSystemType = AccessTools.TypeByName("SaveSystem");
                var loadCarMethod = AccessTools.Method(saveSystemType, "LoadCar");
                var carData = loadCarMethod.Invoke(null, null);

                if (carData != null)
                {
                    var onRoadSpeedField = AccessTools.Field(arcadeVehicleController.GetType(), "onRoadSpeed");
                    var accelerationField = AccessTools.Field(arcadeVehicleController.GetType(), "accelaration");
                    var offRoadMultiplierField = AccessTools.Field(arcadeVehicleController.GetType(), "offRoadMultiplier");
                    var boostForceField = AccessTools.Field(arcadeVehicleController.GetType(), "boostForce");
                    var turnField = AccessTools.Field(arcadeVehicleController.GetType(), "turn");

                    onRoadSpeedField.SetValue(arcadeVehicleController, AccessTools.Field(carData.GetType(), "onRoadSpeed").GetValue(carData));
                    accelerationField.SetValue(arcadeVehicleController, AccessTools.Field(carData.GetType(), "acceleration").GetValue(carData));
                    offRoadMultiplierField.SetValue(arcadeVehicleController, AccessTools.Field(carData.GetType(), "offRoadMultiplier").GetValue(carData));
                    boostForceField.SetValue(arcadeVehicleController, AccessTools.Field(carData.GetType(), "boostForce").GetValue(carData));
                    turnField.SetValue(arcadeVehicleController, AccessTools.Field(carData.GetType(), "handling").GetValue(carData));
                }
            }

            // Load rank bar
            AccessTools.Method(rankBar.GetType(), "LoadRank").Invoke(rankBar, null);

            // Set race started in RoomManager
            AccessTools.Field(typeof(RoomManager), "Instance").GetValue(null);
            RoomManager.Instance.raceStarted = true;

            exitButton.SetActive(true);
            __instance.StartCoroutine("Quack");
            playerCamera.SetActive(true);

            // Load medals
            AccessTools.Field(typeof(Car), "goldMedals").SetValue(__instance, PlayerPrefs.GetInt("Wins", 0));
            AccessTools.Field(typeof(Car), "silverMedals").SetValue(__instance, PlayerPrefs.GetInt("Seconds", 0));
            AccessTools.Field(typeof(Car), "bronzeMedals").SetValue(__instance, PlayerPrefs.GetInt("Thirds", 0));
        }
    }

    [HarmonyPatch(typeof(Car), "FixedUpdate")]
    public class Patch_CarFixedUpdate
    {

        static bool Prefix(Car __instance)
        {
            if (!customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity)
                return true;
            
            CustomFixedUpdate(__instance);
            return false; // Skip original method
        }

        static void CustomFixedUpdate(Car __instance)
        {
            // --- Access private fields via AccessTools ---
            var pv = AccessTools.Field(typeof(Car), "pv").GetValue(__instance) as PhotonView;
            var raceFinished = (bool)AccessTools.Field(typeof(Car), "raceFinished").GetValue(__instance);
            var shownFinishedField = AccessTools.Field(typeof(Car), "shownFinished");
            var resetButton = AccessTools.Field(typeof(Car), "resetButton").GetValue(__instance) as GameObject;
            var duckTransform = AccessTools.Field(typeof(Car), "duckTransform").GetValue(__instance) as Transform;
            var racePositionText = AccessTools.Field(typeof(Car), "racePositionText").GetValue(__instance) as UnityEngine.UI.Text;
            var finishText = AccessTools.Field(typeof(Car), "finishText").GetValue(__instance) as UnityEngine.UI.Text;
            var nameTag = AccessTools.Field(typeof(Car), "nameTag").GetValue(__instance) as GameObject;
            var arcadeVehicleController = AccessTools.Field(typeof(Car), "arcadeVehicleController").GetValue(__instance);
            var playerCamera = AccessTools.Field(typeof(Car), "playerCamera").GetValue(__instance) as GameObject;
            var timerField = AccessTools.Field(typeof(Car), "timer");
            var raceStarted = (bool)AccessTools.Field(typeof(Car), "raceStarted").GetValue(__instance);

            // --- Access private methods ---
            var resetPositionMethod = AccessTools.Method(typeof(Car), "ResetPosition");
            var getTotalTimerMethod = AccessTools.Method(typeof(Car), "FixedUpdate"); // Not needed, but example

            // --- Timer & race logic ---
            float timer = (float)timerField.GetValue(__instance);
            if (!customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity && raceStarted && pv.IsMine)
            {
                timer += Time.deltaTime;
                racePositionText.gameObject.SetActive(true);
                if (timer > 600f)
                {
                    PhotonNetwork.SendAllOutgoingCommands();
                    PhotonNetwork.Disconnect();
                }
                timerField.SetValue(__instance, timer);
            }

            // --- Finish race display ---
            bool shownFinished = (bool)shownFinishedField.GetValue(__instance);
            if (raceFinished && !shownFinished)
            {
                string text = LanguageManager.Instance.currentLang["FINISHED!"];
                finishText.text = pv.Controller.NickName + " " + text;
                finishText.gameObject.SetActive(true);
                racePositionText.gameObject.SetActive(false);
                shownFinishedField.SetValue(__instance, true);
            }
            else if (!raceFinished)
            {
                finishText.gameObject.SetActive(false);
            }

            // --- Always show name tag ---
            nameTag.SetActive(true);

            if (!pv.IsMine)
                return;

            // --- Duck rotation ---
            duckTransform.localRotation = Quaternion.Euler(0f, 0f, 35f * Input.GetAxis("Horizontal") * -1f);

            // --- Reset button & drivable surface ---
            if (!raceStarted)
            {
                resetButton.SetActive(false);
                __instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
                AccessTools.Field(typeof(Car), "arcadeVehicleController").SetValue(__instance, LayerMask.GetMask(""));
            }
            else
            {
                AccessTools.Field(typeof(Car), "arcadeVehicleController").SetValue(__instance, LayerMask.GetMask("drivable"));
                resetButton.SetActive(true);
            }

            // --- Out-of-bounds check ---
            Vector3 pos = __instance.transform.position;
            if (pos.x < -2000f || pos.x > 2000f || pos.y < -10f || pos.y > 100f || pos.z < -2000f || pos.z > 2000f)
            {
                resetPositionMethod.Invoke(__instance, null);
            }
        }
    }

    [HarmonyPatch(typeof(Compass), "Update")]
    public class Patch_CompassUpdate
    {

        static bool Prefix(Compass __instance)
        {
            if (!customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity)
                return true;
            
            CustomUpdate(__instance);
            return false; // Skip original method
        }

        static void CustomUpdate(Compass __instance)
        {
            // --- Access private fields ---
            var pv = AccessTools.Field(typeof(Compass), "pv").GetValue(__instance) as PhotonView;
            var compass = AccessTools.Field(typeof(Compass), "compass").GetValue(__instance) as GameObject;
            var compassBG = AccessTools.Field(typeof(Compass), "compassBG").GetValue(__instance) as GameObject;
            var arrow = AccessTools.Field(typeof(Compass), "arrow").GetValue(__instance) as Transform;
            var compassImage = AccessTools.Field(typeof(Compass), "compassImage").GetValue(__instance) as RawImage;
            var questMarkers = AccessTools.Field(typeof(Compass), "questMarkers").GetValue(__instance) as List<QuestMarker>;
            var maxDistance = (float)AccessTools.Field(typeof(Compass), "maxDistance").GetValue(__instance);

            // --- Access private methods ---
            var gameStartMethod = AccessTools.Method(typeof(Compass), "GameStart");

            if (customTrackLoader.customTracks[customTrackLoader.currentTrackId].isCity)
            {
                if (!compass.activeSelf && pv.IsMine)
                {
                    compass.SetActive(true);
                    compassBG.SetActive(true);
                    arrow.gameObject.SetActive(true);
                    gameStartMethod.Invoke(__instance, null);
                }

                if (!pv.IsMine)
                    return;

                // Update compass rotation
                compassImage.uvRect = new Rect(__instance.transform.localEulerAngles.y / 360f, 0f, 1f, 1f);

                float nearestDistance = float.PositiveInfinity;

                foreach (QuestMarker questMarker in questMarkers)
                {
                    // Set marker position on compass
                    questMarker.image.rectTransform.anchoredPosition = 
                        AccessTools.Method(typeof(Compass), "GetPosOnCompass").Invoke(__instance, new object[] { questMarker }) as Vector2? ?? Vector2.zero;

                    // Distance to player
                    Vector2 playerPos = new Vector2(__instance.transform.position.x, __instance.transform.position.z);
                    float distance = Vector2.Distance(playerPos, questMarker.position);

                    float scale = 0f;
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        arrow.LookAt(questMarker.transform.position);
                    }

                    if (distance < maxDistance)
                    {
                        scale = 1f - distance / maxDistance;
                    }

                    questMarker.image.rectTransform.localScale = Vector3.one * scale;
                }

                return; // Skip hiding compass when in city
            }

            // Hide compass if not in city
            arrow.gameObject.SetActive(false);
            compass.SetActive(false);
            compassBG.SetActive(false);
        }
    }
}