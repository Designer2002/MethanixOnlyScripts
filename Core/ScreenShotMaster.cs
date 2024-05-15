using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotMaster : MonoBehaviour
{
    public static Texture2D CaptureScreenshot(int width, int height, float supersize = 1, string filePath = "") => CaptureScreenshot(cam: Camera.main, width, height, supersize, filePath);
  

    public static Texture2D CaptureScreenshot(Camera cam, int width, int height, float supersize = 1, string filePath = "")
    {
        if(supersize != 1)
        {
            width = Mathf.RoundToInt(width * supersize);
            height = Mathf.RoundToInt(height * supersize);
        }

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 32);
        cam.targetTexture = rt;

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);

        cam.Render();

        RenderTexture.active = rt;

        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        cam.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        if(filePath != "")
        {
            SaveScreenshotToFile(screenshot, filePath);
        }
        else
        {
            Debug.Log("nowhere");
        }
        return screenshot;
    }
    public enum ImageType { PNG, JPG }
    public static void SaveScreenshotToFile(Texture2D screenshot, string filepath, ImageType filetype = ImageType.JPG)
    {
        byte[] bytes = new byte[0];
        string extension = "";
        switch(filetype)
        {
            case ImageType.PNG:
                bytes = screenshot.EncodeToPNG();
                extension = ".png";
                break;
            case ImageType.JPG:
                bytes = screenshot.EncodeToJPG();
                extension = ".jpg";
                break;
        }


        if(!filepath.Contains(".")) filepath = filepath + extension;
        System.IO.File.WriteAllBytes(filepath, bytes);

    }
}
