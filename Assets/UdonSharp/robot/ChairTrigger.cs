using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ChairTrigger : UdonSharpBehaviour
{
    [Header("座った時に動かしたいロボットの親オブジェクト")]
    public GameObject robotRoot;

    public override void Interact()
    {
        if (robotRoot != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, robotRoot);
        }

        Networking.LocalPlayer.UseAttachedStation();
    }
    public override void OnStationEntered(VRCPlayerApi player)
    {
        Debug.Log("ロボットに搭乗しました！");
    }
}