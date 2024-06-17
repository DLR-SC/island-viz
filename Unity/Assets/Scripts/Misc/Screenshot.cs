using UnityEngine;

/// <summary>
/// This class captures a screenshot when Space is pressed.
/// </summary>
public class Screenshot : MonoBehaviour
{
    public int Supersize;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScreenCapture.CaptureScreenshot("Screenshot.png", Supersize);
        }       
    }
}
