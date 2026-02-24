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
    public float zoneRadius = 5.0f;
    public float captureSpeed = 0.05f;

    [UdonSynced] private float internalProgress = 0;
    [UdonSynced] private int currentOwner = 0;

    public float GetProgress()
    {
        return internalProgress;
    }

    void Update()
    {
        if (!Networking.IsMaster) return;

        int b = 0;
        int r = 0;

        foreach (PlayerStatsManager stats in gameManager.allStatsManagers)
        {
            if (stats.ownerPlayerId == -1) continue;

            VRCPlayerApi p = VRCPlayerApi.GetPlayerById(stats.ownerPlayerId);
            if (Utilities.IsValid(p))
            {
                if (Vector3.Distance(p.GetPosition(), captureZone.position) <= zoneRadius)
                {
                    if (stats.playerTeam == 1) b++;
                    else if (stats.playerTeam == 2) r++;
                }
            }
        }

        float delta = (b - r) * captureSpeed * Time.deltaTime;
        float prevProgress = internalProgress;
        internalProgress = Mathf.Clamp(internalProgress + delta, -1.0f, 1.0f);

        int prevOwner = currentOwner;
        if (internalProgress >= 1.0f) currentOwner = 1;
        else if (internalProgress <= -1.0f) currentOwner = 2;
        else if (currentOwner == 1 && internalProgress <= 0) currentOwner = 0;
        else if (currentOwner == 2 && internalProgress >= 0) currentOwner = 0;

        if (prevProgress != internalProgress || prevOwner != currentOwner)
        {
            if (prevOwner != currentOwner)
            {
                gameManager.UpdatePointOwner(pointIndex, currentOwner);
            }
            RequestSerialization();
        }
    }
}