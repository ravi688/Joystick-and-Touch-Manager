
using System.Collections.Generic;


public class TouchLayer
{
    public int id;                                      //Id of this layer
                                     
    public List<TouchEvent> touchEvents;                //no of Touch Event Registered to this Layer
    public TouchLayer()
    {
        id = 0;
        touchEvents = new List<TouchEvent>();
    }
}
