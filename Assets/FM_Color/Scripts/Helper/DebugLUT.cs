using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DebugLUT : MonoBehaviour {

    public FMColor FC;
    public Text DebugText;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(FC == null)
        {
            FC = Camera.main.GetComponent<FMColor>();
            return;
        }
        DebugText.text = (int)FC.LutMode + " / " + (Enum.GetNames(typeof(LutPack)).Length - 1) + " : " + FC.LutMode.ToString();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) FC.Action_PreviousLUT();
        if (Input.GetKeyDown(KeyCode.RightArrow)) FC.Action_NextLUT();
    }
}
