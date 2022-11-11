using BrickHill;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utils;

public class CharacterMain : MonoBehaviour
{
    public PlayerMain main;
    public CharacterMovement characterMovement;
    public NetworkCharacterMovement networkCharacterMovement;
    public CharacterAnimator characterAnimator;
    public CharacterSounds characterSounds;

    public Player player;


    //public FigureData figureData;

    public TMP_Text Nametag;
    public GameObject SpeechObj;

    public Material PartMaterial;
    public Material TShirtMaterial;
    public Material HatMaterial;
    public Texture defaultFaceTexture;
    private GameObject[] Hats = new GameObject[3];
    
    public MeshRenderer Head;
    public MeshRenderer Torso;
    public MeshRenderer TShirt;
    public MeshRenderer LeftArm;
    public MeshRenderer RightArm;
    public GameObject ToolGO;
    public MeshRenderer LeftLeg;
    public MeshRenderer RightLeg;
    
    //private Dictionary<string, GameObject> HatsDict = new Dictionary<string, GameObject>();

    private Texture faceTexture;
    private Texture shirtTexture;
    private Texture pantsTexture;
    private Texture tshirtTexture;

    public Transform BillboardTarget;

    private void Update () {
        /*
        foreach(GameObject gobj in GameObject.FindGameObjectsWithTag("FacePlayer"))
        {
            gobj.transform.LookAt(transform.position);
        }
        */
        if (BillboardTarget != null) {
            Nametag.transform.rotation = BillboardTarget.rotation;
            SpeechObj.transform.rotation = BillboardTarget.rotation;
        }
    }

    public void UpdateFromFigure (bool forceEssential = false) {
        // set pos
        if (player.PlayerFigure._updateValues[0] || player.PlayerFigure._updateValues[1] || player.PlayerFigure._updateValues[2]) {
            Vector3 pos = player.PlayerFigure.Position;
            pos.x *= -1f;
            //transform.position = pos.SwapYZ();
            if (characterMovement != null) characterMovement.SetPosition(pos.SwapYZ());
            if (networkCharacterMovement != null) networkCharacterMovement.SetPosition(pos.SwapYZ());
        }

        // set rot
        if (player.PlayerFigure._updateValues[3] || player.PlayerFigure._updateValues[4] || player.PlayerFigure._updateValues[5]) {
            if (networkCharacterMovement != null) {
                networkCharacterMovement.SetRotation(player.PlayerFigure.Rotation.z * -1 + 180);
            } else {
                transform.eulerAngles = new Vector3(0, player.PlayerFigure.Rotation.z * -1 + 180, 0);
            }
        }

        // set colors
        if (player.PlayerFigure._updateValues[10] || player.PlayerFigure._updateValues[11] || player.PlayerFigure._updateValues[12] || player.PlayerFigure._updateValues[13] || player.PlayerFigure._updateValues[14] || player.PlayerFigure._updateValues[15] || forceEssential) {
            SetBodyColors(player.PlayerFigure.Colors, true);
        }

        // face
        if (player.PlayerFigure._updateValues[16] || forceEssential) SetFace(player.PlayerFigure.FaceID);
        
        // Shirt/Pants/Tshirt
        if (player.PlayerFigure._updateValues[17] || forceEssential) SetShirt(player.PlayerFigure.Shirt);
        if (player.PlayerFigure._updateValues[18] || forceEssential) SetPants(player.PlayerFigure.Pants);
        if (player.PlayerFigure._updateValues[19] || forceEssential) SetTShirt(player.PlayerFigure.TShirt);

        // hats
        for (int i = 0; i < 3; i++) {
            if (player.PlayerFigure._updateValues[20+i] || forceEssential) {
                SetHat(i, player.PlayerFigure.HatID[i], player.PlayerFigure.HatIDTex[i]);
            }
        }
        
        // speed
        if ((player.PlayerFigure._updateValues[25] || player.PlayerFigure._updateValues[26] || forceEssential) && characterMovement != null) {
            characterMovement.PlayerSpeed = player.PlayerFigure.Speed;
            characterSounds.StepRateMultiplier = characterMovement.PlayerSpeed / 4f;
            characterAnimator.SetFloat("RunSpeed", characterMovement.PlayerSpeed / 4f);
            characterMovement.JumpPower = player.PlayerFigure.JumpPower;
        }

        // Speech
        if (player.PlayerFigure._updateValues[39] || forceEssential)
        {
            SetSpeech(player.PlayerFigure.Speech);
        }

        // tool
        if (player.PlayerFigure._updateValues[40] || player.PlayerFigure._updateValues[41]  || player.PlayerFigure._updateValues[42] || forceEssential) {
            if (player.PlayerFigure.Unequip) {
                characterAnimator.SetBool("Holding", false);
                ToolGO.SetActive(false);

                if (player.NetID == main.localInfo.LocalPlayer.NetID) main.ui.DeselectTools();
            } else {
                if (player.PlayerFigure.EquippedToolSlotID != 0) {
                    characterAnimator.SetBool("Holding", true);
                    SetTool(player.PlayerFigure.EquippedToolModel, player.PlayerFigure.EquippedToolModelTex);
                    ToolGO.SetActive(true);

                    if (player.NetID == main.localInfo.LocalPlayer.NetID) {
                        main.ui.DeselectTools();
                        main.ui.SelectTool(player.PlayerFigure.EquippedToolSlotID);
                    }
                }
            }
        }

        Array.Clear(player.PlayerFigure._updateValues, 0, player.PlayerFigure._updateValues.Length); // clear updated value array
    }

    public void UpdateFigure () {
        Vector3 pos = transform.position.SwapYZ();
        pos.x *= -1;
        player.PlayerFigure.Position = pos;

        player.PlayerFigure.Rotation.z = (int)transform.eulerAngles.y * -1 - 180;
    }

    public void SetNametag (string name, Color color) {
        Nametag.gameObject.SetActive(true);
        Nametag.SetText(Helper.FilteredText(name));
        Nametag.color = color;
    }
    
    public void SetSpeech(string text) {
        if (text != "")
        {
            foreach (TMP_Text SpeechText in SpeechObj.transform.GetComponentsInChildren<TMP_Text>()){
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

    public void UpdateNametag () {
        Nametag.SetText(player.Name);
        Nametag.color = main.Teams[player.PlayerFigure.TeamNetID].TeamColor;
    }

    public void SetBodyColors (Figure.BodyColors colors, bool BGR = false) {
        Material headMat = new Material(PartMaterial);
        headMat.color = BGR ? colors.Head.BGR() : colors.Head;
        if (faceTexture != null) headMat.mainTexture = faceTexture;
        Head.material = headMat;
        Material torsoMat = new Material(PartMaterial);
        torsoMat.color = BGR ? colors.Torso.BGR() : colors.Torso;
        if (shirtTexture != null) torsoMat.mainTexture = shirtTexture;
        Torso.material = torsoMat;
        Material leftArmMat = new Material(PartMaterial);
        leftArmMat.color = BGR ? colors.LeftArm.BGR() : colors.LeftArm;
        if (shirtTexture != null) leftArmMat.mainTexture = shirtTexture;
        LeftArm.material = leftArmMat;
        Material rightArmMat = new Material(PartMaterial);
        rightArmMat.color = BGR ? colors.RightArm.BGR() : colors.RightArm;
        if (shirtTexture != null) rightArmMat.mainTexture = shirtTexture;
        RightArm.material = rightArmMat;
        Material leftLegMat = new Material(PartMaterial);
        leftLegMat.color = BGR ? colors.LeftLeg.BGR() : colors.LeftLeg;
        if (pantsTexture != null) leftLegMat.mainTexture = pantsTexture;
        LeftLeg.material = leftLegMat;
        Material rightLegMat = new Material(PartMaterial);
        rightLegMat.color = BGR ? colors.RightLeg.BGR() : colors.RightLeg;
        if (pantsTexture != null) rightLegMat.mainTexture = pantsTexture;
        RightLeg.material = rightLegMat;

        Material tshirtMat = new Material(TShirtMaterial);
        if (tshirtTexture != null) {
            tshirtMat.mainTexture = tshirtTexture;
            TShirt.gameObject.SetActive(true);
        } else {
            TShirt.gameObject.SetActive(false);
        }
        TShirt.material = tshirtMat;
    }

    public void SetHeadMesh (Mesh head) {
        Head.gameObject.GetComponent<MeshFilter>().mesh = head;
    }

    public void SetFaceTexture (Texture face) {
        faceTexture = face;
        Head.material.mainTexture = faceTexture;
    }

    public void SetShirtTexture (Texture shirt) {
        shirtTexture = shirt;
        Torso.material.mainTexture = shirtTexture;
        LeftArm.material.mainTexture = shirtTexture;
        RightArm.material.mainTexture = shirtTexture;
    }

    public void SetPantsTexture (Texture pants) {
        pantsTexture = pants;
        LeftLeg.material.mainTexture = pantsTexture;
        RightLeg.material.mainTexture = pantsTexture;

        // torso underlay
        Torso.material.SetTexture("_UnderlayTexture", pantsTexture);
    }

    public void SetTshirtTexture (Texture tshirt) {
        tshirtTexture = tshirt;
        TShirt.material.mainTexture = tshirtTexture;
        TShirt.gameObject.SetActive(true);
    }

    /*

    public void SetHatTexture (Texture texture) {
        if (HatsDict.TryGetValue(texture.name, out GameObject hat)) {
            Material hatmat = new Material(HatMaterial);
            hatmat.mainTexture = texture;
            hat.AddComponent<MeshRenderer>().material = hatmat;
        } else {
            int hatNumber = HatsDict.Count+1;
            GameObject hatGO = new GameObject("Hat " + hatNumber);
            if (player.NetID == main.localInfo.LocalPlayer.NetID) hatGO.layer = 9; // give local hats self tag
            Material hatmat = new Material(HatMaterial);
            hatmat.mainTexture = texture;
            hatGO.AddComponent<MeshRenderer>().material = hatmat;
            HatsDict.Add(texture.name, hatGO);
        }
    }
    */

    public void SetHatMesh (int index, Mesh mesh) {
        GameObject h;
        if (Hats[index] != null) {
            h = Hats[index];
        } else {
            h = new GameObject("Hat " + index);
            //if (player.NetID == main.localInfo.LocalPlayer.NetID) h.layer = 9; // give local hats self tag
            Hats[index] = h;
        }
        MeshFilter mf = h.GetComponent<MeshFilter>();
        if (mf == null) mf = h.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        h.transform.SetParent(transform);
        h.transform.localPosition = Vector3.zero;
        h.transform.localRotation = Quaternion.identity;
    }

    public void SetHatTexture (int index, Texture texture) {
        GameObject h;
        if (Hats[index] != null) {
            h = Hats[index];
        } else {
            h = new GameObject("Hat " + index);
            //if (player.NetID == main.localInfo.LocalPlayer.NetID) h.layer = 9; // give local hats self tag
            Hats[index] = h;
        }
        Material hatMat = new Material(HatMaterial);
        hatMat.mainTexture = texture;
        MeshRenderer mr = h.GetComponent<MeshRenderer>();
        if (mr == null) mr = h.AddComponent<MeshRenderer>();
        mr.material = hatMat;
    }

    public void SetToolMesh (Mesh mesh) {
        ToolGO.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetToolTexture (Texture texture) {
        Material toolMat = new Material(HatMaterial);
        toolMat.mainTexture = texture;
        ToolGO.GetComponent<MeshRenderer>().material = toolMat;
    }

    public void GetCharacterAssets (AvatarRoot root) {
        // face
        /*
        if (root.items.face != 0) {
            CharacterAssetHelper.instance.GetTexture(root.items.face, SetFace);
        } else {
            SetFace(main.DefaultFace);
        }
        */

        // head
        //if (root.items.head != 0) CharacterAssetHelper.instance.GetMesh(root.items.head, SetHeadMesh); // TODO: Fix.

        // clothing
        /*
        if (root.items.shirt != 0) CharacterAssetHelper.instance.GetTexture(root.items.shirt, SetShirt, false);
        if (root.items.pants != 0) CharacterAssetHelper.instance.GetTexture(root.items.pants, SetPants, false);
        if (root.items.tshirt != 0) CharacterAssetHelper.instance.GetTexture(root.items.tshirt, SetTShirt, false);
        */

        // hats
        /*
        for (int i = 0; i < root.items.hats.Length; i++) {
            if (root.items.hats[i] != 0) {
                CharacterAssetHelper.instance.GetMesh(root.items.hats[i], SetHatMesh);
                CharacterAssetHelper.instance.GetTexture(root.items.hats[i], SetHatTexture);
            }
        }
        */
    }

    public void SetFace (string id) {
        if (id == null || id == "" || id == "none") { SetFaceTexture(defaultFaceTexture); return; }
        CharacterAssetHelper.instance.GetTexture(id, SetFaceTexture);
    }
    
    public void SetShirt(string id)
    {
        if (id == null || id == "" || id == "none") { return; }
        CharacterAssetHelper.instance.GetTexture(id, SetShirtTexture);
    }
    
    public void SetPants(string id)
    {
        if (id == null || id == "" || id == "none") { return; }
        CharacterAssetHelper.instance.GetTexture(id, SetPantsTexture);
    }
    
    public void SetTShirt(string id)
    {
        if (id == null || id == "" || id == "none") { return; }
        CharacterAssetHelper.instance.GetTexture(id, SetTshirtTexture);
    }

    public void SetHat (int index, string id, string id2) {
        if (id == null || id == "" || id == "none") { return; }

        Action<Mesh> meshCallback = (mesh) => SetHatMesh(index, mesh);
        CharacterAssetHelper.instance.GetMesh(id, meshCallback);

        Action<Texture> texCallback = (tex) => SetHatTexture(index, tex);
        CharacterAssetHelper.instance.GetTexture(id2, texCallback);
    }

    public void SetTool (string id, string id2) {
        if (id == null || id == "" || id == "none") { return; }
        CharacterAssetHelper.instance.GetMesh(id, SetToolMesh);
        CharacterAssetHelper.instance.GetTexture(id2, SetToolTexture);
    }

    public void SetVisibility (bool visible) {
        // Head, Torso, Arms, Legs, Hats, Tools
        UnityEngine.Rendering.ShadowCastingMode mode = visible ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        
        Head.shadowCastingMode = mode;
        Torso.shadowCastingMode = mode;
        TShirt.shadowCastingMode = mode;
        LeftArm.shadowCastingMode = mode;
        RightArm.shadowCastingMode = mode;
        LeftLeg.shadowCastingMode = mode;
        RightLeg.shadowCastingMode = mode;

        for (int i = 0; i < Hats.Length; i++) {
            if (Hats[i] != null)
                Hats[i].GetComponent<MeshRenderer>().shadowCastingMode = mode;
        }
    }

    public void SendMovementPacket(bool round = true)
    {
        Vector3 vector = round ? base.transform.position.Round(100f) : base.transform.position;
        if (vector.y == 0f)
        {
            vector.y = 0.03f;
        }
        PacketBuilder packetBuilder = new PacketBuilder(2);
        packetBuilder.AppendFloat(vector.x * -1f);
        packetBuilder.AppendFloat(vector.z);
        packetBuilder.AppendFloat(vector.y);
        packetBuilder.AppendFloat(((int)base.transform.eulerAngles.y * -1 - 180).Mod(360));
        packetBuilder.AppendFloat(((int)base.transform.eulerAngles.y * -1 - 180).Mod(360));
        main.SendPacket(packetBuilder.GetBytes());
    }

    public void SendMovementPacketOld (bool round = true) {
        Vector3 rounded = round ? transform.position.Round(100) : transform.position;
        if (rounded.y == 0) rounded.y = 0.03f;

        PacketBuilder b = new PacketBuilder((byte)NetworkManager.ClientPackets.Movement);
        b.AppendFloat(rounded.x * -1f); //x
        b.AppendFloat(rounded.z); //y
        b.AppendFloat(rounded.y); //z
        b.AppendUInt((uint)Helper.Mod((int)transform.eulerAngles.y*-1-180, 360)); // rotation
        main.SendPacket(b.GetBytes());
    }

    public void SendInputPacket (bool mouse, string key) {
        if (!InputHelper.currentControlsState) return; // only send input when input is enabled
        PacketBuilder b = new PacketBuilder((byte)NetworkManager.ClientPackets.Input);
        b.AppendByte(mouse ? (byte)1 : (byte)0); // if lmb is pressed
        b.AppendString(key); // which key is pressed
        main.SendPacket(b.GetBytes());
    }
}
