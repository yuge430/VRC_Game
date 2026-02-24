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

    [Header("Global Settings")]
    public CoreGameManager gameManager;

    [Header("Robot Settings")]
    public Transform robotRoot;

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject explosionEffect;

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

        if (totalPoints <= 0)
        {
            totalPoints = 0;
            RequestSerialization();
            Die();
        }
        else
        {
            RequestSerialization();
        }
    }

    private void Die()
    {
        if (gameManager != null)
        {
            if (Networking.IsMaster)
            {
                gameManager.ReduceTicket(playerTeam, 1);
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "RequestTicketReductionOnMaster");
            }
        }

        if (explosionEffect != null)
        {
            GameObject exp = VRCInstantiate(explosionEffect);
            exp.transform.position = Networking.LocalPlayer.GetPosition();
        }

        if (robotRoot != null && respawnPoint != null)
        {
            Vector3 robotSpawnPos = respawnPoint.position + (respawnPoint.forward * 3.0f);
            robotRoot.position = robotSpawnPos;
            robotRoot.rotation = respawnPoint.rotation;
        }

        if (respawnPoint != null)
        {
            Networking.LocalPlayer.TeleportTo(respawnPoint.position, respawnPoint.rotation);
        }

        totalPoints = 5000;
        RequestSerialization();
    }

    public void RequestTicketReductionOnMaster()
    {
        if (Networking.IsMaster && gameManager != null)
        {
            gameManager.ReduceTicket(playerTeam, 1);
        }
    }
}