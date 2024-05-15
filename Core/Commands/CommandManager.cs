using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using COMMANDS;
using UnityEngine.Events;

public class CommandManager : MonoBehaviour
{
    private const char SUB_COMMAND_IDENTIFIER = '.';
    public const string DATABASE_CHARACTER_BASE = "characters";
    public const string DATABASE_CHARACTER_SPRITE = "sprite";
    public const string DATABASE_MISSIONS = "mission";
    public static CommandManager instance { get; private set; }
    private CommandDatabase database;
    private Dictionary<string, CommandDatabase> ssubDatabases = new Dictionary<string, CommandDatabase>();

    private List<CommandProcess> activeProcesses = new List<CommandProcess>();
    private CommandProcess topProcess => activeProcesses.FirstOrDefault();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            database = new CommandDatabase();
            

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] extensionTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CMD_DatabseExtensions))).ToArray();
            foreach (Type extension in extensionTypes)
            {
                MethodInfo extendMethod = extension.GetMethod("Extend");
                extendMethod.Invoke(null, new object[] { database });

            }
        }
        else DestroyImmediate(gameObject);
    }
    public CoroutineWrapper Execute(string commandName, params string[] args)
    {
        if (commandName.Contains(SUB_COMMAND_IDENTIFIER)) return ExecuteSubCommand(commandName, args);
        Delegate command = database.GetCommand(commandName);
        if (command == null)
        {
            Debug.Log("NUUULLLLLL");
            return null;
        }
        return StartProcess(commandName, command, args);
    }

    private CoroutineWrapper ExecuteSubCommand(string commandName, string[] args)
    {
        string[] parts = commandName.Split(SUB_COMMAND_IDENTIFIER);
        string dbName = string.Join(SUB_COMMAND_IDENTIFIER.ToString(), parts.Take(parts.Length - 1));
        string subCommandName = parts.Last();
        if(ssubDatabases.ContainsKey(dbName))
        {
            Debug.Log($"name - {dbName}, cmd - {subCommandName}");
            Delegate command = ssubDatabases[dbName].GetCommand(subCommandName);
            if (command != null)
            {
                return StartProcess(commandName, command, args);
            }
            else return null;
        }
        string charName = dbName;
        if(CHARACTERS.CharacterManager.instance.HasCharacter(dbName))
        {
            List<string> newargs = new List<string>(args);
            newargs.Insert(0, charName);
            args = newargs.ToArray();

            return ExecuteCommand(subCommandName, mis:false, ch:true, args);
        }
        if (MISSIONS.MissionManager.instance.HasMission(dbName))
        {
            List<string> newargs = new List<string>(args);
            newargs.Insert(0, charName);
            args = newargs.ToArray();

            return ExecuteCommand(subCommandName, mis: true, ch: false, args);
        }
        Debug.Log($"NO SUBDB CALLED {dbName} EXISTS");
        return null;
    }

    private CoroutineWrapper ExecuteCommand(string CommandName, bool mis = false, bool ch = false, params string[] args)
    {
        Delegate command = null;
        CommandDatabase db = ssubDatabases[DATABASE_CHARACTER_BASE];
        if (db.HasCommand(CommandName))
        {
            command = db.GetCommand(CommandName);
            return StartProcess(CommandName, command, args);
        }
        if(ch)
        {
            CHARACTERS.CharacterConfigData config = CHARACTERS.CharacterManager.instance.GetCharacterConfig(args[0]);
            switch (config.characterType)
            {
                case CHARACTERS.Character.CharacterType.Sprite:
                case CHARACTERS.Character.CharacterType.SpriteSheet:
                    db = ssubDatabases[DATABASE_CHARACTER_SPRITE];
                    command = db.GetCommand(CommandName);
                    break;
            }
        }
        if (mis)
        {

            MISSIONS.MissionConfigData config = MISSIONS.MissionManager.instance.GetMissionConfig(args[0]);
            db = ssubDatabases[DATABASE_MISSIONS];
            command = db.GetCommand(CommandName);

        }
        command = db.GetCommand(CommandName);
        if (command != null) return StartProcess(CommandName, command, args);
        Debug.LogError("NULL SHIT");
        return null;
    }

    private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
    {
        System.Guid processID = Guid.NewGuid();
        CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
        activeProcesses.Add(cmd);
        Coroutine c = StartCoroutine(RunningProcess(cmd));
        cmd.runningProcess = new CoroutineWrapper(this, c);
        return cmd.runningProcess;
    }

    public void KillProcess(CommandProcess cmd)
    {
        activeProcesses.Remove(cmd);
        if(cmd.runningProcess != null & !cmd.runningProcess.isDone)
        {
            cmd.runningProcess.Stop();
        }
        cmd.OnTerminateAction?.Invoke();
    }

    public void StopCurrentProcess()
    {
        if (topProcess != null) KillProcess(topProcess);
    }

    public void StopAllProcess()
    {
        foreach (var ap in activeProcesses)
        {
            if(ap.runningProcess != null && !ap.runningProcess.isDone)
            {
                ap.runningProcess.Stop();
            }
            ap.OnTerminateAction?.Invoke();
        }
        activeProcesses.Clear();
    }

    private IEnumerator RunningProcess(CommandProcess process)
    {
        yield return WaitingToProcessToComplete(process.command, process.args);
        KillProcess(process);
    }
    private IEnumerator WaitingToProcessToComplete(Delegate command, string[] args)
    {
        if (command is Action) command.DynamicInvoke();

        else if (command is Action<string>) command.DynamicInvoke(args.Length == 0 ? string.Empty : args[0]);

        else if (command is Action<string[]>) command.DynamicInvoke((object)args);

        else if (command is Func<IEnumerator>) yield return ((Func<IEnumerator>)command)();

        else if (command is Func<string, IEnumerator>) yield return ((Func<string, IEnumerator>)command)(args.Length == 0 ? string.Empty : args[0]);

        else if (command is Func<string[], IEnumerator>) yield return ((Func<string[], IEnumerator>)command)(args);
    }
    public void AddTerminationActionToCurrentProcess(UnityAction action)
    {
        CommandProcess process = topProcess;
        if (process == null)
        {
            Debug.LogWarning("NULL PROCESSSSSSS");
            return;
        }
        process.OnTerminateAction = new UnityEvent();
        process.OnTerminateAction.AddListener(action);
    }

    public CommandDatabase CreateDatabse(string name)
    {
        name = name.ToLower();
        if (ssubDatabases.TryGetValue(name, out CommandDatabase db))
        {
            Debug.LogWarning("already here");
            return db;   
        }
        CommandDatabase newdb = new CommandDatabase();
        ssubDatabases.Add(name, newdb);
        return newdb;
    }
}
