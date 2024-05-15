using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DIALOGUE
{
    public class DL_COMMAND_DATA
    {
        public List<Command> commands;
        private const char COMMAND_SPLITTER_ID = ',';
        private const char ARGUMENT_SPLITTER_ID = '(';
        private const string WAIT_COMMAND_ID = "[wait]";
        public struct Command
        {
            public string name;
            public string[] arguments;
            public bool waitForCompletion;
        }
        public DL_COMMAND_DATA(string rawCommands)
        {
            commands = RipCommands(rawCommands);
        }
        private List<Command> RipCommands(string rawCommands)
        {
            string[] data = rawCommands.Split(COMMAND_SPLITTER_ID, (char)System.StringSplitOptions.RemoveEmptyEntries);
            List<Command> result = new List<Command>();
            foreach (string cmd in data)
            {
                Command command = new Command();
                int index = cmd.IndexOf(ARGUMENT_SPLITTER_ID);
                command.name = cmd.Substring(0, index).Trim();

                if (command.name.ToLower().StartsWith(WAIT_COMMAND_ID))
                {
                    command.waitForCompletion = true;
                    command.name = command.name.Substring(WAIT_COMMAND_ID.Length);

                }
                else
                {
                    command.waitForCompletion = false;
                }

                command.arguments = GetArgs(cmd.Substring(index + 1, cmd.Length - index - 2));
                result.Add(command);
            }

            return result;
        }

        private string[] GetArgs(string args)
        {
            List<string> argumentList = new List<string>();
            StringBuilder currentArg = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (!inQuotes && args[i] == ' ')
                {
                    argumentList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }
                currentArg.Append(args[i]);
            }
            if (currentArg.Length > 0)
            {
                argumentList.Add(currentArg.ToString());
            }
            return argumentList.ToArray();
        }
    }
}