using UnityEngine;
using System.Collections;
using System;

public class TouchEvent
{
    public static int EventCount { get; private set; }
    public delegate bool TouchCondition(Vector2 inputPos);   //inputPos for touchManager to pass every touch position if they satisfy

    public TouchCondition Condition; //This is  Actually the Condition Based on Which TouchManager will Assign Touch Ids

    public bool IsCheckConditionEveryFrame;             //Note  : It may take some performance cost
    public bool IsCallOnEndedAfterTouchDataLost;
    public int LayerID;             //Layer Id for this Touch Event
    public Touch touch;             //This is the updated touch assigned every frame 
    public Action OnBegan;        //This is called when the touch just satifies the Condition
    public Action OnMoved;        //This is called when the touch moving
    public Action OnStationary;   //This is called when the touch stationary
    public Action OnEnded;        //This is called when the touch is ended or canceled

    public TouchEvent()
    {
        touch.fingerId = -1;       //Must be -ve initially
        IsCallOnEndedAfterTouchDataLost = true;
        IsCheckConditionEveryFrame = false;
        OnBegan = delegate() { };
        OnMoved = delegate() { };
        OnStationary = delegate() { };
        OnEnded = delegate() { };
        EventCount++; 
    }

}