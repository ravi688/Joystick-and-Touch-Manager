using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class JoyStick 
{
    public Vector2 Position
    {
        get
        {
            return Body.localPosition;
        }
        set
        {
            Handle.localPosition = value;
            Body.localPosition = value;
            Settings.position = value;
        }
    }
    public JoyStickSettings Settings { get; private set; }

    public bool IsRunning { get; set; }
    public Vector2 Axis { get { return GetAxis(); } set { } }

    public RectTransform Handle { get; set; }
    public RectTransform Body { get; set; }
    Vector2 InitialHandlePos;
    Vector2 RadiusVector;
    Vector2 InputPos;

    static readonly Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
    float HandleRadius;
    TouchEvent touchEvent;

    public static JoyStick CreateJoyStick(JoyStickSettings Settings, int TouchLayerID)
    {
        #region Joystick Runtime Instantiation
        GameObject joystick = new GameObject("Joystick");
        GameObject handle = new GameObject("Handle");
        handle.AddComponent<Image>().sprite = Settings.HandleSprite;
        GameObject body = new GameObject("Body");
        body.AddComponent<Image>().sprite = Settings.BodySprite;
        RectTransform HandleRectTr = handle.GetComponent<RectTransform>();
        RectTransform BodyRectTr = body.GetComponent<RectTransform>();
        HandleRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Settings.HandleSize.x);
        HandleRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Settings.HandleSize.y);
        BodyRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Settings.BodySize.x);
        BodyRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Settings.BodySize.y);

        GameObject canvas = null;
        if ((canvas = GameObject.FindObjectOfType<Canvas>().gameObject) == null)
        {
            Debug.LogError("There is no Canvas ");
            return null;
        }
        joystick.transform.SetParent(canvas.transform);
        joystick.transform.localPosition = Vector2.zero;
        HandleRectTr.SetParent(joystick.transform);
        BodyRectTr.SetParent(joystick.transform);

        HandleRectTr.localPosition = Settings.position;           //It is supposed that the Settings.position has origin at the center of the Screen
        BodyRectTr.localPosition = Settings.position;
        #endregion

        return new JoyStick(HandleRectTr, BodyRectTr, Settings, TouchLayerID);
    }
    private static int instanceCount = 0;
    private int id; 
    private JoyStick(RectTransform Handle, RectTransform Body, JoyStickSettings settings, int TouchLayerID)
    {
        touchEvent = new TouchEvent();
        touchEvent.Condition = IsInsideOfHandle;
        touchEvent.LayerID = TouchLayerID;
        TouchManager.RegisterEvent(touchEvent);

        Settings = settings;
        IsRunning = true;
        this.Handle = Handle;
        this.Body = Body;
        HandleRadius = settings.HandleSize.x * 0.5f;
        InitialHandlePos = Handle.localPosition;
    }

    public void Update()
    {
        if (!Settings.IsInteractive) return;

        InputPos = touchEvent.touch.position - ScreenSize * 0.5f;

        RadiusVector = InputPos - InitialHandlePos;  //initial handle pos has origin at the center of the screen

        if (TouchManager.IsTouched(touchEvent))
        {
            if (RadiusVector.sqrMagnitude > Settings.BodySize.x * Settings.BodySize.x * 0.25f)
                Handle.localPosition = RadiusVector.normalized * Settings.BodySize.x * 0.5f + InitialHandlePos;
            else
                Handle.localPosition = InputPos;
        }
        else
            Handle.localPosition = Vector2.Lerp(Handle.localPosition, InitialHandlePos, Time.deltaTime * Settings.HandleReturnSpeed);

    }
    private bool IsInsideOfHandle(Vector2 input_pos)
    {
        RadiusVector = input_pos - InitialHandlePos;
        bool result =  RadiusVector.sqrMagnitude <= HandleRadius * HandleRadius;
        return result; 
    }
    private Vector2 GetAxis()
    {
        if (((Vector2)Handle.localPosition - InitialHandlePos).sqrMagnitude < Settings.Threshold_distance * Settings.Threshold_distance)
            return Vector2.zero;
        else
            return ((Vector2)Handle.localPosition - InitialHandlePos) * Settings.Sensitivity / (Settings.BodySize.x * 0.5f);
    }
}