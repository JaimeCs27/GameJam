using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public Camera mainCam;
    public GameObject uiPanel;
    public TMP_Text promptText;
    public bool isDisplayed = false;
    public void Start()
    {
        mainCam = Camera.main;
        uiPanel.SetActive(false);
    }
    public void SetUp(string message)
    {
        promptText.text = message;
        uiPanel.SetActive(true);
        isDisplayed = true;
    }

    public void Close()
    {
        uiPanel.SetActive(false);
        isDisplayed = false;
        promptText.text = "";
    }

}
