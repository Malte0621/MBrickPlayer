using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsElement : MonoBehaviour
{
    public SettingReturn returnSetting;

    public Slider slider;
    public Toggle toggle;
    public TMP_Text valueLabel;

    public void SetLabel (string value) {
        valueLabel.SetText(value);
    }

    public void SetSlider (float value, bool setValueLabel = true) {
        slider.SetValueWithoutNotify(value);
        if (setValueLabel) valueLabel.SetText(value.ToString());
    }

    public float GetSlider () {
        return slider.value;
    }

    public int GetSliderInt () {
        return (int)slider.value;
    }

    public void SetToggle (bool value) {
        toggle.SetIsOnWithoutNotify(value);
    }

    public bool GetToggle () {
        return toggle.isOn;
    }

    // magical automatic value setter function thingy
    public void SetValue (object value, bool setValueLabel = true) {
        if (value is float) {
            // slider
            SetSlider((float)value, setValueLabel);
        } else if (value is int) {
            // slider
            SetSlider((int)value, setValueLabel);
        }else if (value is bool) {
            // toggle
            SetToggle((bool)value);
        }
    }

    public object GetValue () {
        switch (returnSetting) {
            case (SettingReturn.Float): return GetSlider();
            case (SettingReturn.Integer): return GetSliderInt();
            case (SettingReturn.Boolean): return GetToggle();
        }
        return null;
    }

    public enum SettingReturn {
        Float, Integer, Boolean
    }
}
