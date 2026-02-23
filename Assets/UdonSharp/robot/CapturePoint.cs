using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CapturePoint : UdonSharpBehaviour
{
    public CoreGameManager gameManager;
    public int pointIndex;
    public Transform captureZone;
    public float zoneRadius = 10f;
    public float captureSpeed = 0.05f; 
    [UdonSynced] private int syncProgress;
    [UdonSynced] private int currentOwner = 0;
    private float internalProgress = 0f;
    private float scanTimer = 0f;
    private VRCPlayerApi[] playerBuffer = new VRCPlayerApi[100];

    void Update()
    {
        if (!Networking.IsMaster || !gameManager.gameRunning) return;
        scanTimer += Time.deltaTime;
        if (scanTimer >= 0.5f)
        {
            int count = VRCPlayerApi.GetPlayerCount();
            VRCPlayerApi.GetPlayers(playerBuffer);
            int b = 0; int r = 0;
            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(playerBuffer[i])) continue;
                if (Vector3.Distance(playerBuffer[i].GetPosition(), captureZone.position) <= zoneRadius)
                {
                    PlayerStatsManager s = gameManager.GetStatsByPlayer(playerBuffer[i]);
                    if (s != null) { if (s.IsBlue()) b++; else if (s.IsRed()) r++; }
                }
            }
            int diff = b - r;
            if (diff == 0) internalProgress = Mathf.MoveTowards(internalProgress, 0f, captureSpeed * 0.5f);
            else internalProgress = Mathf.Clamp(internalProgress + diff * captureSpeed * 0.5f, -1f, 1f);
            int nOwner = currentOwner;
            if (internalProgress >= 0.99f) nOwner = 1;
            else if (internalProgress <= -0.99f) nOwner = 2;
            else if (Mathf.Abs(internalProgress) < 0.05f) nOwner = 0;
            if (nOwner != currentOwner) { currentOwner = nOwner; gameManager.UpdatePointOwner(pointIndex, nOwner); }
            syncProgress = Mathf.RoundToInt(internalProgress * 100f);
            RequestSerialization();
            scanTimer = 0f;
        }
    }
    public override void OnDeserialization() { internalProgress = syncProgress / 100f; }
}