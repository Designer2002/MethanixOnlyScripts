using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableStore
{
    public delegate void ValueChangedEvent(object sender, ValueChangedEventArgs e);
    public delegate void ValueEvaluatedEvent(object sender, ValueEvaluatingEventArgs e);
    public static event ValueChangedEvent OnValueChanged;
    public static event ValueEvaluatedEvent OnValueEvaluated;
    public static readonly string REGEX_VARIABLE_IDS = @"[!]?\$[a-zA-Z0-9_.]+";
    private const string DEFAULT_NAME = "Default";
    public const char DTABASE_VARIABLE_SPLITTER = '.';
    public const char VARIABLE_ID = '$';
    public class Database
    {
        public Dictionary<string, Variable> variables;
        public Database(string name)
        {
            this.name = name;
            variables = new Dictionary<string, Variable>();
        }
        public string name;
    }
    public abstract class Variable
    {
        public abstract object Get();
        public abstract void Set(object valuea);
    }
    public class Variable<T> : Variable
    {
        private T value;
        private Func<T> getter;
        private Action<T> setter;

        public Variable(T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
        {
            value = defaultValue;
            if(getter == null)
            {
                this.getter = () => value;
            }
            else
            {
                this.getter = getter;
            }
            if(setter == null)
            {
                this.setter = newValue => value = newValue;
            }
            else
            {
                this.setter = setter;
            }
        }
        public override object Get() => getter();

        public override void Set(object type) => setter((T)type);
    }

    public static void AddEvent(object a, object b)
    {
        VariableStore.OnValueEvaluated?.Invoke(GetDatabase(DEFAULT_NAME), new ValueEvaluatingEventArgs { ValueA = a, ValueB = b });
    }

    public static Dictionary<string, Database> databases = new Dictionary<string, Database>() { { DEFAULT_NAME,  new Database(DEFAULT_NAME) } };
    private static Database defaultDatabase => databases[DEFAULT_NAME];

    public static bool CreateDatabase(string name)
    {
        if(!databases.ContainsKey(name))
        {
            databases[name] = new Database(name);
            return true;
        }
        return false;
    }

    public static Database GetDatabase(string name = "")
    {
        if (name == string.Empty) return defaultDatabase;
        if (!databases.ContainsKey(name)) CreateDatabase(name);
        return databases[name];
    }

    public static void PrintAllDatabases()
    {
        foreach (KeyValuePair<string, Database> dbEntry in databases)
        {
            Debug.Log($"Database: {dbEntry.Key}");
        }
    }

    public static void PrintAllDatabases(Database database = null)
    {
        foreach (KeyValuePair<string, Database> dbEntry in databases)
        {
            Debug.Log($"Database: {dbEntry.Key}");
        }
    }

    public static void PrintAll(Database database = null)
    {
        if(database !=null)
        {
            PrintAllVariables(database);
            return;
        }
        foreach(var dbEntry in databases)
        {
            PrintAllVariables(dbEntry.Value);
        }
    }

    public static void PrintAllVariables(Database database = null)
    {
        foreach (KeyValuePair<string, Variable> variablePair in database.variables)
        {
            string variableName = variablePair.Key;
            object variableValue = variablePair.Value.Get();
            Debug.Log($"DB - {database.name}, variable - {variableValue}, type - {variableValue.GetType()}");
        }
    }

    public static bool TryGetValue(string name, out object variable)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);
        if(!db.variables.ContainsKey(variableName))
        {
            variable = null;
            return false;
        }
        variable = db.variables[variableName].Get();
        return true;
    }

    public static bool TrySetValue<T>(string name, T value, bool change = true, bool create = false)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);
        if (!db.variables.ContainsKey(variableName))
        {
            if (!create) return false;
            else CreateVariable(name, value);
        }
        db.variables[variableName].Set(value);
        if (change) OnValueChanged?.Invoke(TryGetValue(variableName, out object myVar), new ValueChangedEventArgs { Value = value });
        return true;
    }

    public static bool CreateVariable<T>(string name, T defaultValue, Func<T> getter = null, Action<T> setter = null)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);
        if (db.variables.ContainsKey(variableName)) return false;
        db.variables[variableName] = new Variable<T>(defaultValue, getter, setter);
        return true;
    }

    private static (string[], Database, string) ExtractInfo(string name)
    {
        string[] parts = name.Split(DTABASE_VARIABLE_SPLITTER);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]) : defaultDatabase;
        string variableName = parts.Length > 1 ? parts[1] : parts[0];
        return (parts, db, variableName);
    }

    public static bool HasVariable(string name)
    {
        string[] parts = name.Split(DTABASE_VARIABLE_SPLITTER);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]) : defaultDatabase;
        string variableName = parts.Length > 1 ? parts[1] : parts[0];
        return db.variables.ContainsKey(name);
    }

    public static void RemoveAllVariables()
    {
        databases.Clear();
        databases[DEFAULT_NAME] = new Database(DEFAULT_NAME);
    }

    public static void RemoveVariable(string name)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);
        if (db.variables.ContainsKey(variableName)) db.variables.Remove(variableName);
    }

    public class ValueChangedEventArgs : EventArgs
    { 
        public object Value { get; set; }
    }

    public class ValueEvaluatingEventArgs : EventArgs
    {
        public object ValueA { get; set; }
        public object ValueB { get; set; }
    }
}
