using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VISUALNOVEL;

public class ScriptUpdater : MonoBehaviour
{
    [SerializeField]
    private TextAsset file;
    private const string ASSET_FORMAT = "txt";
    private string filePath => $"{Application.dataPath}/{FilePaths.main_path}{FilePaths.dialogue_path}{file.name}.{ASSET_FORMAT}";
    private FileSystemWatcher watcher;
    // Start is called before the first frame update
    void Start()
    {
        CheckForScriptChanges();

        watcher = new FileSystemWatcher();
        watcher.Path = Path.GetDirectoryName(filePath);
        watcher.Filter = Path.GetFileName(filePath);
        watcher.Changed += OnFileChanged;
        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        VNGameSave.reload = true;
        Debug.Log("see changes!");
    }

    private void CheckForScriptChanges()
    {
        var currentScriptState = new ScriptState
        {
            fileName = file.name,
            lastModified = File.GetLastWriteTime(filePath).ToString()
        };

        var savedScriptStates = ScriptStateChecker.LoadScriptState();
        var savedScriptState = savedScriptStates.Find(s => s.fileName == file.name);

        if (savedScriptState == null || savedScriptState.lastModified != currentScriptState.lastModified)
        {
            VNGameSave.reload = true;
            Debug.Log("Script has changed since the last run!");

            // Update the saved state
            if (savedScriptState != null)
            {
                savedScriptState.lastModified = currentScriptState.lastModified;
            }
            else
            {
                savedScriptStates.Add(currentScriptState);
            }
            ScriptStateChecker.SaveScriptState(savedScriptStates);
        }
    }
}
