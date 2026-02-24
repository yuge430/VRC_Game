using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class GameUIController : UdonSharpBehaviour
{
    [Header("Managers")]
    public CoreGameManager gameManager;
    public CapturePoint[] capturePoints; 

    [Header("Top Ticket Bars")]
    public Image blueTicketFill;
    public Image redTicketFill;
    public Text blueTicketText;
    public Text redTicketText;

    [Header("Point Progress Bars")]
    public Image[] bluePointBars; 
    public Image[] redPointBars;  

    void LateUpdate()
    {
        if (gameManager == null) return;

        float bRatio = (float)gameManager.blueTickets / gameManager.maxTickets;
        float rRatio = (float)gameManager.redTickets / gameManager.maxTickets;
        blueTicketFill.fillAmount = bRatio;
        redTicketFill.fillAmount = rRatio;
        blueTicketText.text = gameManager.blueTickets.ToString();
        redTicketText.text = gameManager.redTickets.ToString();
        for (int i = 0; i < capturePoints.Length; i++)
        {
            if (capturePoints[i] == null) continue;
            float p = capturePoints[i].GetProgress(); 
            
            if (p >= 0) {
                bluePointBars[i].fillAmount = p;
                redPointBars[i].fillAmount = 0;
            } else {
                bluePointBars[i].fillAmount = 0;
                redPointBars[i].fillAmount = Mathf.Abs(p);
            }
        }
    }
}