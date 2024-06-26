using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LOCATIONS
{
    public class LocationManager : MonoBehaviour
    {
        public static LocationManager instance;
        public static Dictionary<string, Location> locations = new Dictionary<string, Location>();
        public LocationConfigurationSO config;
        public LocationGoal goal;
        private GRAPHICS.GraphicLayer gl;
        public int convProgress;
        public int RollBackProgress { get; set; }
        public string OldLocation { get; set; }
        public DIALOGUE.Conversation OldConversation;
        public ConversationStatus Status { get; set; }
        
        public enum ConversationStatus { Top, Process, End }
        
        public string currentLocation { get; private set; }
        private void Awake()
        {  
            if (instance != this)
            {
                Destroy(instance);
            }
            instance = this;
        }

        private string VariableCheckingLocation = "";

        private void Init()
        {
            CreateLocation("base");
            CreateLocation("entrance");
            CreateLocation("factory");
            CreateLocation("florarium");
            CreateLocation("stadium");
            CreateLocation("street");
            CreateLocation("flystation");
            CreateLocation("larsroom");
            CreateLocation("jamesroom");
            CreateLocation("kitchen");
            CreateLocation("hallway");
            CreateLocation("hall");
            CreateLocation("engineroom");
            CreateLocation("basement");
            CreateLocation("corridor");
            CreateLocation("toilet");
            CreateLocation("workplace");
            VariableStore.CreateVariable("location", "base");
            currentLocation = "base";
            GetLocation(currentLocation).Stay();
        }

        private void Start()
        {
            Init();
            GetLocation("jamesroom").Lock();
            VariableStore.CreateVariable("overmove", false);
            VariableStore.OnValueChanged += OnLocationChanged;
            VariableStore.OnValueEvaluated += LocationIsBeingChecked;
            gl = GRAPHICS.GraphicPanelManager.instance.GetPanel("background").GetLayer(0, true);
            //OnCreatingGoal += DisablingParser;
            //OnKillingGoal += EnablingParser;
        }

        private void LocationIsBeingChecked(object sender, VariableStore.ValueEvaluatingEventArgs e)
        {
            if (locations.ContainsKey(e.ValueA.ToString()))
            {
                //Debug.Log($"check location, {e.ValueA.ToString()} == {e.ValueB.ToString()} - {e.ValueA.ToString() == e.ValueB.ToString()}");
                VariableCheckingLocation = e.ValueB.ToString();
                if(goal!=null && instance == this)StartCoroutine(goal.Pause(e.ValueA.ToString() == e.ValueB.ToString()));
            }
        }

        private void OnLocationChanged(object sender, VariableStore.ValueChangedEventArgs e)
        {
            if (goal != null)
            {
                LL_Goal.LoopProgress();

            }
            if (instance == this && locations.ContainsKey(e.Value.ToString()) && currentLocation != e.Value.ToString())
            {
                StartCoroutine(Teleport(e.Value.ToString().Trim('\"'), is_teleporting_by_button: false));
               // Debug.Log($"changing location -> {currentLocation}");
            }
        }

        public IEnumerator Teleport(string codeWord, bool useAudio = false, bool immediate = false, float transitionSpeed = 1f, bool is_teleporting_by_button = true)
        {
            if (currentLocation == null) yield break;
            //if(goal != null) Debug.Log($"moves - {goal.PlayerMoves}, {goal.AvailibleMoves}");
            LocationInfo.instance.Hide();
            LocationExpander.instance.Hide();
            var loc = GetLocation(currentLocation);
            if (loc == null) yield break;
            loc.Leave();
            currentLocation = GetLocation(codeWord).CodeWord;
            loc = GetLocation(currentLocation);
            if (loc == null) yield break;
            loc.Stay();
            //get blend
            Texture blend = Resources.Load<Texture>(FilePaths.GetRandomTransitionEffectPath().Replace($"{ FilePaths.resources_directory}Resources/", "").Replace(".png", "").Replace(".jpg", ""));
            //Debug.Log(FilePaths.GetRandomTransitionEffectPath().Replace($"{ FilePaths.resources_directory}Resources/", ""));

            //это очень простой, рубящий всё метод. так делать нельзя. кто хочет много слоёв - перепишите!!
            string pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_photo, codeWord);
            UnityEngine.Object graphic = Resources.Load<Texture>(pathToGaphic);
            if (graphic == null)
            {
                pathToGaphic = FilePaths.GetPath(FilePaths.resources_graphics_bg_video, currentLocation);
                graphic = Resources.Load<UnityEngine.Video.VideoClip>(pathToGaphic);
            }
            if (graphic == null)
            {
                Debug.LogWarning("NULL GRAPHIC");
                yield break;
            }
            if (graphic is Texture) yield return gl
                    .SetTexture
                    (graphic as Texture, 
                    path: pathToGaphic, 
                    transitionSpeed: transitionSpeed, 
                    blend, 
                    immediate: 
                    immediate);
            if (graphic is UnityEngine.Video.VideoClip) yield return gl
                    .SetVideo
                    (graphic as UnityEngine.Video.VideoClip, 
                    path: pathToGaphic, 
                    transitionSpeed: 
                    transitionSpeed, 
                    blend, 
                    useAudio: useAudio, 
                    immediate: immediate);

            AUDIO.AudioManager.instance.PlayTrack($"{ FilePaths.resources_audio_locations}{codeWord}");
            if (SearchedObjectManager.instance.IsAnythingToSearch)
            {
                SearchedObjectManager.instance.CheckIfSomethingExistThere(codeWord);
            }
            if (is_teleporting_by_button)
            {
                VariableStore.TrySetValue("location", codeWord);
            }

            if (goal != null)
            {
                goal.Move();
                if (goal.isFailed) goal.FailGoal();
                else if (goal.IsFinished) goal.FinishGoal();
            }

            yield return null;
        }


        public Location GetLocation(string codeWord)
        {
            if (locations.ContainsKey(codeWord.ToLower())) return locations[codeWord.ToLower()];
            return null;
        }

        public void CreateLocation(string codeWord)
        {
            if (locations.ContainsKey(codeWord))
            {
                return;
            }
            LOCATION_INFO info = GetLocationInfo(codeWord);
            Location location = CreateLocationFromInfo(info);
            locations.Add(info.CodeWord.ToLower(), location);
        }

        private Location CreateLocationFromInfo(LOCATION_INFO info) => new Location(info.config);

        private LOCATION_INFO GetLocationInfo(string codeWord)
        {
            LOCATION_INFO result = new LOCATION_INFO();

            result.CodeWord = codeWord;
            result.config = config.GetConfig(result.CodeWord);
            return result;
        }

        internal class LOCATION_INFO
        {
            public string CodeWord = "";
            public LocationConfigData config = null;
        }

        public void SetGoal(string target, int moves, bool agile)
        {
            LocationExpander.instance.Expander.interactable = true;
            goal = new LocationGoal
            {
                targetLocation = target,
                isAgile = agile,
                AvailibleMoves = moves,
                convProgress = convProgress
            };
            DIALOGUE.DialogueSystem.instance.DialogueContainer.hide();
            DIALOGUE.DialogueSystem.instance.DisableGlobalReading();
            convProgress = DIALOGUE.DialogueSystem.instance.conversationManager.convProgress;
        }

        public void KillGoal(bool rollback = false)
        {
            //Debug.Log(LL_Goal.MainConversation);
            goal = null;
            if (LL_Goal.MainConversation != null)
            {
                if(rollback)
                {
                    if (LL_Goal.MainConversation.Count() < 2 && OldConversation != null)
                    {
                        LL_Goal.MainConversation = OldConversation;
                    }
                    LL_Goal.MainConversation.SetProgress(LL_Goal.RollBack);
                    DIALOGUE.DialogueSystem.instance.conversationManager.StartConverstaion(LL_Goal.MainConversation);
                    
                }
                else
                {
                    LL_Goal.MainConversation.SetProgress(LL_Goal.After);
                    DIALOGUE.DialogueSystem.instance.conversationManager.StartConverstaion(LL_Goal.MainConversation);
                }
            }
            



        }

        public void GoalInProgress()
        {
            LL_Goal.MainConversation = DIALOGUE.DialogueSystem.instance.conversationManager.conversation;
            LocationExpander.instance.Hide();
            DIALOGUE.DialogueSystem.instance.DialogueContainer.show();
            DIALOGUE.DialogueSystem.instance.EnableGlobalReading();
        }

      
    }
}