
using UnityEngine;


[CreateAssetMenu]
public class JoyStickSettings : ScriptableObject
{
    public Sprite HandleSprite;
    public Sprite BodySprite;
    public Vector2 BodySize = new Vector2(256, 256);
    public Vector2 HandleSize = new Vector2(256, 256);
    public bool IsInteractive = true;
    public float Sensitivity = 1.0f;
    public float HandleReturnSpeed = 2.0f;
    public int Threshold_distance = 10;
    public Vector2 position = new Vector2(500, -250);
}
