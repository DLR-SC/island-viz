using System.Collections;
using System.Collections.Generic;
using IslandEvents;
using UnityEngine;
using UnityEngine.UI;

public class LogoAnimation : MonoBehaviour
{
    public Image Logo;
    public Image Island;
    public Image Viz;

    private float IslandOffset = 80f;
    private float VizOffset = 40f;


    private void Start()
    {
        Logo.color = new Color(1, 1, 1, 0);
        Island.transform.localPosition += Vector3.left * IslandOffset;
        Viz.transform.localPosition += Vector3.left * VizOffset;

        VizEvents.OnVisualizationInitilized += HideAnimation;

        StartAnimiation();
    }

    public void StartAnimiation ()
    {
        StartCoroutine(Animation());
    }

    public void HideAnimation ()
    {
        StartCoroutine(HideLogo());
    }

    IEnumerator Animation ()
    {
        // Fade in logo.
        float value = 0f;
        while (value <= 1f)
        {
            Logo.color = new Color(1, 1, 1, value);
            value += 2f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Logo.color = Color.white;

        // Slide in "Island".
        value = 0f;
        Vector3 startPos = Island.transform.localPosition;
        Vector3 targetPos = Island.transform.localPosition + Vector3.right * IslandOffset;
        while (value <= 1f)
        {
            Island.transform.localPosition = Vector3.Lerp(startPos, targetPos, value);
            value += 2f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Island.transform.localPosition = targetPos;

        // Slide in "Viz".
        value = 0f;
        startPos = Viz.transform.localPosition;
        targetPos = Viz.transform.localPosition + Vector3.right * VizOffset;
        while (value <= 1f)
        {
            Viz.transform.localPosition = Vector3.Lerp(startPos, targetPos, value);
            value += 2f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Viz.transform.localPosition = targetPos;
    }

    IEnumerator HideLogo ()
    {
        // Slide out "Viz".
        float value = 0f;
        var startPos = Viz.transform.localPosition;
        var targetPos = Viz.transform.localPosition + Vector3.left * VizOffset;
        while (value <= 1f)
        {
            Viz.transform.localPosition = Vector3.Lerp(startPos, targetPos, value);
            value += 4f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Viz.transform.localPosition = targetPos;

        // Slide out "Island".
        value = 0f;
        startPos = Island.transform.localPosition;
        targetPos = Island.transform.localPosition + Vector3.left * IslandOffset;
        while (value <= 1f)
        {
            Island.transform.localPosition = Vector3.Lerp(startPos, targetPos, value);
            value += 4f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Island.transform.localPosition = targetPos;

        // Fade out logo.
        value = 1f;
        while (value >= 0f)
        {
            Logo.color = new Color(1, 1, 1, value);
            value -= 4f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(false);
    }
}
