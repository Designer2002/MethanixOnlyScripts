using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class ConversationManager
    {
        private DialogueSystem dialogueSystem = DialogueSystem.instance;
        private Coroutine process = null;

        public event EventHandler OnStartOfGoal;

        public static bool userPrompt = false;

        public Logical_Lines.LogicalLineManager LogicalLineManager { get; private set; }
        public bool isRunning => process != null;
        public TextArchitect architect = null;

        private ConversationQueue conversationQueue;

        public int convProgress => conversationQueue.isEmpty() ? -1 : conversationQueue.top.GetProgress();

        private Coroutine co_ending = null;
        private bool is_ending => co_ending != null;

        public Conversation conversation => conversationQueue.isEmpty() ? null : conversationQueue.top;

        public string architectedText => architect.current_text;
        public ConversationManager(TextArchitect textArchitect)
        {
            this.architect = textArchitect;
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
            LogicalLineManager = new Logical_Lines.LogicalLineManager();
            conversationQueue = new ConversationQueue();
            OnStartOfGoal += OnGoalEnding;
        }

        public Conversation[] GetConversationQueue() => conversationQueue.GetReadOnly();

        private void OnGoalEnding(object sender, EventArgs e)
        {
            EndGoal();
        }

        private Coroutine EndGoal()
        {
            if(is_ending)
            {
                DialogueSystem.instance.StopCoroutine(EndingGoal());
                co_ending = null;
            }
            co_ending = DialogueSystem.instance.StartCoroutine(EndingGoal());
            return co_ending;
        }

        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);
        private void OnUserPrompt_Next()
        {
            userPrompt = true;
        }

        private IEnumerator EndingGoal()
        {
            DialogueSystem.instance.conversationManager.StopConversation();
            yield return DIALOGUE.DialogueSystem.instance.DialogueContainer.hide();
            DialogueSystem.instance.DisableGlobalReading();

            co_ending = null;
        }

        public Coroutine StartConverstaion(Conversation conversation)
        {
            StopConversation();
            conversationQueue.Clear();
            Enqueue(conversation);
            process = dialogueSystem.StartCoroutine(RunningConversation());
            return process;
        }
        public void StopConversation()
        {
            if (!isRunning)
                return;
            dialogueSystem.StopCoroutine(process);
            process = null;

        }

        

        IEnumerator RunningConversation()
        {
            while(!conversationQueue.isEmpty())
            {
                Conversation current = conversation;
                if (current.HasReachedEnd() || current.Count() == 0)
                {
                    conversationQueue.Dequeue();
                    continue;
                }
                string rawLine = current.currentLine();
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    TryAdvanceConverstation(current);
                    continue;
                }
                if (rawLine.Contains("}"))
                {
                    TryAdvanceConverstation(current);
                    continue;
                }

                DIALOGUE_LINE line = DialogParser.Parse(rawLine);

                if (LogicalLineManager.TryGetLogic(line, out Coroutine logic))
                {   
                    yield return logic;
                }
                else
                {

                    if (line.hasDialogue)
                    {
                        yield return Line_RunDialogue(line);
                    }
                    if (line.hasCommands)
                    {
                        //Debug.Log("Command found");
                        yield return Line_RunCommands(line);
                    }

                    if (line.hasDialogue)
                    {
                        yield return WaitForInput();
                        CommandManager.instance.StopAllProcess();

                        dialogueSystem.OnSystemPrompt_Clear();
                    }
                }

                TryAdvanceConverstation(current);
            }
            process = null;
        }

        private void TryAdvanceConverstation(Conversation conversation)
        {
            if (conversation.TryStepNext() && conversation.nextLine().Contains("goal("))
            {
                LOCATIONS.LocationManager.instance.RollBackProgress = conversation.GetProgress() - 1;
                LOCATIONS.LocationManager.instance.OldLocation = LOCATIONS.LocationManager.instance.currentLocation;
                LOCATIONS.LocationManager.instance.OldConversation = this.conversation;
            }
            conversation.IncrementProgress();
            if (conversation != conversationQueue.top) return;
            if (conversation.HasReachedEnd())
            {
                LOCATIONS.LocationManager.instance.Status = LOCATIONS.LocationManager.ConversationStatus.End;
                if (LOCATIONS.LocationManager.instance.goal != null) OnStartOfGoal?.Invoke(this, EventArgs.Empty);
                conversationQueue.Dequeue();
                
            }
            if(conversation.GetProgress() == 0) conversation.OnProgressStarted += conversation.SetLocationManagerStatusToTop;
        }

        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            if (line.hasSpeaker)
            {
                HandleSpeakerLogic(line.speakerData);
            }

            //if (!dialogueSystem.DialogueContainer.isVisible) dialogueSystem.DialogueContainer.show();
            //else
            //{
            //    dialogueSystem.HideSpeakerName();
            //    dialogueSystem.HideSideImage();
            //}
           
            yield return BuildLineSegments(line.dialogueData);
            //yield return WaitForInput();
        }

        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool characterMustBeCreated = (speakerData.MakeCharacterEnter || speakerData.isCastingExpression || speakerData.isCastingPosition);
            if (speakerData.name == "n") CHARACTERS.CharacterManager.instance.UnhighlightAll();
            CHARACTERS.Character character = CHARACTERS.CharacterManager.instance.GetCharacter(speakerData.name, createIfNotExist: characterMustBeCreated);
            if(character != null) character.LightUp();
            if (speakerData.MakeCharacterEnter && (!character.IsVisible) && !character.is_revealing)
            {
                character.Show();               
            }
                
            dialogueSystem.ShowName(TagManager.Inject(CHARACTERS.CharacterManager.instance.FindByAlias(speakerData.name) == string.Empty ? speakerData.DisplayName : CHARACTERS.CharacterManager.instance.FindByAlias(speakerData.name)));
            dialogueSystem.ShowImage(TagManager.Inject(CHARACTERS.CharacterManager.instance.FindByAlias(speakerData.name) == string.Empty ? speakerData.DisplayName : CHARACTERS.CharacterManager.instance.FindByAlias(speakerData.name)));
            DialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            if (speakerData.isCastingPosition) character.MoveToPosition(speakerData.castPosition);

            if(speakerData.isCastingExpression)
            {
                foreach (var ce in speakerData.CastExpressions)
                {
                   character.OnReceiveExpressions(ce.expression, ce.layer);
                }
            }
        }
        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for(int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];
                yield return WaitForDialogueSegmentSgnalToBeTriggered(segment);
                yield return BuildDialogue(segment.dialogue, segment.AppendText);
            }
        }

        public bool isWaitingOnAutoTimer { get; private set; } = false;
        IEnumerator WaitForDialogueSegmentSgnalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch(segment.Signal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                    yield return WaitForInput();
                    dialogueSystem.OnSystemPrompt_Clear();
                    break; 
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                    isWaitingOnAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    dialogueSystem.OnSystemPrompt_Clear();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    isWaitingOnAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingOnAutoTimer = false;
                    break;
                default:
                    break;
            }
        }

        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;
            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if(command.waitForCompletion || command.name == "wait")
                {    
                    CoroutineWrapper cw = CommandManager.instance.Execute(command.name, command.arguments);
                    while(!cw.isDone)
                    {
                        if(userPrompt)
                        {
                            CommandManager.instance.StopCurrentProcess();
                            userPrompt = false;
                        }
                        yield return null;
                    }
                }
                else CommandManager.instance.Execute(command.name, command.arguments);
            }
            yield return null;
        }

        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            dialogue = TagManager.Inject(dialogue);
            if (!append) architect.Build(dialogue);
            else architect.Append(dialogue);
            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    //if (!architect.hurryup)
                    //    architect.hurryup = true;
                    //else
                    //{
                    //    architect.ForceComplete();

                    //}
                    architect.ForceComplete();
                    userPrompt = false;
                    yield break;
                }
                yield return null;
            }

            if (!architect.isBuilding)
            {
                userPrompt = true;
            }
        }

        IEnumerator WaitForInput()
        {
            dialogueSystem.DialogueContinuationPrompt.Change();
            dialogueSystem.DialogueContinuationPrompt.Show();
            
            while(!userPrompt)
            {
                yield return null;
            }
            dialogueSystem.DialogueContinuationPrompt.Hide();
            userPrompt = false;
        }
    }
}
