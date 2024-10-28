
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    static float fps;
    float updateTimer = 0.2f;

    [SerializeField] TextMeshProUGUI fpsTitle;

    public float Fps // Make this an instance property
    {
        get { return fps; }
    }

    private void updateFPS()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer < 0 )
        {
            updateTimer = 0.2f;

            fps = 1f / Time.unscaledDeltaTime;
            fpsTitle.text = "FPS: "+ Mathf.Round(fps);
        }
    }

    void Update()
    {
        updateFPS();
    }
}
