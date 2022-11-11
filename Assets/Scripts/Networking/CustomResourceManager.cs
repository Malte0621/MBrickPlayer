using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class CustomResourceManager : MonoBehaviour
{
    public static CustomResourceManager instance;

    public MapBuilder mapBuilder;
    private static MapBuilder mb;

    private static string ResourcePath;
    public static string ServerSubfolder;

    private static string AssetDirectory = "assets/";
    private static string BrickDirectory = "brick/";
    private static string SkyDirectory = "sky/";
    private static string UIDirectory = "ui/";
    private static string AudioDirectory = "audio/";

    public static Dictionary<string, Texture> TextureOverrides = new Dictionary<string, Texture>();
    public static Dictionary<string, Mesh> MeshOverrides = new Dictionary<string, Mesh>();

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(instance);
        }

        if (Application.platform == RuntimePlatform.Android) {
            ResourcePath = Application.persistentDataPath + "/CustomResources/";
        } else {
            ResourcePath = Application.dataPath + "/CustomResources/";
        }

        mb = mapBuilder;
    }

    public static void PrepareResourceDirectory (string subfolder, bool clear = false) {
        // check if it needs to be created
        if (!Directory.Exists(ResourcePath)) {
            Directory.CreateDirectory(ResourcePath);
        }

        if (!Directory.Exists(ResourcePath + subfolder)) {
            Directory.CreateDirectory(ResourcePath + subfolder);
        }

        if (clear) {
            string[] contents = Directory.GetFileSystemEntries(ResourcePath + subfolder);
            for (int i = 0; i < contents.Length; i++) {
                FileAttributes fa = File.GetAttributes(contents[i]);
                if ((fa & FileAttributes.Directory) == FileAttributes.Directory) {
                    Directory.Delete(contents[i], true);
                } else {
                    File.Delete(contents[i]);
                }
            }
        }
    }

    public static bool CheckPackChanges (string version) {
        if (File.Exists(ResourcePath + ServerSubfolder + "version")) {
            string contents = File.ReadAllText(ResourcePath + ServerSubfolder + "version");
            return contents != version;
        }
        return true;
    }

    public static void DownloadResourcePack (NetworkManager nm, PacketReader r) {
        nm.SetPacketHandlerState(false); // pause packet handling while resourcepack downloads
        nm.main.ui.SetConnectingStatus(true, "Downloading server resource pack...");

        string gameID = nm.GameID.ToString();
        ServerSubfolder = gameID + "/";

        string url = r.ReadString(); // url to download pack
        string ver = r.ReadString(); // pack version to check pack version

        bool redl = CheckPackChanges(ver);

        if (redl) {
            // gotta dl pack
            PrepareResourceDirectory(gameID, true);

            if (File.Exists(ResourcePath + "temp")) {
                File.Delete(ResourcePath + "temp");
            }

            Action<byte[]> downloadComplete = (data) => {
                File.WriteAllBytes(ResourcePath + "temp", data);
                Debug.Log("Downloaded server resourcepack!");
                ExtractArchive(ResourcePath + "temp", ResourcePath + ServerSubfolder, true);
                File.WriteAllText(ResourcePath + ServerSubfolder + "version", ver);
                CheckForResources();
                nm.main.ui.SetConnectingStatus(false);
                nm.SetPacketHandlerState(true);
            };

            instance.StartCoroutine(downloadFile(url, downloadComplete));
        } else {
            // gotta not dl pack
            CheckForResources();
            nm.main.ui.SetConnectingStatus(false);
            nm.SetPacketHandlerState(true);
        }
    }

    private static void ExtractArchive (string file, string path, bool deleteAfter = false) {
        ZipFile.ExtractToDirectory(file, path);
        if (deleteAfter) File.Delete(file);
    }

    public static void CheckForResources () {
        // assets
        if (Directory.Exists(ResourcePath + ServerSubfolder + AssetDirectory)) {
            string[] files = Directory.GetFiles(ResourcePath + ServerSubfolder + AssetDirectory);
            for (int i = 0; i < files.Length; i++) {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                string ext = Path.GetExtension(files[i]);

                if (ext == ".png") {
                    if (TextureOverrides.ContainsKey(name)) continue;

                    Texture2D tex = new Texture2D(64, 64);
                    tex.name = name;
                    tex.LoadImage(File.ReadAllBytes(files[i]));

                    TextureOverrides.Add(name, tex);
                } else if (ext == ".obj") {
                    if (MeshOverrides.ContainsKey(name)) continue;

                    Mesh mesh = FastObjImporter.Instance.ImportFile(files[i]);
                    mesh.name = name;

                    MeshOverrides.Add(name, mesh);
                }
            }
            Debug.Log("added " + (TextureOverrides.Count + MeshOverrides.Count) + " asset overrides");
        }

        // brick textures
        if (Directory.Exists(ResourcePath + ServerSubfolder + BrickDirectory)) {
            string[] files = Directory.GetFiles(ResourcePath + ServerSubfolder + BrickDirectory);
            for (int i = 0; i < files.Length; i++) {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                string ext = Path.GetExtension(files[i]);

                if (ext == ".png") {
                    bool normal = name.EndsWith("_n");
                    Texture2D tex = new Texture2D(64, 64, TextureFormat.R8, true, normal);
                    tex.name = name;
                    tex.LoadImage(File.ReadAllBytes(files[i]));

                    switch (name) {
                        case "stud":
                            //MaterialCache.instance.Stud = tex;
                            mb.StudMat.SetTexture("_MainTex", tex);
                            mb.StudMatAlpha.SetTexture("_MainTex", tex);
                            break;
                        case "stud_n":
                            //MaterialCache.instance.Stud = tex;
                            mb.StudMat.SetTexture("_NormalTex", tex);
                            mb.StudMatAlpha.SetTexture("_NormalTex", tex);
                            break;
                        case "inlet":
                            //MaterialCache.instance.Inlet = tex;
                            mb.InletMat.SetTexture("_MainTex", tex);
                            mb.InletMatAlpha.SetTexture("_MainTex", tex);
                            break;
                        case "inlet_n":
                            //MaterialCache.instance.Inlet = tex;
                            mb.InletMat.SetTexture("_NormalTex", tex);
                            mb.InletMatAlpha.SetTexture("_NormalTex", tex);
                            break;
                        case "spawnpoint":
                            //MaterialCache.instance.Spawnpoint = tex;
                            mb.SpawnpointMat.SetTexture("_MainTex", tex);
                            mb.SpawnpointMatAlpha.SetTexture("_MainTex", tex);
                            break;
                        case "spawnpoint_n":
                            //MaterialCache.instance.Spawnpoint = tex;
                            mb.SpawnpointMat.SetTexture("_NormalTex", tex);
                            mb.SpawnpointMatAlpha.SetTexture("_NormalTex", tex);
                            break;
                    }
                }

                if (name == "tiling" && ext == ".txt") {
                    string[] tileSettings = File.ReadAllLines(files[i]);
                    if (tileSettings.Length > 0) {
                        mb.StudTile = float.Parse(tileSettings[0], CultureInfo.InvariantCulture);
                    } //MaterialCache.instance.StudTile = float.Parse(tileSettings[0], CultureInfo.InvariantCulture);
                    
                    if (tileSettings.Length > 1) {
                        mb.InletTile = float.Parse(tileSettings[1], CultureInfo.InvariantCulture);
                    } //MaterialCache.instance.InletTile = float.Parse(tileSettings[1], CultureInfo.InvariantCulture);
                }
            }
        }

        // sky
        if (Directory.Exists(ResourcePath + ServerSubfolder + SkyDirectory)) {
            if (File.Exists(ResourcePath + ServerSubfolder + SkyDirectory + "sky.png")) {
                Texture2D sky = new Texture2D(1024, 768, TextureFormat.RGBA32, false);
                sky.name = "sky";
                sky.wrapMode = TextureWrapMode.Clamp;
                sky.LoadImage(File.ReadAllBytes(ResourcePath + ServerSubfolder + SkyDirectory + "sky.png"));
                PlayerMain.instance.map.SetSkybox(sky);
            }
        }

        // audio
        if (Directory.Exists(ResourcePath + ServerSubfolder + AudioDirectory)) {
            string[] files = Directory.GetFiles(ResourcePath + ServerSubfolder + AudioDirectory);
            for (int i = 0; i < files.Length; i++) {
                string name = Path.GetFileNameWithoutExtension(files[i]);
                string ext = Path.GetExtension(files[i]);

                // only load ogg files
                if (ext == ".ogg") {
                    Action<AudioClip> loadComplete = (clip) => {
                        clip.name = name;
                        AudioManager.AddClip(clip);
                    };

                    instance.StartCoroutine(LoadAudioFile("file:///" + files[i], loadComplete));
                }
            }
        }
    }

    private static IEnumerator LoadAudioFile (string url, Action<AudioClip> callback) {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS); // ogg format
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            callback(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    private static IEnumerator downloadFile (string url, Action<byte[]> callback) {
        UnityWebRequest www = new UnityWebRequest(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            callback(www.downloadHandler.data);
        }
    }
}
