using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UI_Button : MonoBehaviour
{
    public Image Image;
    public Text Text;
    public Color HighlightColor;
    public UnityEvent OnClick;

    private Color startColor;
    private bool selected;


    private void Start()
    {
        IslandVizInteraction.Instance.OnUIButtonSelected += OnButtonSelected;

        startColor = Image.color;
    }


    private void OnButtonSelected (UI_Button button, IslandVizInteraction.SelectionType selectionType, bool selected)
    {
        if (button != this)
        {
            return;
        }

        //Debug.Log(gameObject.name + " was selected with button = " + button.gameObject.name + " and slectionType = " + selectionType.ToString() + " and selected = " + selected);

        if (selectionType == IslandVizInteraction.SelectionType.Select && selected)
        {
            Debug.Log(gameObject.name + " was pressed.");
            Click();
        }
        else if (selectionType == IslandVizInteraction.SelectionType.Highlight)
        {
            Highlight(selected);
        }
    }


    public void Highlight (bool enable)
    {
        Image.color = enable ? HighlightColor : startColor;
    }

    public void Click ()
    {
        Debug.Log(gameObject.name + " was pressed!");

        if (OnClick != null)
            OnClick.Invoke();
    }
}
