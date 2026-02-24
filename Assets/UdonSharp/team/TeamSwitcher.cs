using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeamSwitcher : UdonSharpBehaviour
{
    public CoreGameManager gameManager;
    public int teamToJoin;

    public override void Interact()
    {
        PlayerStatsManager myStats = gameManager.GetStatsByPlayer(Networking.LocalPlayer);
        if (myStats != null)
        {
            myStats.JoinTeam(teamToJoin);
            Debug.Log("チームに参加しました: " + teamToJoin);
        }
    }
}