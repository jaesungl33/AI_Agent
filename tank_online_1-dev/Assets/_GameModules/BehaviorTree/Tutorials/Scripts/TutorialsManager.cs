using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UniRx;
using Unity.Behavior;
using UnityEngine;
namespace BehaviorTree.Tutorials
{

    public class TutorialUserData
    {
        public Dictionary<string, string> flowCheckpoints = new Dictionary<string, string>();
    }
    /// <summary>
    /// Each tutorial must have a unique tutorialID. In blackboard, there are variables to track current step name and whether to skip tutorial.
    /// "tutorialID" (string): Unique identifier for the tutorial.
    /// "currentStepName" (string): The name of the current step in the tutorial.
    /// "stepsName" (List<string>): List of step names in the tutorial.
    /// "skipTutorial" (bool): Flag to indicate if the tutorial should be skipped.
    /// When tutorial is completed, "currentStepName" is set to "tutorial_finish".
    /// </summary>
    public class TutorialsManager : Singleton<TutorialsManager>
    {
        private const string TUTORIALS_DATA_KEY = "tutorials_data";
        public const string TUTORIAL_FINISH_VALUE = "tutorial_finish";


        public const string CURRENT_STEP_NAME_REF = "currentStepName";
        public const string TUTORIALID_REF = "tutorialID";
        public const string STEPS_NAME_REF = "stepsName";
        public const string SKIP_TUTORIAL_SHAREDBOOL_KEY = "skipTutorial";

        [SerializeField] private GameObject _tutorialObj;
        [SerializeField] private string tutorialID;
        private BehaviorGraphAgent currentBT;

        private List<string> stepsName = new List<string>();
        void Start()
        {
            Init(null);
        }
        public void Init(System.Action callback)
        {
            EventManager.Register<TutorialStepCompletedEventData>(TutorialStepCompleletedEventHandle);
            EventManager.Register<DoneTutorialEventData>(DoneTutorialEventHandle);
            if (IsTutorialFinished())
            {

            }
            else
            {
                StartTutorial();
            }
            callback?.Invoke();
        }

        private void DoneTutorialEventHandle(DoneTutorialEventData data)
        {
            SaveCheckPoint(TUTORIAL_FINISH_VALUE, true);
            currentBT.gameObject.SetActive(false);
        }

        private void TutorialStepCompleletedEventHandle(TutorialStepCompletedEventData data)
        {
            if(tutorialID == data.tutorialID)
            {
                SaveCheckPoint(data.stepName);
            }
        }
        private void StartTutorial()
        {
            currentBT = _tutorialObj.GetComponent<BehaviorGraphAgent>();
            currentBT.SetVariableValue(TUTORIALID_REF, tutorialID);
            var tutorialUserData = GetTutorialUserDataFromLocal();
            if (tutorialUserData.flowCheckpoints.ContainsKey(tutorialID))
            {
                var checkpoint = tutorialUserData.flowCheckpoints[tutorialID];
                currentBT.SetVariableValue(CURRENT_STEP_NAME_REF, checkpoint);
            }
            else
            {
                currentBT.SetVariableValue(CURRENT_STEP_NAME_REF, string.Empty);
            }
            _tutorialObj.SetActive(true);
            stepsName = new List<string>();
        }

        public bool IsTutorialFinished()
        {
            var tutorialUserData = GetTutorialUserDataFromLocal();
            if (tutorialUserData != null && tutorialUserData.flowCheckpoints.TryGetValue(tutorialID, out var checkpoint))
            {
                if (checkpoint == TUTORIAL_FINISH_VALUE)
                {
                    return true;
                }
            }
            return false;
        }
        private void SaveCheckPoint(string stepName, bool forceSave = false)
        {
            TutorialUserData tutorialUserData = GetTutorialUserDataFromLocal();
            if(!forceSave)
            {
                Debug.Log("SaveCheckPoint: " + stepName);
                if (stepsName.Count == 0)
                {
                    currentBT.GetVariable(STEPS_NAME_REF, out BlackboardVariable<List<string>> stepsNameBlackboardVar);
                    if (stepsNameBlackboardVar != null)
                    {
                        stepsName = stepsNameBlackboardVar.Value;
                    }
                }

                if (!stepsName.Contains(stepName))
                {
                    return;
                }
                string nextCheckpoint;
                var stepNameIndex = stepsName.IndexOf(stepName);
                if (stepNameIndex >= stepsName.Count - 1)
                {
                    nextCheckpoint = TUTORIAL_FINISH_VALUE;
                    currentBT.gameObject.SetActive(false);
                }
                else
                {
                    nextCheckpoint = stepsName[stepNameIndex + 1];
                }
                currentBT.SetVariableValue(CURRENT_STEP_NAME_REF, nextCheckpoint);
                tutorialUserData.flowCheckpoints[tutorialID] = nextCheckpoint;
            }
            else
            {
                currentBT.SetVariableValue(CURRENT_STEP_NAME_REF, stepName);
                tutorialUserData.flowCheckpoints[tutorialID] = stepName;
            }
            SaveTutorialUserDataToLocal(tutorialUserData);
        }
        private TutorialUserData GetTutorialUserDataFromLocal()
        {
            TutorialUserData tutorialUserData = null;
            if (PlayerPrefs.HasKey(TUTORIALS_DATA_KEY))
            {
                var jsonData = PlayerPrefs.GetString(TUTORIALS_DATA_KEY);
                tutorialUserData = JsonConvert.DeserializeObject<TutorialUserData>(jsonData);
            }
            if (tutorialUserData == null)
            {
                tutorialUserData = new TutorialUserData();
                SaveTutorialUserDataToLocal(tutorialUserData);
            }

            return tutorialUserData;
        }
        private void SaveTutorialUserDataToLocal(TutorialUserData tutorialUserData)
        {
            PlayerPrefs.SetString(TUTORIALS_DATA_KEY, JsonConvert.SerializeObject(tutorialUserData));
        }
    }
}

