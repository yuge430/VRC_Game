using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerStatsManager : UdonSharpBehaviour
{
    [UdonSynced] public int ownerPlayerId = -1;
    [UdonSynced] public int playerTeam = 0;
    [UdonSynced] public int totalPoints;

    public bool IsBlue() => playerTeam == 1;
    public bool IsRed() => playerTeam == 2;

    public void AssignOwner(VRCPlayerApi player)
    {
        if (!Networking.IsMaster) return;
        Networking.SetOwner(player, gameObject);
        ownerPlayerId = player.playerId;
        totalPoints = 5000;
        RequestSerialization();
    }

    public void ClearOwner()
    {
        if (!Networking.IsMaster) return;
        ownerPlayerId = -1;
        playerTeam = 0;
        RequestSerialization();
    }

    public void JoinTeam(int teamId)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        playerTeam = teamId;
        RequestSerialization();
    }

    public void AddPoints(int amount)
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
        totalPoints += amount;
        RequestSerialization();
    }
}