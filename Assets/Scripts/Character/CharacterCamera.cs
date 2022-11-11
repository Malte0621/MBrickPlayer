using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utils;

public class CharacterCamera : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public CharacterMain main;

    uint touchCount = 0;
    Dictionary<uint, PointerEventData> deltaTouches = new Dictionary<uint, PointerEventData>();
    Dictionary<uint, PointerEventData> touches = new Dictionary<uint, PointerEventData>();
    Dictionary<int, uint> touches2 = new Dictionary<int, uint>();

    public float CameraDistance;
    public float MaxCameraDistance = 10f;
    public float MinCameraDistance = 1f;
    public float CameraSensitivity;
    public Vector3 CameraTargetPosition;
    public float ZoomSensitivity;
    public bool InvertZoom;
    public Transform CameraTarget;
    public Transform Player;
    public Transform CameraObject;

    public LayerMask CameraRaycastMask;
    public LayerMask DefaultViewMask;
    public LayerMask FirstPersonViewMask;

    private bool mouseLookButton;
    public bool firstPerson;

    private Camera cam;
    private float camDist;
    private Vector3 camRot;

    private bool inEditor = false;

    private void Awake () {
        cam = GetComponent<Camera>();
        inEditor = Application.platform == RuntimePlatform.LinuxEditor; // linux editor cursor locking is stupid so i disable cursor locking for ez testing
    }

    private void Start () {
        xAngle = 0.0f;
        yAngle = 0.0f;

        CameraSensitivity = SettingsManager.PlayerSettings.MouseSensitivity;
        ZoomSensitivity = SettingsManager.PlayerSettings.ZoomSensitivity;
    }

    private Vector3 firstpoint; //change type on Vector3
    private Vector3 secondpoint;
    private float xAngle = 0.0f; //angle for axes x for rotation
    private float yAngle = 0.0f;
    private float xAngTemp = 0.0f; //temp variable for angle
    private float yAngTemp = 0.0f;

    private void Update () {
        CameraTarget.position = Player.position + CameraTargetPosition; // get camera into position

        float zoom = 0;
        if (main.main.isPhone)
        {
            if (touchCount >= 2)
            {
                // get current touch positions
                PointerEventData tZero = touches[1];
                PointerEventData tOne = touches[2];
                PointerEventData deltaTZero = deltaTouches[1];
                PointerEventData deltaTOne = deltaTouches[2];
                // get touch position from the previous frame
                Vector2 tZeroPrevious = tZero.position - deltaTZero.position;
                Vector2 tOnePrevious = tOne.position - deltaTOne.position;

                float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
                float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

                // get offset value
                zoom = ((oldTouchDistance - currentTouchDistance) * -1) * (ZoomSensitivity / 100);
            }
        }
        else
        {
            zoom = InputHelper.controls.Player.Zoom.ReadValue<float>();
        }
        if (zoom != 0) {
            if (!InvertZoom) zoom *= -1;
            zoom /= 10000;
            CameraDistance = Mathf.Clamp(CameraDistance + zoom * ZoomSensitivity, MinCameraDistance, MaxCameraDistance);
        }
        
        if (main.player.PlayerFigure.CameraType == "first")
        {
            CameraDistance = 0;
        }

        if (CameraDistance == 0) {
            mouseLookButton = true;
            if (!firstPerson) {
                //cam.cullingMask = FirstPersonViewMask;
                main.SetVisibility(false);
                firstPerson = true;
                PlayerMain.instance.ui.CrosshairVisible = true;
            }
        } else if (firstPerson) {
            // exit first person
            //cam.cullingMask = DefaultViewMask;
            main.SetVisibility(true);
            mouseLookButton = false;
            PlayerMain.instance.ui.CrosshairVisible = false;
            firstPerson = false;
        }else if (!main.main.isPhone)
        {
            mouseLookButton = Mouse.current.rightButton.isPressed;
        }

        if (mouseLookButton || main.main.isPhone) {
            if (!inEditor && !main.main.isPhone) Cursor.lockState = CursorLockMode.Locked;
            PlayerMain.instance.ui.CrosshairVisible = true;

            Vector3 rot = Vector3.zero;

            if (main.main.isPhone)
            {
                if (touchCount > 0)
                {
                    secondpoint = touches[1].position;
                    xAngle = xAngTemp + (secondpoint.x - firstpoint.x) * 180.0f / Screen.width;
                    yAngle = yAngTemp - (secondpoint.y - firstpoint.y) * 90.0f / Screen.height;
                    CameraTarget.rotation = Quaternion.Euler(yAngle * (CameraSensitivity / 100), xAngle * (CameraSensitivity / 100), 0.0f);
                }
            }
            else
            {
                rot = Helper.SwapXY(InputHelper.controls.Player.Look.ReadValue<Vector2>());
                //Vector3 rot = new Vector3(-Input.GetAxi
                //s("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
                camRot += rot * (CameraSensitivity / 500);
                camRot.x = Mathf.Clamp(camRot.x, -89, 89);
                CameraTarget.eulerAngles = camRot;
            }
        } else {
            if (!inEditor) Cursor.lockState = CursorLockMode.None;
            if (!firstPerson) PlayerMain.instance.ui.CrosshairVisible = false;
        }

        Vector3 desiredCamPos = CameraTarget.TransformPoint(0,0,-(CameraDistance+1));

        if (Physics.Linecast(CameraTarget.position, desiredCamPos, out RaycastHit hit, CameraRaycastMask)) {
            camDist = Mathf.Clamp((hit.distance * 0.87f), MinCameraDistance, MaxCameraDistance);
        } else {
            camDist = CameraDistance;
        }

        CameraObject.rotation = CameraTarget.rotation;
        CameraObject.position = CameraTarget.TransformPoint(0,0,-camDist);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        touchCount++;
        if (touchCount == 1)
        {
            firstpoint = eventData.position;
            xAngTemp = xAngle;
            yAngTemp = yAngle;
        }
        touches.Add(touchCount,eventData);
        deltaTouches.Add(touchCount, eventData);
        touches2.Add(eventData.pointerId,touchCount);
    }

    public void OnDrag(PointerEventData eventData)
    {
        deltaTouches[touches2[eventData.pointerId]] = touches[touches2[eventData.pointerId]];
        touches[touches2[eventData.pointerId]] = eventData;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        deltaTouches.Remove(touches2[eventData.pointerId]);
        touches.Remove(touches2[eventData.pointerId]);
        touches2.Remove(eventData.pointerId);
        touchCount--;
    }
}
