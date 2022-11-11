using System;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static Settings PlayerSettings;
    private static string filePath;

    private void Awake() {
        if (Application.platform == RuntimePlatform.Android) {
            filePath = Application.persistentDataPath + "/settings.json";
        } else {
            filePath = Application.dataPath + "/settings.json";
        }
        LoadSettings();
    }

    public static void LoadSettings () {
        if (File.Exists(filePath)) {
            // load settings
            string data = File.ReadAllText(filePath);
            PlayerSettings = JsonUtility.FromJson<Settings>(data);
            Debug.Log("Loaded Settings");
        } else {
            // create settings
            PlayerSettings = new Settings();

            // default settings
            PlayerSettings.MouseSensitivity = 100;
            PlayerSettings.ZoomSensitivity = 100;
            PlayerSettings.InvertZoom = false;
            PlayerSettings.KeepMovementInChat = true;
            PlayerSettings.GlobalVolume = 1.0f;
            PlayerSettings.TextureQuality = 1; // full
            PlayerSettings.Shadows = 2; // soft
            PlayerSettings.ShadowQuality = 1; // medium
            PlayerSettings.Antialiasing = 0; // off
            PlayerSettings.Framelimiter = 60;
            PlayerSettings.DrawDistance = 1000;
            PlayerSettings.UIBlur = true;
            PlayerSettings.ShowFPS = false;

            // save to file
            SaveSettings();
            Debug.Log("Created & Loaded Settings");
        }
    }

    public static void SaveSettings () {
        string data = JsonUtility.ToJson(PlayerSettings);
        File.WriteAllText(filePath, data);
        Debug.Log("Saved Settings");
    }

    public static object GetSettingFromIndex (int index) {
        switch (index) {
            case 0: return PlayerSettings.MouseSensitivity;
            case 1: return PlayerSettings.ZoomSensitivity;
            case 2: return PlayerSettings.InvertZoom;
            case 3: return PlayerSettings.KeepMovementInChat;
            case 4: return PlayerSettings.GlobalVolume;
            case 5: return PlayerSettings.TextureQuality;
            case 6: return PlayerSettings.Shadows;
            case 7: return PlayerSettings.ShadowQuality;
            case 8: return PlayerSettings.Antialiasing;
            case 9: return PlayerSettings.DrawDistance;
            case 10: return PlayerSettings.Framelimiter;
            case 11: return PlayerSettings.UIBlur;
            case 12: return PlayerSettings.ShowFPS;
        }
        return null;
    }

    public static void SetSettingFromIndex (int index, object value) {
        switch (index) {
            case 0: PlayerSettings.MouseSensitivity = (int)value; break;
            case 1: PlayerSettings.ZoomSensitivity = (int)value; break;
            case 2: PlayerSettings.InvertZoom = (bool)value; break;
            case 3: PlayerSettings.KeepMovementInChat = (bool)value; break;
            case 4: PlayerSettings.GlobalVolume = (float)value; break;
            case 5: PlayerSettings.TextureQuality = (int)value; break;
            case 6: PlayerSettings.Shadows = (int)value; break;
            case 7: PlayerSettings.ShadowQuality = (int)value; break;
            case 8: PlayerSettings.Antialiasing = (int)value; break;
            case 9: PlayerSettings.DrawDistance = (int)value; break;
            case 10: PlayerSettings.Framelimiter = (int)value; break;
            case 11: PlayerSettings.UIBlur = (bool)value; break;
            case 12: PlayerSettings.ShowFPS = (bool)value; break;
        }
    }
}

[Serializable]
public struct Settings {
    public int MouseSensitivity;
    public int ZoomSensitivity;
    public bool InvertZoom;
    public bool KeepMovementInChat;
    public float GlobalVolume;
    public int TextureQuality;
    public int Shadows;
    public int ShadowQuality;
    public int Antialiasing;
    public int DrawDistance;
    public int Framelimiter;
    public bool UIBlur;
    public bool ShowFPS;
}
