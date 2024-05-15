using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using VISUALNOVEL;
using System;
using System.Text;

public class FileManager
{
    private const string key = "MEFOVARKA";
    public static List<string> ReadTextFile(string filePath, bool includeBlankLines = true)
    {
        if (!filePath.StartsWith("/"))
        {
            filePath = FilePaths.root + filePath;
        }
        List<string> lines = new List<string>();
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (includeBlankLines || !string.IsNullOrWhiteSpace(line)) lines.Add(line);

                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogError($"file not found: {ex.FileName}");
        }
        return lines;

    }

    public static T Load<T>(string filePath, bool encrypt = false)
    {
        if(File.Exists(filePath))
        {
            if(encrypt)
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] decryptedData = XOR(encryptedData, keyBytes);
                string decryptedString = Encoding.UTF8.GetString(decryptedData);
                return JsonUtility.FromJson<T>(decryptedString);
            }
            else
            {
                string JSONdata = File.ReadAllLines(filePath)[0];
                return JsonUtility.FromJson<T>(JSONdata);
            }
        }
        else
        {
            Debug.LogError($"file {filePath} doesnt exist ");
            return default(T);
        }
    }

    public static List<string> ReadTextAsset(string filePath, bool includeBlankLines = true)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(filePath);
        if (textAsset == null)
        {
            Debug.LogError($"Asset not found: {filePath}");
            return null;
        }
        return ReadTextAsset(textAsset, includeBlankLines);

    }
    public static List<string> ReadTextAsset(TextAsset asset, bool includeBlankLines = true)
    {
        List<string> lines = new List<string>();
        using (StringReader reader = new StringReader(asset.text))
        {
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (includeBlankLines || !string.IsNullOrWhiteSpace(line)) lines.Add(line);
            }

        }
        //Debug.Log(asset.name);
        //string line = File.ReadAllText($"Assets/_MAIN_/Resources/{asset.name}.txt", System.Text.Encoding.UTF8);
        //var linesar = line.Split('\n');
        //foreach(var l in linesar)
        //{
        //    lines.Add(l);
        //}
        //Debug.Log("text - " + lines[1]);
        return lines;
    }

    public static bool TryCreateDirectoryFromPath(string path)
    {
        if (Directory.Exists(path) || File.Exists(path)) return true;
        if(path.Contains("."))
        {
            path = Path.GetDirectoryName(path);
            if (Directory.Exists(path)) return true;
        }
        if (path == string.Empty) return false;
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not create directory {e}");
            return false;
        }
    }

    public static void Save(string filePath, string JSONdata, bool encrypt = false)
    {
        if(!TryCreateDirectoryFromPath(filePath))
        {
            Debug.LogError($"FAILED TO SAVE FILE {filePath}!!!");
            return;
        }
        if(encrypt)
        {
            byte[] databytes = Encoding.UTF8.GetBytes(JSONdata);
            byte[] keybytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedbytes = XOR(databytes, keybytes);
            File.WriteAllBytes(filePath, encryptedbytes);
        }
        else
        {
            StreamWriter sw = new StreamWriter(filePath);
            sw.Write(JSONdata);
            sw.Close();
        }
        

        Debug.Log("saved data to path!");
    }

    private static byte[] XOR(byte[] input, byte[] key)
    {
        byte[] output = new byte[input.Length];
        for(int i = 0; i < input.Length; i++)
        {
            output[i] = (byte)(input[i] ^ key[i % key.Length]);
        }
        return output;
    }
}


