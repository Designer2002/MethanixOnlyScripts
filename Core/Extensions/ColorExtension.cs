using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorExtension
{
    public static Color SetAlpha(Color origignal, float alpha) => new Color(origignal.r, origignal.b, origignal.b, alpha);

    public static Color ColorFromName(string name)
    {
        switch(name)
        {
            case "red":
                return Color.red;
            case "green":
                return Color.green;
            case "blue":
                return Color.blue;
            case "yellow":
                return Color.yellow;
            case "white":
                return Color.white;
            case "black":
                return Color.black;
            case "gray":
                return Color.gray;
            case "cyan":
                return Color.cyan;
            case "magenta":
                return Color.magenta;
            case "orange":
                return new Color(1, 0.5f, 0);
            default:
                return ColorFromHex(name);
        }
    }

    public static Color ColorFromHex(string hex)
    {
        Color returnValue = Color.clear;
        if (string.IsNullOrWhiteSpace(hex)) return returnValue;
        try
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.Message);
            return returnValue;
        }
        return returnValue;
    }


}
