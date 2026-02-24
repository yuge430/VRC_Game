using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ChairTrigger : UdonSharpBehaviour
{
    [Header("Robot Settings")]
    public GameObject robotRoot;
    public Transform robotArm;
    public ParticleSystem muzzleFlash;
    public GameObject sniperScope;
    public AudioSource shootSound;

    [Header("Game System")]
    public CoreGameManager gameManager;
    public PlayerStatsManager myStats;
    public Text hpText;

    [Header("Movement Settings")]
    public float normalSpeed = 5.0f;
    public float dashForce = 20.0f;
    public float turnSmoothing = 5.0f;
    public float deadZoneAngle = 30.0f;

    private bool isSeated = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isSeated = true;
            Networking.SetOwner(player, robotRoot);
            myStats = gameManager.GetStatsByPlayer(player);
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            isSeated = false;
            if (muzzleFlash != null) muzzleFlash.Stop();
            if (shootSound != null) shootSound.Stop();
            if (sniperScope != null) sniperScope.SetActive(false);
            myStats = null;
        }
    }

    void LateUpdate()
    {
        if (!isSeated)
        {
            if (muzzleFlash != null && muzzleFlash.isPlaying) muzzleFlash.Stop();
            if (shootSound != null && shootSound.isPlaying) shootSound.Stop();
            return;
        }

        if (myStats != null && hpText != null)
        {
            hpText.text = "ENERGY: " + myStats.totalPoints.ToString();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            GetComponent<VRCStation>().ExitStation(Networking.LocalPlayer);
        }

        bool isAiming = Input.GetMouseButton(1);
        VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        if (sniperScope != null)
        {
            sniperScope.SetActive(isAiming);
            if (isAiming)
            {
                sniperScope.transform.position = headData.position + (headData.rotation * Vector3.forward * 0.5f);
                sniperScope.transform.rotation = headData.rotation;
            }
        }

        if (isAiming)
        {
            if (robotArm != null)
            {
                Vector3 lookTarget = headData.position + (headData.rotation * Vector3.forward * 50f);
                robotArm.LookAt(lookTarget);
            }
            if (Input.GetButtonDown("Fire1")) 
            { 
                if (muzzleFlash != null) muzzleFlash.Play();
                if (shootSound != null) shootSound.Play();
            }
            else if (Input.GetButtonUp("Fire1")) 
            { 
                if (muzzleFlash != null) muzzleFlash.Stop();
                if (shootSound != null) shootSound.Stop();
            }
        }
        else
        {
            if (muzzleFlash != null && muzzleFlash.isPlaying) muzzleFlash.Stop();
            if (shootSound != null && shootSound.isPlaying) shootSound.Stop();
            if (robotArm != null) robotArm.localRotation = Quaternion.Slerp(robotArm.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }

        // --- 移動処理 ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0)
        {
            dashTimer = 0.2f;
            cooldownTimer = 1.0f;
        }
        if (dashTimer > 0) dashTimer -= Time.deltaTime;
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float speed = (dashTimer > 0) ? dashForce : normalSpeed;
        robotRoot.transform.position += robotRoot.transform.rotation * new Vector3(h, 0, v) * speed * Time.deltaTime;

        Vector3 lookDir = headData.rotation * Vector3.forward;
        lookDir.y = 0;
        float angleDiff = Vector3.SignedAngle(robotRoot.transform.forward, lookDir, Vector3.up);

        if (Mathf.Abs(angleDiff) > deadZoneAngle)
        {
            float targetAngle = angleDiff > 0 ? angleDiff - deadZoneAngle : angleDiff + deadZoneAngle;
            robotRoot.transform.Rotate(0, targetAngle * turnSmoothing * Time.deltaTime, 0);
        }
    }

    public void OnParticleCollision(GameObject other)
    {
        if (myStats != null)
        {
            myStats.AddPoints(-100);
        }
    }
}