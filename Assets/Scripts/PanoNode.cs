using UnityEngine;

public class PanoNode : MonoBehaviour {
    public string areaName;
    public Texture2D frame;
    public PanoNode[] neighbors;
    public float[] neighborYaw;   // 0 = ahead, 180 = back, negative = left, positive = right
}
