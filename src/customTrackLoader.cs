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

namespace DCDMapLoader
{

    public class InfoData
    {
        public string Name;
        public string Desc;
        public string Author;
    }

    public class customTrack
    {
        public string name;
        public Texture2D icon;
        public string mapPath;
        public string desc;

        public customTrack(string name, string mapPath, string desc, Texture2D icon = null)
        {
            this.name = name;
            this.mapPath = mapPath;
            this.icon = icon;
            this.desc = desc;
        }
    }

    public class customTrackLoader
    {
        public static List<customTrack> customTracks = new List<customTrack>();

        public static void LoadRace(int MapID) {
            if (PhotonNetwork.InRoom) {
                MelonLogger.Msg("Loading " + customTracks[MapID].name + "!");
                PhotonNetwork.LoadLevel(System.IO.Path.GetFileNameWithoutExtension(customTracks[MapID].mapPath));
            }
        }

        public static void ToLastCheckPoint(GameObject other)
        {
            if (!other.GetComponent<Car>().GetComponent<PhotonView>().IsMine) return; // Only affect local player

            GameObject checkpointManager = GameObject.Find("CheckpointManager");

            int currectCheckpoint = 0;
            if (other.GetComponent<Car>().furthestCheckpointReached == 0) {
                currectCheckpoint = checkpointManager.GetComponent<CheckpointManager>().checkpoints.Length-1;
            } else {
                currectCheckpoint = other.GetComponent<Car>().furthestCheckpointReached-1;
            }

            Vector3 offset = new Vector3(0f, 20f, 0f);
            Rigidbody[] componentsInChildren = other.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigidbody in componentsInChildren)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = true;
                if (SceneManager.GetActiveScene().buildIndex == 3)
                {
                    rigidbody.position = checkpointManager.GetComponent<CheckpointManager>().checkpoints[currectCheckpoint].GetComponent<Transform>().position + offset;
                }
                else if (SceneManager.GetActiveScene().buildIndex > 3)
                {
                    rigidbody.position = checkpointManager.GetComponent<CheckpointManager>().checkpoints[currectCheckpoint].GetComponent<Transform>().position + offset;
                }
                rigidbody.isKinematic = false;
            }

            other.GetComponent<Transform>().position = checkpointManager.GetComponent<CheckpointManager>().checkpoints[currectCheckpoint].GetComponent<Transform>().position + offset;
        
        }

        public static void InitCustomMaps() {
            string mapsPath = AppDomain.CurrentDomain.BaseDirectory + "/Maps/";

            string[] mapFolders = Directory.GetDirectories(mapsPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string mapPath in mapFolders) {
                string[] files = Directory.GetFiles(mapPath, "*", SearchOption.TopDirectoryOnly);
                
                Texture2D icon = new Texture2D(2, 2);
                AssetBundle bundle = null;
                InfoData info = null;
                
                foreach (string file in files) {
                    bool isBundle = Path.GetExtension(file) == "";
                    if (isBundle) {
                        bundle = AssetBundle.LoadFromFile(file);
                    } else if (Path.GetExtension(file) == ".json") {
                        info = JsonUtility.FromJson<InfoData>(File.ReadAllText(file));
                    } else {
                        UnityEngine.ImageConversion.LoadImage(icon, File.ReadAllBytes(file));
                    }
                }

                if (bundle == null)
                {
                    MelonLogger.Msg("Failed to load AssetBundle!");
                    continue;
                }

                if (bundle.GetAllScenePaths().Length == 0)
                {
                    MelonLogger.Msg("No scenes found in AssetBundle!");
                    continue;
                } else {
                    string[] scenes = bundle.GetAllScenePaths();
                    foreach (string trackPath in scenes)
                    {
                        
                        customTrack track = new customTrack(System.IO.Path.GetFileNameWithoutExtension(trackPath), trackPath, "No Desc", icon);
                        if (info != null) {
                            track = new customTrack(info.Name+" By: "+info.Author, trackPath, info.Desc, icon);
                        }
                        customTracks.Add(track);
                    }
                }
            }

            if (customTracks.Count <= 0) {
                MelonLogger.Msg("No Custom Maps Found!");
            } else {
                MelonLogger.Msg("Found Custom Maps: " + string.Join(", ", customTracks.Select(track => track.name)));
            }
        }

        public static void initCustomMapObjects(int buildIndex, string sceneName)
        {
            customTrack customTrack = customTracks.FirstOrDefault(ct => System.IO.Path.GetFileNameWithoutExtension(ct.mapPath) == sceneName);
            if (customTrack != null) {
                if (PhotonNetwork.InRoom) {
                    GameObject[] allObjectsInCustomMap = GameObject.FindObjectsOfType<GameObject>();

                    MelonLogger.Msg("Initializing Spawns and Triggers!");
                    List<Transform> positions = new List<Transform>();

                    foreach (GameObject obj in allObjectsInCustomMap)
                    {
                        if (obj.name == "Spawn" || obj.name == "spawn")
                        {
                            positions.Add(obj.transform);
                        }
                        if (obj.name == "ToLastCheckPoint" || obj.name == "toLastCheckPoint")
                        {
                            Collider col = obj.GetComponent<Collider>();
                            if (col != null)
                            {
                                TriggerHandler handler = obj.AddComponent<TriggerHandler>();
                                handler.OnTriggered += ToLastCheckPoint;
                            }
                        }
                    }

                    if (positions.Count == 0)
                    {
                        MelonLogger.Msg("INVALID MAP NO SPAWNS FOUND!");
                        MelonLogger.Msg("SPAWN SET TO 0,5,0 THIS MIGHT BREAK THINGS!");

                        GameObject emptyGO = new GameObject();
                        Transform transform = emptyGO.transform;
                        transform.position = new Vector3(0f, 5f, 0f);
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++) 
                        {
                            positions.Add(transform);
                        }
                    }

                    Transform[] spawnPositions = positions.ToArray();

                    MelonLogger.Msg("Initializing Player!");
                    int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                    //PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), spawnPositions[index].position, spawnPositions[index].rotation, 0);

                    Vector3 spawnOffsetFix = new Vector3(-13.55f, 0f, 9.5f);

                    if (buildIndex > 3)
                    {
                        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPositions[index].position + spawnOffsetFix, spawnPositions[index].rotation, 0);
                        return;
                    }
                    GameObject gameObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPositions[index].position + spawnOffsetFix, spawnPositions[index].rotation, 0);
                    Garage[] array = UnityEngine.Object.FindObjectsOfType<Garage>();
                    for (int num = 0; num < array.Length; num++)
                    {
                        array[num].localPlayer = gameObject.GetComponent<Car>();
                    }
                }
            }
        }
    }
}