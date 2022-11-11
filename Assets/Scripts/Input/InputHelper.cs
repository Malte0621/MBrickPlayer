using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHelper : MonoBehaviour
{
    public static InputHelper instance;

    public static Controls controls;
    private static bool controlsEnabled; // whether the controls should be active - not necessarily whether they are active
    
    public static bool currentControlsState; // whether the controls ARE active, regardless of if they should be - makes sense?

    private static bool uiSelectLock = true;

    public static MouseState CurrentMouseState = MouseState.ThirdPerson;

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }

        controls = new Controls();
        EnableControls();
    }

    void Update () {
        if (EventSystem.current.currentSelectedGameObject != null) {
            // GO selected - inputfield?
            if (uiSelectLock) {
                uiSelectLock = false;
                DisableControls(true, false);
            }
        } else {
            if (!uiSelectLock && controlsEnabled) {
                uiSelectLock = true;
                EnableControls(true);
            }
        }
    }

    public static void SetControlsEnabled (bool value) {
        if (value) {
            EnableControls();
        } else {
            DisableControls(false, true);
        }
    }

    public static void EnableControls (bool dontSetEnabledBool = false) {
        if (uiSelectLock) {
            controls.Enable();
            currentControlsState = true;
        }
        if (!dontSetEnabledBool) controlsEnabled = true;
    }

    public static void DisableControls (bool dontSetEnabledBool = false, bool allowMenu = false) {
        controls.Disable();
        currentControlsState = false;
        if (!dontSetEnabledBool) controlsEnabled = false;
        if (allowMenu) controls.Player.ToggleMenu.Enable();
    }

    public static void SetMouseState (MouseState state) {
        CurrentMouseState = state;

        if (state == MouseState.FirstPerson) {
            Cursor.lockState = CursorLockMode.Locked;
        } else {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public enum MouseState {
        ThirdPerson, // Unlocked
        FirstPerson, // Locked
        Menu // Locked
    }
}
