using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE.Logical_Lines
{
    public interface ILogicalLine
    {
        string KeyWord { get; }
        bool Matches(DIALOGUE_LINE line);
        IEnumerator Execute(DIALOGUE_LINE data);

    }
}