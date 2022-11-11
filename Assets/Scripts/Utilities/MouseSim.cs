using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseSim : MonoBehaviour, IPointerDownHandler
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;
    PlayerUI m_PlayerUI;

    private void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = canvas.GetComponent<EventSystem>();

        GameObject manager = GameObject.Find("Manager");
        m_PlayerUI = manager.GetComponent<PlayerUI>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = eventData.position;

        List<RaycastResult> results = new List<RaycastResult>();

        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach (RaycastResult result in results)
        {
            try
            {
                m_PlayerUI.SetChatFocused(false);
                Button btn = null;
                result.gameObject.TryGetComponent<Button>(out btn);
                if (btn)
                {
                    btn.onClick.Invoke();
                    continue;
                }
                TMPro.TMP_InputField input = null;
                result.gameObject.TryGetComponent<TMPro.TMP_InputField>(out input);
                if (input)
                {
                    m_PlayerUI.SetChatFocused(true);
                    //input.Select();
                    continue;
                }
            }
            catch { }
        }
    }
}