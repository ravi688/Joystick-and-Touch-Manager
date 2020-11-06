using UnityEngine;
using System.Collections.Generic;



//This is MonoBehaviour must be Attached to a GameObject in the Scene
public class TouchManager : MonoBehaviour
{
    private static Touch[] touches;                         //Stores the Current Raw Touches present on the Screen
    private static List<TouchLayer> TouchLayers;            //Touch Layers registered by the Queries
    private static TouchLayer[] ArrTouchLayers;             //Temporary Array of TouchLayer for Faster memory accessing
    private static List<int> ReservedFingerIDs;             //This is used to store the current reserved touche finger ids based on the conditions
    private static List<TouchEvent> RegisteredEvents;       //This stores all the Registered Touch Events of all the layers

    private static int num_touches;                         //This stores the no of raw touch counts present on the screen
    private static int layer_count;                         //This stores touch layer count which are registerd by TouchEvents
    private static bool IsAnyChange;                        //This will be true in the current frame in which any change in touch count has occured
    private static bool isCleared;                          //This is temporary variable , holds the information whether Registered Ids are cleared or not after the Touch count == 0
    public static readonly Vector2 screenSize = new Vector2(Screen.width, Screen.height);
    private static bool IsInitialized;                      //This is temporary variable and most important, initially It is false
                                 //It is assumed that no  script can register TouchEvent During After one Frame Update
                                 //Since after the Start, Update is Called for First Time , in the update
                                 //all the layers are sorted and initialized
                                 //This is so because any script is free to register TouchEvents in Start or Awake
                           

    //This setter must be called to register an TouchEvent from any Script
    public static void RegisterEvent(TouchEvent touchEvent)
    {
        if (RegisteredEvents == null)
            RegisteredEvents = new List<TouchEvent>();
        if (TouchLayers == null)
            TouchLayers = new List<TouchLayer>();
        if (!IsAlreadyRegisteredLayer(touchEvent.LayerID))
        {
            TouchLayer new_layer = new TouchLayer();
            new_layer.id = touchEvent.LayerID;
            new_layer.touchEvents.Add(touchEvent);
            TouchLayers.Add(new_layer);
        }
        else    //if Already Registered Layer
        {       //Find the Layer and Add the touchEvent to this Event
            GetTouchLayerWithID(touchEvent.LayerID).touchEvents.Add(touchEvent);
        }

        RegisteredEvents.Add(touchEvent);
    }
    //This is to Query whether passed TouchEvent has Touched or not
    public static bool IsTouched(TouchEvent touchEvent)
    {
        return isReserved(touchEvent.touch.fingerId);
    }

    //This is to Query whether the passed Finger id is current running for other Events or not 
    private static bool isReserved(int id)
    {
        int[] arr = ReservedFingerIDs.ToArray();
        int count = arr.Length;
        for (int i = 0; i < count; i++)
            if (arr[i] == id)
                return true;
        return false;
    }
    //This is used to Get Touch with fingerId, used to assign appropriate touches to the TouchEvents for Continuos tracking
    private static Touch GetTouchWithID(int id)
    {
        Touch out_touch = new Touch();
        for (int i = 0; i < num_touches; i++)
            if (id == touches[i].fingerId)
            {
                out_touch = touches[i];
                break;
            }
            else
                continue;
        return out_touch;
    }
    //This is used to Reserve a fingerId so that any other TouchEvent can't access the touch
    //until this reserved fingerId is UnReserved by calling and passing the fingerId to UnReserve(int id) method
    private static void ReserveID(int id)
    {
        if (ReservedFingerIDs == null)
            ReservedFingerIDs = new List<int>();
        ReservedFingerIDs.Add(id);
    }
    private static void UnReserveID(int id)
    {
        ReservedFingerIDs.Remove(id);
    }
    //This is used to Get the TouchLayer with the layerId
    private static TouchLayer GetTouchLayerWithID(int id)
    {
        TouchLayer out_layer = new TouchLayer();
        for (int i = 0; i < TouchLayers.Count; i++)
            if (TouchLayers[i].id == id)
                out_layer = TouchLayers[i];
        return out_layer;
    }
    //This is used to Query Whether the Current Layerid is Already Exists or Not
    //If this Exists the new TouchEvent with the same Id will be add to the Layer
    private static bool IsAlreadyRegisteredLayer(int id)
    {
        for (int i = 0; i < TouchLayers.Count; i++)
        {
            if (TouchLayers[i].id == id)
                return true;
        }
        return false;
    }
    //This is used to Initialize the layers during the First Update Call
    private static void InitializeRegisteredEvents()
    {
        IsInitialized = true;
        ArrTouchLayers = TouchLayers.ToArray();
        layer_count = TouchLayers.Count;
        SortTouchLayer();
        if (RegisteredEvents.Count != TouchEvent.EventCount)
            Debug.Log("<color=yellow>Warning : Some touch Events aren't registered</color>");
    }
    private static void SortTouchLayer()
    {

        for (int i = 1; i < layer_count; i++)
            for (int j = 0; j < (layer_count - i); j++)
                if (ArrTouchLayers[j].id < ArrTouchLayers[j + 1].id)
                {
                    TouchLayer layer = ArrTouchLayers[j];
                    ArrTouchLayers[j] = ArrTouchLayers[j + 1];
                    ArrTouchLayers[j + 1] = layer;
                }
    }
    //This is Very Important , the Core of the TouchManager
    private static void HandleTouchAssignments()
    {
        if (IsAnyChange)
            for (int i = 0; i < num_touches; i++)
            {
                Touch touch = touches[i];
                for(int j =0; j < layer_count;  j++)
                {

                    int eventCount = ArrTouchLayers[j].touchEvents.Count;
                    TouchEvent[] touchEvents = ArrTouchLayers[j].touchEvents.ToArray();
                    for (int k = 0; k < eventCount; k++)
                    {
                        if (!IsTouched(touchEvents[k]) && !isReserved(touch.fingerId) && touchEvents[k].Condition(touch.position - screenSize * 0.5f) == true)
                        {
                            touchEvents[k].touch = touch;
                            touchEvents[k].OnBegan();
                            ReserveID(touch.fingerId);
                        }
                    }
                }
            }
        for (int j = 0; j < layer_count; j++ )
        {
            int eventCount = ArrTouchLayers[j].touchEvents.Count; 
            TouchEvent[] touchEvents = ArrTouchLayers[j].touchEvents.ToArray() ;
            for (int k = 0; k < eventCount; k++ )
            {
                TouchEvent touchEvent = touchEvents[k];
                if (touchEvent.touch.fingerId != -1)
                {
                    touchEvent.touch = GetTouchWithID(touchEvent.touch.fingerId);
                    if (touchEvent.touch.phase == TouchPhase.Moved)
                        touchEvent.OnMoved();
                    else if (touchEvent.touch.phase == TouchPhase.Stationary)
                        touchEvent.OnStationary();
                    else if (touchEvent.touch.phase == TouchPhase.Ended || touchEvent.touch.phase == TouchPhase.Canceled)
                    {
                        if (!touchEvent.IsCallOnEndedAfterTouchDataLost)
                            touchEvent.OnEnded();
                        UnReserveID(touchEvent.touch.fingerId);
                        touchEvent.touch = new Touch();
                        touchEvent.touch.fingerId = -1;
                        if (touchEvent.IsCallOnEndedAfterTouchDataLost)
                            touchEvent.OnEnded();
                    }
                }
            }
        }
    }
    //It Takes the Input from the Device
    private static void ManageRawTouchInput()
    {
        touches = Input.touches;
        if (isCleared && Input.touchCount != 0)
            isCleared = false;
        if (!isCleared && Input.touchCount == 0)
        {
            ReservedFingerIDs.Clear();
            isCleared = true;
        }
        if (num_touches != Input.touchCount)
        {
            num_touches = Input.touchCount;
            IsAnyChange = true;
        }
        else
            IsAnyChange = false;
    }


    private void Awake()
    {
        if (ReservedFingerIDs == null)
            ReservedFingerIDs = new List<int>();
        DontDestroyOnLoad(this.gameObject);
        isCleared = true;
        IsInitialized = false;
    }
    private void Update()
    {
        if (!IsInitialized)
            InitializeRegisteredEvents();
        ManageRawTouchInput();
        HandleTouchAssignments();
    }
}
