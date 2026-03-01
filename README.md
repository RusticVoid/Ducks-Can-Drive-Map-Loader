# Ducks Can Drive Map Loader

A Map Loader mod for **Ducks Can Drive**.

---

## 🚧 TODO
- [ ] Add the ability to use custom cities

---

# 📦 How To Install [VIDEO](https://www.youtube.com/watch?v=qvViDuwhJwk)

## 1️⃣ Install MelonLoader

You must install **MelonLoader** for Ducks Can Drive.

Download it here:  
https://melonwiki.xyz/#/?id=requirements  

If you have trouble installing MelonLoader, try this installation video.

Installation video:  
https://youtu.be/AMMvtprzh4I?si=qiHABkxAePVoAB9I

---

## 2️⃣ Install Ducks Can Drive Map Loader

Download the latest release here:  
https://github.com/RusticVoid/Ducks-Can-Drive-Map-Loader/releases/latest

1. Download the latest `.dll` file.
2. Place `DCDMapLoader.dll` inside the **Mods** folder in your Ducks Can Drive game directory.
3. If there is no `Mods` folder, create one named "Mods"

The Map Loader is now installed.

---

# 🗺️ How To Install Custom Maps

After running the game once, a `Maps` folder should appear in the game directory.

If it does not exist, create a folder named "Maps".

You can download example maps here:  
https://github.com/RusticVoid/Ducks-Can-Drive-Map-Loader/tree/main/Maps

---

## Example: Installing "Template Road"

1. Download `TemplateRoad.zip`
2. Extract it into the `Maps` folder
3. Delete the zip file after extracting

Your folder structure should look like this:

```
Ducks Can Drive/
├── Maps/
│   └── Template Road/
│       ├── icon.png
│       ├── info.json
│       └── templateroad
```

---

# 🛠️ How To Make a Custom Map [VIDEO](https://www.youtube.com/watch?v=PTQbMUplUzc)

## Step 1 — Install Unity Hub

Download Unity Hub here:  
https://docs.unity3d.com/hub/manual/InstallHub.html

---

## Step 2 — Download the Template Project

Download the template here:  
https://github.com/RusticVoid/Ducks-Can-Drive-Map-Loader/tree/main/UnityTemplates

Extract the zip wherever you want.

For example:

```
C:\Users\YourUsername\Desktop\MyDCDMap\
```

---

## Step 3 — Add the Project to Unity Hub

1. Open Unity Hub
2. Click **Add**
3. Click **Add project from disk**
4. Select your extracted `MyDCDMap` folder

When prompted, install Unity version:

```
2022.3.8f1
```

Click **Install** and wait for Unity to finish installing.

---

## Step 4 — Open the Scene

After opening the project:

- You should automatically load into the `Template_Road` scene.
- If not, open the **Scenes** folder (bottom-left panel in Unity).
- Double-click `Template_Road`.

---

🎉 You are now ready to create your custom map.

---

## 📂 Folder Structure (For Map Creators)

Each custom map folder should look like this:

```
MapName/
├── icon.png
├── info.json
└── assetbundle file
```

info.json should look like this:

```
{
    "Name": "TRACK NAME",
    "Desc": "TRACK DESCRIPTION",
    "Author": "YOUR NAME"
}
```

