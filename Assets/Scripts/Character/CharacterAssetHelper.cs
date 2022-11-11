using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterAssetHelper : MonoBehaviour
{
    public static CharacterAssetHelper instance;

    Dictionary<string, Action<AvatarRoot>> OngoingAvatarInfoDownloads = new Dictionary<string, Action<AvatarRoot>>();
    Dictionary<string, Action<Texture>> OngoingTextureDownloads = new Dictionary<string, Action<Texture>>();
    Dictionary<string, Action<Mesh>> OngoingMeshDownloads = new Dictionary<string, Action<Mesh>>();

    private void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }
    }

    private void Start () {
        ResourceDownloader.instance.downloadCompleteEvent.AddListener(DownloadCompleteEvent);
        ResourceDownloader.instance.downloadBytesCompleteEvent.AddListener(DownloadBytesCompleteEvent);
        ResourceDownloader.instance.getTextCompleteEvent.AddListener(GetTextEvent);
    }

    // main functions

    public void GetAvatarInfo (int id, Action<AvatarRoot> callback) {
        if (!OngoingAvatarInfoDownloads.ContainsKey("gai"+id)) {
            ResourceDownloader.instance.GetTextFromURL("gai"+id, "https://api.brick-hill.com/v1/games/retrieveAvatar?id=" + id);
            OngoingAvatarInfoDownloads.Add("gai"+id, callback);
        }
    }

    public void GetTexture (string id, Action<Texture> callback, bool cacheDL = true) {
        if (CustomResourceManager.TextureOverrides.TryGetValue(id, out Texture texOverride)) {
            // texture is overrided by resourcepack
            callback(texOverride);
            return;
        }

        if (File.Exists(ResourceDownloader.instance.CacheFolder + id + ".png")) {
            // load file locally
            Texture2D tex = new Texture2D(64, 64);
            tex.name = id.ToString();
            tex.LoadImage(File.ReadAllBytes(ResourceDownloader.instance.CacheFolder + id + ".png"));
            callback(tex);
        } else {
            // download file
            if (cacheDL) {
                if (!OngoingTextureDownloads.ContainsKey("gt"+id)) {
                    ResourceDownloader.instance.DownloadAssetToCache("gt" + id, id, ResourceDownloader.AssetType.png);
                    OngoingTextureDownloads.Add("gt" + id, callback);
                }
            } else {
                if (!OngoingTextureDownloads.ContainsKey("gt_m"+id)) {
                    ResourceDownloader.instance.DownloadAssetToMemory("gt_m" + id, id, ResourceDownloader.AssetType.png);
                    OngoingTextureDownloads.Add("gt_m" + id, callback);
                }
            }
        }
    }

    public void GetMesh (string id, Action<Mesh> callback) {
        if (CustomResourceManager.MeshOverrides.TryGetValue(id, out Mesh meshOverride)) {
            // mesh is overrided by resourcepack
            callback(meshOverride);
            return;
        }

        if (File.Exists(ResourceDownloader.instance.CacheFolder + id + ".obj")) {
            // load file locally
            Mesh mesh = FastObjImporter.Instance.ImportFile(ResourceDownloader.instance.CacheFolder + id + ".obj");
            mesh.name = id.ToString();
            callback(mesh);
        } else {
            // download file
            if (!OngoingMeshDownloads.ContainsKey("gm"+id)) {
                ResourceDownloader.instance.DownloadAssetToCache("gm"+id, id, ResourceDownloader.AssetType.obj);
                OngoingMeshDownloads.Add("gm"+id, callback);
            }
        }        
    }

    // event listeners

    void GetTextEvent (string key, string text) {
        if (key.StartsWith("gai")) { // Avatar Info
            if (OngoingAvatarInfoDownloads.TryGetValue(key, out Action<AvatarRoot> callback)) {
                callback(JsonUtility.FromJson<AvatarRoot>(text));
                OngoingAvatarInfoDownloads.Remove(key);
            }
        }
    }

    void DownloadCompleteEvent (string key, string path) {
        if (key.StartsWith("gt")) { // Get Texture
            if (OngoingTextureDownloads.TryGetValue(key, out Action<Texture> callback)) {
                // load texture
                Texture2D tex = new Texture2D(64,64);
                tex.name = Path.GetFileNameWithoutExtension(path);
                tex.LoadImage(File.ReadAllBytes(path));
                callback(tex);
                OngoingTextureDownloads.Remove(key);
            }
        } else if (key.StartsWith("gm")) { // Get Mesh
            if (OngoingMeshDownloads.TryGetValue(key, out Action<Mesh> callback)) {
                Mesh mesh = FastObjImporter.Instance.ImportFile(path);
                mesh.name = Path.GetFileNameWithoutExtension(path);
                callback(mesh);
                OngoingMeshDownloads.Remove(key);
            }
        }
    }

    void DownloadBytesCompleteEvent (string key, byte[] data, string name) {
        if (key.StartsWith("gt_m")) { // Get Texture
            if (OngoingTextureDownloads.TryGetValue(key, out Action<Texture> callback)) {
                // load texture
                Texture2D tex = new Texture2D(64, 64);
                tex.name = Path.GetFileNameWithoutExtension(name);
                tex.LoadImage(data);
                callback(tex);
                OngoingTextureDownloads.Remove(key);
            }
        }
    }
}

[Serializable]
public class AvatarItems {
    public string face;
    public string[] hats;
    public int head;
    public int tool;
    public int pants;
    public string shirt;
    public string figure;
    public string tshirt;
}

[Serializable]
public class AvatarColors {
    public string head;
    public string torso;
    public string left_arm;
    public string right_arm;
    public string left_leg;
    public string right_leg;
}

[Serializable]
public class AvatarRoot {
    public int user_id;
    public AvatarItems items;
    public AvatarColors colors;
}