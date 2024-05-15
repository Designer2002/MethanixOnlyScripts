using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace COMMANDS
{
    public class CommandProcess
    {
        public Guid ID;
        public string processName;
        public Delegate command;
        public string[] args;
        public UnityEvent OnTerminateAction;
        public CoroutineWrapper runningProcess;

        public CommandProcess(Guid id, string processName, Delegate command, CoroutineWrapper runningProcess, string[] args, UnityEvent onTerminate)
        {
            ID = id;
            this.runningProcess = runningProcess;
            this.processName = processName;
            this.command = command;
            this.args = args;
            this.OnTerminateAction = onTerminate;
        }

    }
}