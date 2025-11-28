namespace BehaviorTree.Tutorials
{   
    public struct DoneTutorialEventData { public string tutorialID; }
    public struct SkipTutorialEventData { public string tutorialID; }
    public struct TutorialStepCompletedEventData
    {
        public string tutorialID;
        public string stepName;
        public TutorialStepCompletedEventData(string tutorialID, string stepName)
        {
            this.tutorialID = tutorialID;
            this.stepName = stepName;
        }
    }
}