using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMANDS
{
    public abstract class CMD_DatabseExtensions
    {
        public static void Extend(CommandDatabase database)
        {

        }
        public static CommandParameters ConvertDataToParameters(string[] data, int startingIndex = 0)
        {
            return new CommandParameters(data, startingIndex);
        }
    }
}