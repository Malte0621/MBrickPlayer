using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ResourceDownloader : MonoBehaviour
{
    public static ResourceDownloader instance;

    public DownloadCompleteEvent downloadCompleteEvent = new DownloadCompleteEvent();
    public DownloadBytesCompleteEvent downloadBytesCompleteEvent = new DownloadBytesCompleteEvent();
    public GetTextCompleteEvent getTextCompleteEvent = new GetTextCompleteEvent();

    public string CacheFolder = "";
    private string AssetDLApi = "https://brkcdn.com/v2/assets/{0}";

    private void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }

        if (Application.platform == RuntimePlatform.Android) {
            CacheFolder = Application.persistentDataPath + "/cache/";
        } else {
            CacheFolder = Application.dataPath + "/cache/";
        }
        if (!Directory.Exists(CacheFolder)) Directory.CreateDirectory(CacheFolder); // create cache folder if it doesnt exist
    }

    public void GetTextFromURL (string key, string url) {
       Action<string> gotText = (text) => {
           getTextCompleteEvent.Invoke(key, text);
       };
       StartCoroutine(GetText(url, gotText));
    }

    public void DownloadAssetToCache (string key, string id, AssetType type) {
        string _type = type == AssetType.png ? "png" : "obj";
        string apiURL = String.Format(AssetDLApi, id);
        string filename = $"{id}.{_type}";

        Action<string, byte[]> downloadComplete = (name, data) => {
           File.WriteAllBytes(CacheFolder+filename, data);
           downloadCompleteEvent.Invoke(key, CacheFolder+filename);
        };

        StartCoroutine(DownloadAsset(apiURL, $"{id}.{_type}", downloadComplete));
    }

    public void DownloadAssetToMemory(string key, string id, AssetType type) {
        string _type = type == AssetType.png ? "png" : "obj";
        string apiURL = String.Format(AssetDLApi, id, _type);

        Action<string, byte[]> downloadComplete = (name, data) => {
            downloadBytesCompleteEvent.Invoke(key, data, name);
        };

        StartCoroutine(DownloadAsset(apiURL, $"{id}.{_type}", downloadComplete));
    }

    IEnumerator DownloadAsset (string url, string name, Action<string, byte[]> callback) {
        UnityWebRequest www = new UnityWebRequest(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error + " - " + url);
        } else {
            callback(name, www.downloadHandler.data);
        }
    }

    public IEnumerator GetText (string url, Action<string> callback, string method = "GET") {
        UnityWebRequest www = new UnityWebRequest(url);
        www.method = method;
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            if (callback != null)
            {
                callback(www.downloadHandler.text);
            }
        }
    }

    public enum AssetType {
        png, obj
    }
}

public class DownloadCompleteEvent : UnityEvent<string, string> {}
public class DownloadBytesCompleteEvent : UnityEvent<string, byte[], string> { }
public class GetTextCompleteEvent : UnityEvent<string, string> {}
