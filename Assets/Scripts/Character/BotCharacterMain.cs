using BrickHill;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class BotCharacterMain : MonoBehaviour
{
    public Bot bot;

    public TMP_Text Nametag;
    public GameObject SpeechObj;

    public Material PartMaterial;
    //public Material TShirtMaterial;
    public Material HatMaterial;
    public GameObject Head;
    public GameObject Torso;
    //public GameObject TShirt;
    public GameObject LeftArm;
    public GameObject RightArm;
    public GameObject LeftLeg;
    public GameObject RightLeg;
    public Dictionary<string, GameObject> Hats = new Dictionary<string, GameObject>();

    private Texture faceTexture;

    public NetworkCharacterMovement movement;

    public Transform BillboardTarget;

    private void Update() {
        if (BillboardTarget != null) {
            Nametag.transform.rotation = BillboardTarget.rotation;
            SpeechObj.transform.rotation = BillboardTarget.rotation;
        }
    }

    public void UpdateBot () {
        Vector3 pos = bot.Position.SwapYZ();
        pos.x *= -1;
        movement.SetPosition(pos);
        movement.SetRotation(bot.Rotation.z * -1 + 180);

        SetSpeech(bot.Speech);

        UpdateColors();
    }

    public void SetNametag (string name, Color color) {
        Nametag.gameObject.SetActive(true);
        Nametag.SetText(Helper.FilteredText(name));
        Nametag.color = color;
    }

    public void SetSpeech(string text)
    {
        if (text != "")
        {
            foreach (TMP_Text SpeechText in SpeechObj.transform.GetComponentsInChildren<TMP_Text>())
            {
                SpeechText.transform.parent.gameObject.SetActive(true);
                SpeechText.SetText(Helper.FilteredText(text));
                SpeechText.color = Color.white;
            }
        }
        else
        {
            foreach (TMP_Text SpeechText in SpeechObj.transform.GetComponentsInChildren<TMP_Text>())
            {
                SpeechText.transform.parent.gameObject.SetActive(false);
                SpeechText.SetText("");
                SpeechText.color = Color.white;
            }
        }
    }

    public void UpdateColors () {
        Material headMat = new Material(PartMaterial);
        headMat.color = bot.Colors.Head.BGR();
        if (faceTexture != null) headMat.mainTexture = faceTexture;
        Head.GetComponent<MeshRenderer>().material = headMat;

        Material torsoMat = new Material(PartMaterial);
        torsoMat.color = bot.Colors.Torso.BGR();
        Torso.GetComponent<MeshRenderer>().material = torsoMat;

        Material leftArmMat = new Material(PartMaterial);
        leftArmMat.color = bot.Colors.LeftArm.BGR();
        LeftArm.GetComponent<MeshRenderer>().material = leftArmMat;

        Material rightArmMat = new Material(PartMaterial);
        rightArmMat.color = bot.Colors.RightArm.BGR();
        RightArm.GetComponent<MeshRenderer>().material = rightArmMat;

        Material leftLegMat = new Material(PartMaterial);
        leftLegMat.color = bot.Colors.LeftLeg.BGR();
        LeftLeg.GetComponent<MeshRenderer>().material = leftLegMat;

        Material rightLegMat = new Material(PartMaterial);
        rightLegMat.color = bot.Colors.RightLeg.BGR();
        RightLeg.GetComponent<MeshRenderer>().material = rightLegMat;
    }
}
