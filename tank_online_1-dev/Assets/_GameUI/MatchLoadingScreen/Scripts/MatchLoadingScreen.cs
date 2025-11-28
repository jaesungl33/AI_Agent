using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MatchLoadingScreen : UIScreenBase
{
    [SerializeField] private MatchLoadingCard[] team1, team2;

    private SOMatchData soMatchPlayerData;
    public SOMatchData GameMatchData
    {
        get => soMatchPlayerData;
        set => soMatchPlayerData = value;
    }

    public override void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        base.Show();
        ClearPlayerCards();
        StartCoroutine(UpdatePlayerCards());
    }

    private void ClearPlayerCards()
    {
        for (int i = 0; i < team1.Length; i++)
        {
            team1[i].SetInfo("", null, "", null);
            team1[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < team2.Length; i++)
        {
            team2[i].SetInfo("", null, "", null);
            team2[i].gameObject.SetActive(false);
        }
    }
    
    private IEnumerator UpdatePlayerCards()
    {
        yield return null;
        GameMatchData = DatabaseManager.GetDB<SOMatchData>();
        var players = GameMatchData.MatchDataArray;
        GameAssetCollection gameAssetCollection = DatabaseManager.GetDB<GameAssetCollection>();
        TankCollection tankCollection = DatabaseManager.GetDB<TankCollection>();
        yield return null;
        int team1Count = 0, team2Count = 0;
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            var tankData = tankCollection.GetTankByID(player?.TankId);
            var tankClassData = gameAssetCollection.GetClassIcon(player?.TankId);
            var tankImage = gameAssetCollection.GetMainIcon(player?.TankId);

            if (player.TeamIndex == 0)
            {
                team1[team1Count].gameObject.SetActive(true);
                team1[team1Count].SetInfo(player.PlayerName, tankImage as Sprite, tankData?.tankName, tankClassData as Sprite);
                team1Count++;
            }
            else if (player.TeamIndex == 1)
            {
                team2[team2Count].gameObject.SetActive(true);
                team2[team2Count].SetInfo(player.PlayerName, tankImage as Sprite, tankData?.tankName, tankClassData as Sprite);
                team2Count++;
            }
        }
    }
}
