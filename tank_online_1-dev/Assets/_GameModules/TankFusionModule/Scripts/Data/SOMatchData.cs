using System.Collections.Generic;
using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "SOMatchData", menuName = "Game/SOMatchData", order = 1)]
public class SOMatchData : ScriptableObject
{
    public int winningTeam = -1;
    public int defenderKill = 0;
    public int attackerKill = 0;
    /// <summary>
    /// The array of match player data.
    /// Max of 10 players
    /// </summary>
    [SerializeField] private List<MatchPlayerData> matchDataList = new List<MatchPlayerData>();

    public MatchPlayerData[] MatchDataArray => matchDataList.ToArray();
    /// <summary>
    /// Update specific fields of player data
    /// </summary>
    public void UpdatePlayerData(PlayerRef playerRef, MatchPlayerData newData)
    {
        if (matchDataList == null)
            matchDataList = new List<MatchPlayerData>();

        int index = matchDataList.FindIndex(p => p.PlayerId == playerRef.AsIndex);

        if (index >= 0)
        {
            // ===== UPDATE EXISTING PLAYER =====
            MatchPlayerData existingPlayer = matchDataList[index];

            // Preserve immutable fields
            newData.PlayerId = existingPlayer.PlayerId;
            newData.TeamIndex = existingPlayer.TeamIndex;
            newData.IndexInTeam = existingPlayer.IndexInTeam;
            newData.PlayerName = existingPlayer.PlayerName;
            newData.IsLocalPlayer = existingPlayer.IsLocalPlayer;
            newData.IsJoined = existingPlayer.IsJoined;

            //WrapDecalStickers Fixme
            newData.WrapId = existingPlayer.WrapId;
            
            // Update lại vào list
            matchDataList[index] = newData;

            Debug.Log($"[SOMatchData] Updated player {newData.PlayerName}: Joined={newData.IsJoined}, Tank={newData.TankId}, Wrap = {newData.WrapId}");
        }
        else
        {
            // ===== ADD NEW PLAYER =====
            Debug.LogWarning($"[SOMatchData] Player {playerRef.AsIndex} not found, adding new player");
            newData.PlayerId = playerRef.AsIndex;
            matchDataList.Add(newData);
        }
    }

    // public void UpdatePlayerData(PlayerRef playerRef, MatchPlayerData playerData)
    // {
    //     if (matchDataList == null) matchDataList = new List<MatchPlayerData>();
    //     int index = matchDataList.FindIndex(p => p.PlayerId == playerRef.AsIndex);
    //     MatchPlayerData existingPlayer = matchDataList[index];

    //     bool isLocalPlayer =  MatchDataArray[index].IsLocalPlayer;
    //     string playerName = MatchDataArray[index].PlayerName;
    //     int teamIndex = MatchDataArray[index].TeamIndex;
    //     int playerIndex = MatchDataArray[index].IndexInTeam;
    //     // Ensure consistency
    //     playerData.TeamIndex = teamIndex; // keep the original team index
    //     playerData.PlayerId = playerRef.AsIndex;
    //     playerData.IndexInTeam = playerIndex; // keep the original index in team
    //     //playerData.IsJoined = true;
    //     playerData.PlayerName = playerName; // keep the original name
    //     playerData.IsLocalPlayer = isLocalPlayer; // keep the original local player status

    //     if (index >= 0)
    //     {
    //         matchDataList[index] = playerData;
    //     }
    //     else
    //     {
    //         matchDataList.Add(playerData);
    //     }

    //    // Debug.Log($"SOMatchData UpdatePlayerData {JsonUtility.ToJson(playerData)}");
    // }

    public int GetLocalTeamIndex()
    {
        for (int i = 0; i < matchDataList.Count; i++)
        {
            if (matchDataList[i].IsLocalPlayer)
            {
                return matchDataList[i].TeamIndex;
            }
        }
        Debug.LogWarning("SOMatchData GetLocalTeamIndex: Local player not found.");
        return -1; // Return -1 if local player is not found
    }

    public void AddPlayer(MatchPlayerData playerData)
    {
        if (matchDataList == null) matchDataList = new List<MatchPlayerData>();
        //Debug.Log($"SOMatchData AddPlayer {JsonUtility.ToJson(playerData)}");
        matchDataList.Add(playerData);
    }
    
    public void RemovePlayerData(PlayerRef playerRef)
    {
        int index = matchDataList.FindIndex(p => p.PlayerId == playerRef.AsIndex);
        if (index >= 0)
        {
            matchDataList.RemoveAt(index);
            Debug.Log($"SOMatchData RemovePlayerData: Removed player with PlayerId {playerRef.AsIndex}");
        }
        else
        {
            Debug.LogWarning($"SOMatchData RemovePlayerData: Player with PlayerId {playerRef.AsIndex} not found.");
        }
    }

    public MatchPlayerData GetPlayerAtIndex(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= MatchDataArray.Length)
        {
            Debug.LogWarning($"SOMatchData GetPlayerAtIndex: Invalid player index {playerIndex}");
            return default;
        }
        //Debug.Log($"SOMatchData GetPlayerAtIndex {playerIndex}");
        MatchPlayerData existingPlayer = MatchDataArray[playerIndex];
        Debug.Log($"SOMatchData GetPlayerAtIndex found {JsonUtility.ToJson(existingPlayer)}");
        return existingPlayer;
    }

    public MatchPlayerData GetPlayerAtIndex(PlayerRef playerRef)
    {
        int index = matchDataList.FindIndex(p => p.PlayerId == playerRef.AsIndex);
        return GetPlayerAtIndex(index);
    }

    public void ClearAll()
    {
        matchDataList.Clear();
    }

    // public void GetReady(PlayerRef playerRef)
    // {
    //     MatchPlayerData player = GetPlayerAtIndex(playerRef);
    //     if (player != null)
    //     {
    //         player.IsReady = true;
    //         Debug.Log($"SOMatchData GetReady: Player {player.PlayerName} is ready.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"SOMatchData GetReady: Player with PlayerId {playerRef.AsIndex} not found.");
    //     }
    // }

    // public void SetPlay(PlayerRef playerRef)
    // {
    //     MatchPlayerData player = GetPlayerAtIndex(playerRef);
    //     if (player != null)
    //     {
    //         player.IsJoined = true;
    //         Debug.Log($"SOMatchData SetPlay: Player {player.PlayerName} has joined the match.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"SOMatchData SetPlay: Player with PlayerId {playerRef.AsIndex} not found.");
    //     }
    // }
}
