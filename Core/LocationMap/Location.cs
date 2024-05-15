using System;
using System.Collections.Generic;

namespace LOCATIONS
{
    public class Location
    {
        public string Description;
        public string CodeWord;
        public string Name;
        public List<string> Neighbours;
        public OpenStatus Status 
        {
            get
            {
                if (IsHere) _status = OpenStatus.Here;
                else if (IsLocked) _status = OpenStatus.Unaccessable;
                else if (!IsHere && IsAvailible) _status = OpenStatus.Opened;
                else _status = OpenStatus.Closed;
                return _status;
            } 
            set 
            { 
                _status = value; 
            } 
        }
        private OpenStatus _status;
        public bool IsAvailible => Neighbours.Contains(LocationManager.instance.currentLocation);
        public bool IsLocked { get; private set; }
        public bool IsHere { get; private set; }

        public enum OpenStatus
        {
            Here,
            Opened,
            Closed,
            Unaccessable
        }

        public Location(LocationConfigData config)
        {
            this.Description = config.Description;
            this.CodeWord = config.CodeWord;
            this.Name = config.Name;
            this.Neighbours = config.Neighbours;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Stay()
        {
            IsHere = true;
        }

        public void Leave()
        {
            IsHere = false;
        }

        internal void Unlock()
        {
            IsLocked = false;
        }
    }
}