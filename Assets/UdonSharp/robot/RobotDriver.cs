using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class RobotDriver : UdonSharpBehaviour
{
    [Header("移動設定")]
    public float normalSpeed = 5.0f;
    public float dashForce = 20.0f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;

    [Header("視点追従・旋回設定")]
    public float turnSmoothing = 5.0f;
    public float deadZoneAngle = 30.0f;

    [Header("射撃システム")]
    public Transform robotArm; // アムロの狙うシステム用
    public ParticleSystem muzzleFlash;

    private bool isPilot = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;

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
            if (muzzleFlash != null) muzzleFlash.Stop();
        }
    }

    void LateUpdate()
    {
        if (!isPilot) return;

        // Fキーで降りる
        if (Input.GetKeyDown(KeyCode.F))
        {
            VRCStation seat = (VRCStation)GetComponentInChildren(typeof(VRCStation));
            if (seat != null) seat.ExitStation(Networking.LocalPlayer);
        }

        // 1. 射撃＆照準 (アムロ・スタイル)
        VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        if (robotArm != null)
        {
            Vector3 lookTarget = headData.position + (headData.rotation * Vector3.forward * 50f);
            robotArm.LookAt(lookTarget);
        }

        if (Input.GetButtonDown("Fire1")) { if (muzzleFlash != null) muzzleFlash.Play(); }
        else if (Input.GetButtonUp("Fire1")) { if (muzzleFlash != null) muzzleFlash.Stop(); }

        // 2. 回避（ブーストダッシュ）判定
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0)
        {
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
        }
        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // 3. 移動処理
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveInput = new Vector3(h, 0, v);
        float currentSpeed = (dashTimer > 0) ? dashForce : normalSpeed;
        transform.position += transform.rotation * moveInput * currentSpeed * Time.deltaTime;

        // 4. 視点追従（遊び付き・ぐるぐる防止）
        Vector3 lookDir = headData.rotation * Vector3.forward;
        lookDir.y = 0;
        float angleDiff = Vector3.SignedAngle(transform.forward, lookDir, Vector3.up);

        if (Mathf.Abs(angleDiff) > deadZoneAngle)
        {
            float targetAngle = angleDiff > 0 ? angleDiff - deadZoneAngle : angleDiff + deadZoneAngle;
            transform.Rotate(0, targetAngle * turnSmoothing * Time.deltaTime, 0);
        }
    }
}