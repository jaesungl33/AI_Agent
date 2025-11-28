using Fusion.GameSystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using GDO.Audio;

/// <summary>
/// MainMenuState represents the main menu state of the game.
/// It handles the display of the main menu UI and load scene and manages the game's time scale.
/// </summary>
public class OnboardingMatchIdlingState : GameState
{
    public OnboardingMatchIdlingState(GameStateMachine stateMachine) : base(stateMachine) { }
    private MatchmakingCollection matchmakingCollection;

    public override void Enter()
    {
        base.Enter();
        matchmakingCollection = DatabaseManager.GetDB<MatchmakingCollection>();

        SelectOnboardingMatch();
        // Start matchmaking process
        EventManager.TriggerEvent(MatchmakingEvent.Find());
        // Set the time scale to normal
        Time.timeScale = 1f;
    }

    private int FindOnboardingMatch()
    {
        int index = -1;
        matchmakingCollection.GetDocumentByProperty(doc => doc.matchType == MatchType.OnBoarding && doc.FixedRegion == GameManager.fixedRegion, true,
        (matchmakingDocument) =>
        {
            Debug.Log("Onboarding Match Mode: " + matchmakingDocument.MatchMode + " & " + matchmakingDocument.matchType);
            Debug.Log("Onboarding Match Document ID: " + matchmakingDocument.matchID);
            index = matchmakingCollection.GetIndex(matchmakingDocument.matchID);
        });
        Debug.Log("Onboarding Match Index: " + index);
        return index;
    }

    private void SelectOnboardingMatch()
    {
        int onboardingIndex = FindOnboardingMatch();
        if (onboardingIndex != -1)
        {
            matchmakingCollection.SetMatchIndex(onboardingIndex);
        }
        else
        {
            Debug.LogError("Onboarding match mode not found!");
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}