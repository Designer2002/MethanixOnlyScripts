using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDesigner : MonoBehaviour
{
    public static PanelDesigner instance;
    System.Random r = new System.Random();

    private void Awake()
    {
        instance = this;
    }

    public Color32 GetBrightColor(byte a = 255)
    {
        int red = 0, blue = 0, green = 0;
        while (red + blue + green < 500)
        {
            red = r.Next(0, 255);
            blue = r.Next(0, 255);
            green = r.Next(0, 255);
        }
        return new Color32((byte)red, (byte)blue, (byte)green, a);
    }
}
