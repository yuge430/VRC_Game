using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CoreGameManager : UdonSharpBehaviour
{
    public int maxTickets = 500;
    public float ticketDrainInterval = 3.0f;
    public int totalPointCount = 5;
    public PlayerStatsManager[] allStatsManagers;

    [UdonSynced] public bool gameRunning = false;
    [UdonSynced] public int blueTickets;
    [UdonSynced] public int redTickets;
    [UdonSynced] public int[] pointOwners;

    private float timer;

    void Start()
    {
        if (pointOwners == null || pointOwners.Length != totalPointCount) pointOwners = new int[totalPointCount];
        if (Networking.IsMaster) InitializeGame();
    }

    public void InitializeGame()
    {
        if (!Networking.IsMaster) return;
        blueTickets = maxTickets;
        redTickets = maxTickets;
        gameRunning = true;
        RequestSerialization();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.IsMaster) return;
        foreach (PlayerStatsManager stats in allStatsManagers)
        {
            if (stats.ownerPlayerId == -1) { stats.AssignOwner(player); break; }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.IsMaster)
        {
            foreach (PlayerStatsManager stats in allStatsManagers)
            {
                if (stats.ownerPlayerId == player.playerId) stats.ClearOwner();
            }
            RequestSerialization();
        }
    }

    public PlayerStatsManager GetStatsByPlayer(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player)) return null;
        int targetId = player.playerId;
        foreach (PlayerStatsManager stats in allStatsManagers)
        {
            if (stats.ownerPlayerId == targetId) return stats;
        }
        return null;
    }

    void Update()
    {
        if (!gameRunning || !Networking.IsMaster) return;
        timer += Time.deltaTime;
        if (timer >= ticketDrainInterval) { CalculateTicketDrain(); timer = 0; }
    }

    public void UpdatePointOwner(int pointIndex, int newOwner)
    {
        if (!Networking.IsMaster) return;
        if (pointIndex >= 0 && pointIndex < pointOwners.Length)
        {
            pointOwners[pointIndex] = newOwner;
            RequestSerialization();
        }
    }

    public void ReduceTicket(int team, int amount)
    {
        if (!Networking.IsMaster) return;
        if (!gameRunning) return;

        if (team == 1) blueTickets = Mathf.Max(0, blueTickets - amount);
        else if (team == 2) redTickets = Mathf.Max(0, redTickets - amount);

        if (blueTickets <= 0 || redTickets <= 0) gameRunning = false;
        RequestSerialization();
    }

    private void CalculateTicketDrain()
    {
        int b = 0; int r = 0;
        foreach (int o in pointOwners) { if (o == 1) b++; else if (o == 2) r++; }
        if (b == r) return;
        if (b > r) redTickets = Mathf.Max(0, redTickets - (b - r));
        else blueTickets = Mathf.Max(0, blueTickets - (r - b));
        if (blueTickets <= 0 || redTickets <= 0) gameRunning = false;
        RequestSerialization();
    }
}