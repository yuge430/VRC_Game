using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RobotDriver : UdonSharpBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 150.0f;

    private VRCStation seat;
    private bool isPilot = false;

    void Start()
    {

        seat = (VRCStation)GetComponentInChildren(typeof(VRCStation));
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isPilot = true;
            Networking.SetOwner(player, gameObject);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isPilot = false;
        }
    }

    void Update()
    {
        if (!isPilot) return;

        float moveInput = Input.GetAxis("Vertical");  
        float rotateInput = Input.GetAxis("Horizontal"); 

        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * rotateInput * rotationSpeed * Time.deltaTime);
    }
}