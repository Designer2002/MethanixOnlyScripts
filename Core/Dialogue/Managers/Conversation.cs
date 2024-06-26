using System.Collections.Generic;

namespace DIALOGUE
{
    public class Conversation
    {
        public event System.EventHandler OnProgressStarted;
        private List<string> lines = new List<string>();
        private int progress = 0;

        public string file { get; private set; }
        public int fileStartIndex { get; private set; }

        public int fileEndIndex { get; private set; }

        public Conversation( List<string> lines,int progress = 0, string file = "", int fileStartIndex = -1, int fileEndIndex  = -1)
        {
            this.lines = lines;
            this.progress = progress;
            this.file = file;
            if(fileStartIndex == -1)
            {
                fileStartIndex = 0;
            }
            if (fileEndIndex == -1)
            {
                fileEndIndex = lines.Count - 1;
            }
            this.fileStartIndex = fileStartIndex;
            this.fileEndIndex = fileEndIndex;
            OnProgressStarted += SetLocationManagerStatusToTop;
        }

 

        public int FindProgress(string line)
        {
            int idx = 0;
            foreach(var l in GetLines())
            {
                string lt = l;
                while (!l.Contains("\t"))
                    lt = l.Trim('\t');
                if (lt.Contains(line)) return idx;
                idx++;
            }
            return -1;
        }
        public int GetProgress() => progress;
        public void SetProgress(int value) => progress = value;
        public void IncrementProgress()
        {
            OnProgressStarted?.Invoke(this, System.EventArgs.Empty);
            OnProgressStarted -= SetLocationManagerStatusToTop;
            progress++;
        }
        
        public int Count() => lines.Count;
        public List<string> GetLines() => lines;
        public string currentLine() => lines[progress];
        public string nextLine() => lines[progress + 1];
        public bool HasReachedEnd() => progress >= lines.Count;

        public void SetLocationManagerStatusToTop(object sender, System.EventArgs e)
        {
            if (LOCATIONS.LocationManager.instance.goal != null)
            {
                LOCATIONS.LocationManager.instance.Status = LOCATIONS.LocationManager.ConversationStatus.Top;
                UnityEngine.Debug.Log("TOP!!!");
            }
        }

        public bool TryStepNext()
        {
            if (progress < lines.Count - 1) return true;
            return false;
        }
    }
}