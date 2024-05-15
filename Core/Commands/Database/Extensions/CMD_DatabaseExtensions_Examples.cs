using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public class CMD_DatabaseExtensions_Examples : CMD_DatabseExtensions
    {
        new public static void Extend(CommandDatabase database)
        {
            //add action no params
            database.AddCommand("print", new Action(PrintDefaultMessage));
            database.AddCommand("print_1p", new Action<string>(PrintUserMessage));
            database.AddCommand("print_mp", new Action<string[]>(PrintLines));
            //add lambda no params
            database.AddCommand("lambda", new Action(() => { Debug.Log("printing by LAMBDA"); }));
            database.AddCommand("lambda_1p", new Action<string>((message) => { Debug.Log($"printing '{message}' by LAMBDA"); }));
            database.AddCommand("lambda_mp", new Action<string[]>((args) => { Debug.Log($"printing '{string.Join(", ", args)}' by LAMBDA"); }));
            //add coroutines
            database.AddCommand("process", new Func<IEnumerator>(SimpleProcess));
            database.AddCommand("process_1p", new Func<string, IEnumerator>(DataProcess));
            database.AddCommand("process_mp", new Func<string[], IEnumerator>(LinesProcess));

            //special Example
            database.AddCommand("move_demo", new Func<string, IEnumerator>(MoveCharacterDemo));
        }
        private static void PrintDefaultMessage()
        {
            Debug.Log("Printing default message");
        }

        private static void PrintUserMessage(string message)
        {
            Debug.Log($"user message: {message}");
        }
        private static void PrintLines(string[] lines)
        {
            int i = 1;
            foreach (string line in lines)
            {
                Debug.Log($"{i++}. {line}");
            }

        }
        private static IEnumerator SimpleProcess()
        {
            for (int i = 1; i <= 5; i++)
            {
                Debug.Log($"Process {i} is running");
                yield return new WaitForSeconds(1);
            }
        }

        private static IEnumerator DataProcess(string data)
        {
            if (int.TryParse(data, out int num))
            {
                for (int i = 1; i <= num; i++)
                {
                    Debug.Log($"Process {i} is running");
                    yield return new WaitForSeconds(1);
                }
            }
        }

        private static IEnumerator LinesProcess(string[] data)
        {
            foreach (string line in data)
            {

                Debug.Log($"Process message: {line}");
                yield return new WaitForSeconds(0.5f);
            }
        }

        private static IEnumerator MoveCharacterDemo(string direction)
        {
            bool left = direction.ToLower() == "left";
            Transform character = GameObject.Find("Dave").transform;
            if (character == null) Debug.LogError("NO AMOGUS!");
            float movespeed = 145;

            float targetX = left ? 0 : 521;
            float currentX = character.position.x;
            Debug.Log($"current: {currentX}\ntarget: {targetX}");
            while (Mathf.Abs(targetX - currentX) > 0.1f)
            {
                // Debug.Log($"Moving character to {(left ? "left" : "right")} [{currentX}/{targetX}]");
                currentX = Mathf.MoveTowards(currentX, targetX, movespeed * Time.deltaTime);
                character.position = new Vector3(currentX, character.position.y, character.position.z);
                yield return null;
            }
        }
    }
}