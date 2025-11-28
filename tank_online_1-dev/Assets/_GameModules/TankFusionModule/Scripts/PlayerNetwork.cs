using Fusion;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> PlayerName { get; set; } // Max 32 chars
    private string cachedName = "";
    public TMPro.TextMeshPro nameTag;

    public override void Spawned()
    {
        // Nếu là local player, set name
        if (Object.HasInputAuthority)
        {
            var playerDocument = DatabaseManager.GetDB<PlayerCollection>().GetMine();
            RPC_SetPlayerName(playerDocument.playerName);
        }

        // Gán name tag text
        nameTag.text = PlayerName.ToString();
    }

   public override void FixedUpdateNetwork()
    {
        string currentName = PlayerName.ToString();

        if (cachedName != currentName)
        {
            cachedName = currentName;
            nameTag.text = currentName;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }
}
