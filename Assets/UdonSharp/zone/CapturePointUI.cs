using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CapturePointUI : UdonSharpBehaviour
{
    public CapturePoint targetPoint; 
    public Image blueBar;
    public Image redBar;
    public Text statusText; 

    void Update()
    {
        if (targetPoint == null) return;
        float progress = targetPoint.GetProgress(); 

        if (progress >= 0)
        {
            blueBar.fillAmount = progress;
            redBar.fillAmount = 0;
        }
        else
        {
            blueBar.fillAmount = 0;
            redBar.fillAmount = Mathf.Abs(progress);
        }
    }
}