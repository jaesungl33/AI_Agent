using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Fusion.GameSystems;
using UnityEditor;
using System.Collections.Generic;

public class LoginScreen : UIScreenBase
{
    public GameObject panelLoginMethods;
    public GameObject[] methodLoginButtons;
    public bool isDebug = true;
    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder, param);
        bool hasKey = PlayerPrefs.HasKey(Constants.Key_UserID);
        panelLoginMethods.SetActive(!hasKey);
        if (hasKey)//has logged in before
        {
            foreach (var button in methodLoginButtons)
            {
                button.SetActive(false);
            }
            ChooseGuestLogin();
        }
        else
        {
            foreach (var button in methodLoginButtons)
            {
                button.SetActive(true);
            }
        }
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteKey(Constants.Key_UserID);
        PlayerPrefs.DeleteKey(Constants.Key_PlayerName);
        PlayerPrefs.DeleteKey(Constants.Key_RoleID);
        PlayerPrefs.Save();
        
        Debug.Log("Account data deleted.");
    }

    public void ChooseLoginMethod(int index)
    {
        if (index == 0)
        {
            LoginWithGooglePlayGames();
        }
        else if (index == 1)
        {
            LoginWithGameCenter();
        }
        else if (index == 2)
        {
            ChooseGuestLogin();
        }
        else
        {
            Debug.LogError("Invalid login method selected.");
        }
    }

    public void LoginWithGooglePlayGames()
    {
        Debug.Log("Login with Google Play Games is not implemented yet.");
        // Implement Google Play Games login logic here
    }

    public void LoginWithGameCenter()
    {
        Debug.Log("Login with Game Center is not implemented yet.");
        // Implement Game Center login logic here
    }

    public async void ChooseGuestLogin()
    {
        Debug.Log("Guest login selected.");
        string savedName;
        if (PlayerPrefs.HasKey(Constants.Key_PlayerName))
        {
            savedName = PlayerPrefs.GetString(Constants.Key_PlayerName);
        }
        else
        {
            savedName = RandomNameGenerator.GetRandomEnglishName();
        }
        await DoLogin(savedName);
    }

    public void HideCanvasGuestLogin()
    {
        // Hide the guest login UI
    }

    private async Task DoLogin(string playerName)
    {
        await Task.Yield();
        PlayerPrefs.SetString(Constants.Key_PlayerName, playerName);

        if (!PlayerPrefs.HasKey(Constants.Key_UserID))
        {
            PlayerPrefs.SetString(Constants.Key_UserID, SystemInfo.deviceUniqueIdentifier);
        }

        var userId = PlayerPrefs.GetString(Constants.Key_UserID);

        var userCollection = DatabaseManager.GetDB<UserCollection>();
        userCollection.Initialize(userId);
        await userCollection.ReadAsync();

        var userDocument = userCollection.GetMine();
        if (userDocument == null)
        {
            userDocument = new UserDocument
            {
                roleID = $"{userId}_{0}",
                userID = userId,
                inactiveRoleIDs = new()
            };
            userCollection.AddDocumentAsync(userDocument);
        }
        else if (string.IsNullOrWhiteSpace(userDocument.roleID))
        {
            userDocument.roleID = $"{userId}_{userDocument.inactiveRoleIDs.Count}";
            await userCollection.WriteAsync();
        }
        PlayerPrefs.SetString(Constants.Key_RoleID, userDocument.roleID);
        PlayerPrefs.Save();
        var roleId = userDocument.roleID;
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        playerCollection.Initialize(roleId);
        await playerCollection.ReadAsync();

        var playerDocument = playerCollection.GetMine();
        if (playerDocument == null)
        {
            var allWraps = DatabaseManager.GetDB<TankWrapCollection>().GetAllWraps();
            playerDocument = new PlayerDocument
            {
                roleID = roleId,
                playerName = playerName,
                gold = 100000,
                diamond = 1000,
                elo = 1000,
                level = 1,
                tanks = new List<string> { "tank.scout2", "tank.assault2", "tank.heavy2" },
                selectedTank = "tank.scout2",
                ownedAIStickers = new List<string> { "deco.aisticker1", "deco.aisticker2" },
                ownedDecals = new List<string> { "deco.decal1", "deco.decal2" },
                ownedStickers = new List<string> { "deco.sticker1", "deco.sticker2" },
                ownedWraps = allWraps?.ConvertAll(item => item.itemCatalogId),
                formationTanks = new List<string> { }
                //formationTanks = new List<string> { JsonUtility.ToJson(new PlayerDocument.FormationTanks("tank.scout3", new int[] { 1 })) }
            };
            playerCollection.AddDocumentAsync(playerDocument);
        }
        // else // closed for bug, maybe not update name every login
        // {
        //     playerDocument.playerName = playerName;
        //     await playerCollection.WriteAsync();
        // }

        HideCanvasGuestLogin();
        CheckOnboarding(playerDocument);
    }
    
    private void CheckOnboarding(PlayerDocument playerDocument)
    {
        if (isDebug)
        {
            EventManager.TriggerEvent<GamePhase>(GamePhase.MatchIdling);
            return;
        }
        if (playerDocument.tutorialCompleted == false)
        {
            // First step of onboarding: Find match with onboarding mode
            EventManager.TriggerEvent<GamePhase>(GamePhase.OnboardingMatchIdling);
        }
        else
        {
            EventManager.TriggerEvent<GamePhase>(GamePhase.MatchIdling);
        }
    }
}