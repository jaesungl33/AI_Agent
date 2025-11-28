using Fusion;
using UnityEngine;

public class OutpostManager : NetworkBehaviour
{
    private void Start()
    {
       EventManager.Register<OutpostDestroyEvent>(KillOutpost);
    }

    // Method to handle outpost killing logic
    // Only the host or master client should process this event
    private void KillOutpost(OutpostDestroyEvent outpostData)
    {
        if (!Object.HasInputAuthority)
            return;

        // Implement the logic to kill the outpost with the given ID
        Debug.Log($"Outpost with ID {outpostData.OutpostId.ToString()} has been killed by {outpostData.KillerId}.");

        
    }
}

public struct OutpostDestroyEvent : INetworkStruct
{
    public NetworkString<_32> OutpostId;

    public int KillerId;
}