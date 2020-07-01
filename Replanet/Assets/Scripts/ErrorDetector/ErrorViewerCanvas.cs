using System;
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

        if (FindObjectsOfType<ErrorViewerCanvas>().Length > 1)
            Destroy(this.gameObject);

    }

    public GameObject [] LeftRightButtons;
    public GameObject StartButton, ImageViewer, CompareButton;
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

    public void EnableCompareModeTest()
    {
        CompareButton.SetActive(false);
        ScreenShotManager.Instance.originalPhotoCompareMode = true;
    }

    public void NextError()
    {
        ErrorViewerManager.Instance.NextError();
    }

    public void PrevError()
    {
        ErrorViewerManager.Instance.PrevError();
    }

    public void AnalyzeScreenShots()
    {
        ErrorViewerManager.Instance.ExeBatchFile(Application.dataPath + "../../../Error Detector/", "run.bat");
    }
}
