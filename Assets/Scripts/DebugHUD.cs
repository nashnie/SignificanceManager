using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    private TextMesh debugText;
    // Start is called before the first frame update
    void Start()
    {
        debugText = gameObject.GetComponentInChildren<TextMesh>();
    }

    public void ShowDebugView(float significance, bool shouldDisplayDebug)
    {
        debugText.gameObject.SetActive(shouldDisplayDebug);
        if (significance > 0f)
        {
            if (shouldDisplayDebug)
            {
                if (debugText)
                {
                    debugText.color = Color.green * significance;
                }
            }
        }
        else
        {
            if (shouldDisplayDebug)
            {
                if (debugText)
                {
                    debugText.color = Color.red;
                }
            }
        }
    }
}
