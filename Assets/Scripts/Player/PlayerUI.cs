using BrickHill;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;
using System.Linq;

public class PlayerUI : MonoBehaviour
{
    public PlayerMain main;

    public Canvas UICanvas;

    public GameObject[] GameUI;

    public TMP_Text ClientInfoText;

    public GameObject DCPanel;
    //public TMP_InputField DirectConnectGameID;
    //public TMP_InputField DirectConnectIP;
    //public TMP_InputField DirectConnectPort;
    //public TMP_InputField DirectConnectCookies;
    public bool dcVisible = false;

    public TMP_Text GameInfoText;

    public TMP_Text LocalPlayerName;
    public TMP_Text LocalPlayerScore;

    public RectTransform DefaultPlayerlistRoot;
    public RectTransform FullPlayerListRoot;
    public GameObject PlayerlistElement;
    public GameObject[] PlayerlistElements;
    public Sprite[] PlayerlistIcons;
    public Color AdminColor;
    public GameObject PlayerlistTeamElement;
    public List<GameObject> PlayerlistTeamElements = new List<GameObject>();

    public Color GenericBackgroundColor;

    public GameObject MenuRoot;
    public SettingsElement[] SettingsElements;
    public GameObject ShowMenuButton;
    private bool menuVisible = false;

    public GameObject ChatRoot;
    public Image ChatBG;
    public Transform ChatLogContent;
    public TMP_InputField ChatInputField;
    public ScrollRect ChatScrollrect;
    public GameObject ChatMessageTemplate;
    private bool chatFocused = false;
    //public GameObject MaximizeChatButton;
    //private bool chatVisible = true;

    public GameObject[] PrintTexts;
    private Coroutine[] PrintCoroutines = new Coroutine[3];

    public GameObject ConnectingStatusRoot;
    public TMP_Text ConnectingStatusText;

    public GameObject MessageRoot;
    public TMP_Text MessageTitle;
    public TMP_Text MessageText;

    public Image HealthBar;
    public Gradient HealthColor;

    public Transform InventoryRoot;
    public GameObject ToolSlot;
    public Dictionary<int, GameObject> Toolslots = new Dictionary<int, GameObject>();
    private int toolIndex = 1;

    public GameObject Crosshair;
    public bool CrosshairVisible;

    public TMP_Text FPSCounter;
    private int FPSWaitFrames = 5;
    private int currentFrame = 0;
    private float collectiveDeltaTime = 0f;

    //public AudioClip[] ChatSounds;
    public AudioClip PopSound;

    public Texture2D[] Cursors;
    private int currentCursor;

    public Material UIBlurMat;

    private float uiBlurAmount = 3f;
    private bool showFPS;
    private FixedButton menuButtonScript;

    private void Start() {
        SetGameUI(false);
        UpdateSettingsUI();

        ClientInfoText.SetText(Application.productName + " " + Application.version);

        InputHelper.controls.Player.ToggleMenu.performed += ctx => SetMenuVisible(!menuVisible);

        if (main.isPhone)
        {
            GameObject canvas = GameObject.Find("Canvas");
            canvas.transform.Find("MenuButton").gameObject.SetActive(true);
            menuButtonScript = canvas.transform.Find("MenuButton").gameObject.GetComponent<FixedButton>();
            foreach(Transform idk in canvas.transform)
            {
                if (idk.gameObject.name == "Idk")
                {
                    idk.gameObject.SetActive(true);
                }
            }
        }

        //InputHelper.controls.Player.ToggleChat.performed += ctx => SetChatVisible(!chatVisible);
        
        InputHelper.controls.Player.Chatbox.performed += ctx => SetChatFocused(true); //ChatInputField.Select();

        SettingsElements[10].slider.maxValue = Screen.currentResolution.refreshRate; // set max framelimit

        SetUIBlur(SettingsManager.PlayerSettings.UIBlur);
        SetFPSVisible(SettingsManager.PlayerSettings.ShowFPS);
    }

    private void Update() {
        if (menuButtonScript && menuButtonScript.Pressed)
        {
            SetMenuVisible(true);
        }
        if (SettingsManager.PlayerSettings.ShowFPS) {
            int currentFPS = (int)(1.0f / Time.deltaTime);
            FPSWaitFrames = currentFPS / 4; // try to update fps counter about 4x a second
            if (FPSWaitFrames <= 0) FPSWaitFrames = 1; // prevent it from being 0

            if (currentFrame < FPSWaitFrames) {
                collectiveDeltaTime += Time.deltaTime;
                currentFrame++;
            } else {
                int fps = (int)(currentFrame / collectiveDeltaTime);
                FPSCounter.text = fps.ToString() + " FPS";
                
                collectiveDeltaTime = 0f;
                currentFrame = 0;
            }
        }

        if (CrosshairVisible) {
            if (Crosshair.activeSelf == false) Crosshair.SetActive(true);
        } else {
            if (Crosshair.activeSelf == true) Crosshair.SetActive(false);
        }

        if (!chatFocused && !menuVisible && !main.isPhone)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 1)
                {
                    int slotId = Toolslots.Keys.ToArray()[0];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 2)
                {
                    int slotId = Toolslots.Keys.ToArray()[1];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 3)
                {
                    int slotId = Toolslots.Keys.ToArray()[2];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 4)
                {
                    int slotId = Toolslots.Keys.ToArray()[3];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 5)
                {
                    int slotId = Toolslots.Keys.ToArray()[4];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit6Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 6)
                {
                    int slotId = Toolslots.Keys.ToArray()[5];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit7Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 7)
                {
                    int slotId = Toolslots.Keys.ToArray()[6];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit8Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 8)
                {
                    int slotId = Toolslots.Keys.ToArray()[7];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit9Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 9)
                {
                    int slotId = Toolslots.Keys.ToArray()[8];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
            else if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                if (Toolslots.Keys.Count >= 10)
                {
                    int slotId = Toolslots.Keys.ToArray()[9];
                    SelectTool(slotId, true, int.Parse(Toolslots[slotId].gameObject.name));
                }
            }
        }
    }

    public void ShowDCPanel () {
        DCPanel.SetActive(true);
        dcVisible = true;
    }

    public void DirectConnect () {
        /*
        if (!string.IsNullOrWhiteSpace(DirectConnectGameID.text) && !string.IsNullOrWhiteSpace(DirectConnectIP.text) && !string.IsNullOrWhiteSpace(DirectConnectPort.text) && !string.IsNullOrWhiteSpace(DirectConnectCookies.text)) {
            main.net.GameID = DirectConnectGameID.text;
            main.net.ServerIP = DirectConnectIP.text;
            main.net.ServerPort = int.Parse(DirectConnectPort.text, CultureInfo.InvariantCulture);
            main.net.CookieToken = DirectConnectCookies.text;
            main.net.AuthenticateAndConnect();

            // delete dc panel
            DirectConnectIP = null;
            DirectConnectPort = null;
            DirectConnectCookies = null;

            // show ui
            dcVisible = false;
            SetGameUI(true);
        }
        */
        main.net.ConnectToServer("local", 42480, "local"); // connect to local server

        // hide ui
        dcVisible = false;
        SetGameUI(true);
    }

    public void SetGameUI (bool visible) {
        for (int i = 0; i < GameUI.Length; i++) {
            if (GameUI[i] != null)
                GameUI[i].SetActive(visible);
        }

        if (visible) {
            Destroy(DCPanel);
            Destroy(ClientInfoText);
        }
    }

    public void UpdateGameInfo (LocalInformation li) {
        GameInfoText.SetText($"{li.LocalPlayer.Name}\nID: {li.LocalPlayer.ID}\nAdmin: {li.LocalPlayer.Admin}\nMembership: {li.LocalPlayer.Membership}\nNetID: {li.LocalPlayer.NetID}\nMap Size: {li.MapSize} Bricks");
    }

    public void SetLocalPlayerInfo (Player p) {
        LocalPlayerName.text = p.Name;
        LocalPlayerScore.text = p.PlayerFigure.Score.ToString();
    }

    public void UpdatePlayerlist () {
        if (PlayerlistElements != null) {
            for (int i = 0; i < PlayerlistElements.Length; i++) {
                Destroy(PlayerlistElements[i]);
            }
            PlayerlistElements = null;
        }
        PlayerlistElements = new GameObject[main.Playerlist.Count];

        for (int i = 0; i < main.Playerlist.Count; i++) {
            GameObject ple = Instantiate(PlayerlistElement);
            ple.name = main.Playerlist[i].NetID.ToString();
            int iconIndex = (int)Mathf.Clamp(main.Playerlist[i].Membership - 2, -1, 2);
            if (main.Playerlist[i].Admin) iconIndex = 3;
            if (main.Playerlist[i].ID == 2760) iconIndex = 4; // vaporeon

            if (iconIndex >= 0) ple.transform.GetChild(0).GetComponent<Image>().sprite = PlayerlistIcons[iconIndex];

            ple.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = main.Playerlist[i].Name;
            if (iconIndex == 3) ple.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = AdminColor;

            ple.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "0"; // score

            ple.transform.SetParent(DefaultPlayerlistRoot, false);
            ple.SetActive(true);

            PlayerlistElements[i] = ple;
        }
    }

    public void AddTeamToPlayerlist (Team t) {
        GameObject plte = Instantiate(PlayerlistTeamElement);
        plte.name = t.Name;

        plte.transform.SetParent(FullPlayerListRoot, false);
        TextMeshProUGUI teamLabel = plte.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        teamLabel.SetText(t.Name);
        teamLabel.color = t.TeamColor;

        plte.SetActive(true);
        PlayerlistTeamElements.Add(plte);
    }

    public void AddPlayerToTeam (int playerNetID, int teamNetID) {
        for (int i = 0; i < PlayerlistElements.Length; i++) {
            if (PlayerlistElements[i].name == playerNetID.ToString()) {
                RectTransform listTrans = PlayerlistTeamElements[teamNetID - 1].transform.GetChild(1).GetComponent<RectTransform>();
                PlayerlistElements[i].transform.SetParent(listTrans, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(FullPlayerListRoot);
                LayoutRebuilder.ForceRebuildLayoutImmediate(listTrans);
                return;
            }
        }
        
    }

    public void UpdatePlayerScore (int netID) {
        if (netID == main.localInfo.LocalPlayer.NetID) {
            // it me
            LocalPlayerScore.text = main.localInfo.LocalPlayer.PlayerFigure.Score.ToString();
        }
        for (int i = 0; i < PlayerlistElements.Length; i++) {
            if (PlayerlistElements[i].name == netID.ToString()) {
                PlayerlistElements[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = main.GetPlayer(netID).PlayerFigure.Score.ToString();
            }
        }
    }

    public void SetMenuVisible (bool value) {
        if (dcVisible) return; // dont show menu if direct connect panel is visible
        MenuRoot.SetActive(value);
        //ShowMenuButton.SetActive(!value);
        menuVisible = value;

        // hide chat
        //bool chatState = chatVisible ? !value : false;
        //MaximizeChatButton.SetActive(!chatState);
        ChatRoot.SetActive(!value);

        // set input state
        InputHelper.SetControlsEnabled(!value);

        // unlock mouse
    }

    public void SendChatMessage (bool autoSplit = true) {
        string messageToSend = ChatInputField.text;
        string[] messages;

        if (messageToSend.Length > 85 && autoSplit) {
            messages = new string[Mathf.CeilToInt(messageToSend.Length/85f)];
            int index = 0;
            for (int i = 0; i < messageToSend.Length; i += 85, index++) {
                int chunkSize = 85;
                if (i + 85 > messageToSend.Length) chunkSize = messageToSend.Length - i;
                messages[index] = messageToSend.Substring(i, chunkSize);
            }
        } else {
            messages = new string[1];
            messages[0] = messageToSend;
        }

        for (int i = 0; i < messages.Length; i++) {
            if (!string.IsNullOrWhiteSpace(messages[i])) {
                if (messages[i][0] == '/') {
                    string msg = messages[i].Substring(1);
                    string command = msg.Split(' ')[0];
                    string args = msg.Replace(command+" ", "");
                    SendMessage(command, args);
                    ChatInputField.text = "";
                    SetChatFocused(false);
                } else {
                    SendMessage("chat", messages[i]);
                    ChatInputField.text = "";
                    SetChatFocused(false);
                }
            }
        }
    }

    private void SendMessage (string type, string message) {
        PacketBuilder b = new PacketBuilder((byte)NetworkManager.ClientPackets.Command);
        b.AppendString(type);
        b.AppendString(message);
        main.SendPacket(b.GetBytesCompressed());
        ChatInputField.text = "";
    }

    public void LogChatMessage (string content) { // TODO: combine messages into single tmp object if possible
        string filteredMessage = Helper.FilteredText(content); // convert color codes to be compatible with TMP

        GameObject messageGO = Instantiate(ChatMessageTemplate); // create message object
        messageGO.transform.SetParent(ChatLogContent, false); // set parent
        messageGO.GetComponent<TMP_Text>().SetText(filteredMessage); // set text
        messageGO.SetActive(true); // show

        Canvas.ForceUpdateCanvases();
        ChatScrollrect.verticalNormalizedPosition = 0f;

        Helper.PlaySound(PopSound, 1f, 0.8f); // audio

        //StopCoroutine(BeginChatUnfocusTimer());
        //StartCoroutine(BeginChatUnfocusTimer());
    }

    public IEnumerator BeginChatUnfocusTimer () {
        yield return new WaitForSeconds(10f);
        SetChatFocused(false);
    }

    public void SetChatFocused (bool focus) {
        //StopCoroutine(BeginChatUnfocusTimer());

        Color bgColor = focus ? GenericBackgroundColor : new Color(0,0,0,0);
        ChatBG.color = bgColor;
        chatFocused = focus;
        
        if (focus) {
            ChatInputField.Select();
        } else {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SetChatVisible (bool visible) {
        if (menuVisible || dcVisible) return; // dont change chat state if menu or dc panel is open
        //MaximizeChatButton.SetActive(!visible);
        ChatRoot.SetActive(visible);
        //chatVisible = visible;
    }

    public void PrintMessage (int pos, string message, int time) {
        GameObject printGO = PrintTexts[pos];
        TextMeshProUGUI text = printGO.GetComponent<TextMeshProUGUI>();

        if (PrintCoroutines[pos] != null) {
            // there is already a print displayed - replace it
            StopCoroutine(PrintCoroutines[pos]);
            PrintCoroutines[pos] = null;
        }

        text.SetText(Helper.FilteredText(message));
        printGO.SetActive(true);

        PrintCoroutines[pos] = StartCoroutine(hidePrintText(text, time));
    }

    IEnumerator hidePrintText (TextMeshProUGUI text, int time) {
        yield return new WaitForSeconds(time); // wait x seconds
        text.SetText(""); // clear text
        text.gameObject.SetActive(false); // disable GO
    }

    public void SetConnectingStatus (bool visible, string text = "") {
        ConnectingStatusRoot.SetActive(visible);
        ConnectingStatusText.SetText(text);
    }

    public void ShowMessage (string title, string text) {
        MessageTitle.SetText(title);
        MessageText.SetText(text);

        SetChatVisible(false);
        SetMenuVisible(false);
        MessageRoot.SetActive(true);

        // set input state
        InputHelper.SetControlsEnabled(false);
    }

    public void UpdateHealth (float value) {
        HealthBar.fillAmount = value;
        HealthBar.color = HealthColor.Evaluate(Mathf.Clamp01(HealthBar.fillAmount));
    }

    public void AddTool (Tool tool) {
        int slotID = tool.SlotID;
        int index = toolIndex;

        GameObject slot = Instantiate(ToolSlot);
        slot.gameObject.name = index.ToString();
        slot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(tool.Name);
        slot.gameObject.GetComponent<Button>().onClick.AddListener(delegate {SelectTool(slotID, true, index);});
        slot.transform.SetParent(InventoryRoot, false);
        slot.SetActive(true);

        Toolslots.Add(slotID, slot);
        toolIndex++;
    }

    public void DeleteTool (int slot) {
        Destroy(Toolslots[slot]);
        Toolslots.Remove(slot);
        toolIndex--;
    }

    public void SelectTool (int id, bool simulateInput = false, int inputToSimulate = 0) {
        if (Toolslots.TryGetValue(id, out GameObject slot)) {
            if (slot.transform.GetChild(1).gameObject.active)
            {
                DeselectTools();
            }
            else
            {
                DeselectTools();
                slot.transform.GetChild(1).gameObject.SetActive(true);
            }
            if (simulateInput) main.Characters[main.localInfo.LocalPlayer.NetID].GetComponent<CharacterMain>().SendInputPacket(false, inputToSimulate.ToString());
        }
    }

    public void DeselectTools () {
        foreach (KeyValuePair<int, GameObject> kvp in Toolslots) {
            kvp.Value.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public void SetCursor (int index) {
        if (index != currentCursor) {
            Cursor.SetCursor(Cursors[index], Vector2.zero, CursorMode.Auto);
            currentCursor = index;
        }
    }

    // settings

    public void UpdateSettingsUI () {
        CharacterCamera cc = main.LocalCamera.GetComponent<CharacterCamera>();

        for (int i = 0; i < SettingsElements.Length; i++) {
            object val = SettingsManager.GetSettingFromIndex(i);
            SettingsElements[i].SetValue(val); // ez

            if (SettingsElements[i].returnSetting == SettingsElement.SettingReturn.Float) {
                string label = GetSettingLabel(i, (float)val);
                if (label == "VALUE") label = val.ToString();
                SettingsElements[i].SetLabel(label);
            } else if (SettingsElements[i].returnSetting == SettingsElement.SettingReturn.Integer) {
                string label = GetSettingLabel(i, (int)val);
                if (label == "VALUE") label = val.ToString();
                SettingsElements[i].SetLabel(label);
            }
        }

    }

    public void UpdateSetting (int index) {
        /*
        for (int i = 0; i < SettingsElements.Length; i++) {
            SettingsManager.SetSettingFromIndex(i, SettingsElements[i].GetValue());
        }
        */
        object val = SettingsElements[index].GetValue();
        SettingsManager.SetSettingFromIndex(index, val);

        if (SettingsElements[index].returnSetting == SettingsElement.SettingReturn.Float) {
            string label = GetSettingLabel(index, (float)val);
            if (label == "VALUE") label = val.ToString();
            SettingsElements[index].SetLabel(label);
        } else if (SettingsElements[index].returnSetting == SettingsElement.SettingReturn.Integer) {
            string label = GetSettingLabel(index, (int)val);
            if (label == "VALUE") label = val.ToString();
            SettingsElements[index].SetLabel(label);
        }

        main.SetSettings(index);
    }

    public void SaveSettings () {
        SettingsManager.SaveSettings();
    }

    public string GetSettingLabel (int index, float value) {
        switch (index) {
            case 4: return (Mathf.Round(value * 100) + "%");
            case 5: return (value == 0 ? "Half" : "Full");
            case 6: return (value == 0 ? "Off" : value == 1 ? "Hard" : "Soft");
            case 7: return (value == 0 ? "Low" : value == 1 ? "Medium" : "Hard");
            case 8: return (value == 0 ? "Off" : value == 1 ? "2x" : value == 2 ? "4x" : "8x");
            case 10: return (value <= 14 ? "Off" : "VALUE");
        }
        return "VALUE";
    }

    public void SetUIBlur (bool value) {
        if (value) {
            UIBlurMat.SetFloat("_Radius", uiBlurAmount);
        } else {
            UIBlurMat.SetFloat("_Radius", 0f);
        }
    }

    public void SetFPSVisible (bool value) {
        showFPS = value;
        FPSCounter.gameObject.SetActive(value);
    }
}
