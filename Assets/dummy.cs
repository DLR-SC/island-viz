using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dummy : MonoBehaviour {


    TMPro.TMP_Text text;
	// Use this for initialization
	void Start () {
        text = GetComponent<TMPro.TMP_Text>();
        InvokeRepeating("overflowing", 2f, 1f);
	}

    void overflowing()
    {
        Debug.Log("Overflow in Text: " + text.isTextOverflowing);
    }
}
