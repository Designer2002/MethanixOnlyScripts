using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GoalData
{
    public string currentLocation;
    public bool isActiveGoal = false;
    public string targetLocation;
    public bool isFinished;
    public bool isFailed;
    public int playerMoves;
    public int availibleMoves;
    public bool isAgile;
    public int convProgress;
    public string OldLocation;
    public string dataJSON;
    public int GoalAfter;
    public int GoalStart;
    public int GoalEnd;
    public static GoalData Capture()
    {
        GoalData entry = new GoalData();
        if (LOCATIONS.LocationManager.instance.goal == null) return entry;
        entry.isActiveGoal = true;
        entry.currentLocation = LOCATIONS.LocationManager.instance.goal.currentLocation;
        entry.availibleMoves = LOCATIONS.LocationManager.instance.goal.AvailibleMoves;
        entry.targetLocation = LOCATIONS.LocationManager.instance.goal.targetLocation;
        entry.isAgile = LOCATIONS.LocationManager.instance.goal.isAgile;
        entry.isFailed = LOCATIONS.LocationManager.instance.goal.isFailed;
        entry.isFinished = LOCATIONS.LocationManager.instance.goal.IsFinished;
        entry.playerMoves = LOCATIONS.LocationManager.instance.goal.PlayerMoves;
        entry.convProgress = LOCATIONS.LocationManager.instance.convProgress;
        entry.GoalAfter = LL_Goal.After;
        entry.GoalEnd = LL_Goal.Ending;
        entry.GoalStart = LL_Goal.Starting;
        entry.OldLocation = LOCATIONS.LocationManager.instance.OldLocation;

        entry.dataJSON = JsonUtility.ToJson(entry);

        return entry;
    }
    public static void Apply(GoalData gdata)
    {
        LOCATIONS.LocationGoal goal = new LOCATIONS.LocationGoal();
        
        var data = JsonUtility.FromJson<GoalData>(gdata.dataJSON);
        if (data == null)
        {
            LOCATIONS.LocationExpander.instance.Hide();
            DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
            DIALOGUE.DialogueSystem.instance.EnableGlobalReading();
            return;
        }
        
        goal.AvailibleMoves = data.availibleMoves;
        goal.isAgile = data.isAgile;
        goal.PlayerMoves = data.playerMoves;
        goal.targetLocation = data.targetLocation;
        goal.convProgress = data.convProgress;
        LL_Goal.Starting = data.GoalStart;
        LL_Goal.Ending = data.GoalEnd;
        LL_Goal.After = data.GoalAfter;

        LOCATIONS.LocationManager.instance.OldLocation = data.OldLocation;
        
        LOCATIONS.LocationManager.instance.goal = goal;

        LOCATIONS.LocationManager.instance.GoalInProgress();
        
    }
}
