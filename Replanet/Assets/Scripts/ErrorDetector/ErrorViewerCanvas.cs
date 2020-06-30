﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorViewerCanvas : Singleton<ErrorViewerCanvas>
    {
    private void Awake()
    {
#if !UNITY_EDITOR //para que solo funcione en modo debug
         gameObject.SetActive(false);
#endif
    }

    public GameObject [] LeftRightButtons;
    public GameObject StartButton, ImageViewer;
    public void StartTest()
    {
        foreach(GameObject g in LeftRightButtons)
            g.SetActive(true);

        if (StartButton)
            StartButton.SetActive(false);

        if (ImageViewer)
            ImageViewer.SetActive(true);

        ErrorViewerManager.Instance.StartTest();
    }

    public void NextError()
    {
        ErrorViewerManager.Instance.NextError();
    }

    public void PrevError()
    {
        ErrorViewerManager.Instance.PrevError();
    }
}
