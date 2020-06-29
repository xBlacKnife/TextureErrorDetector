using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using MiniJSON;
using System;
using Cinemachine;
using UnityEngine.UI;

struct CameraPosition
{
    public float X;
    public float Y;
    public float Z;
}
struct CameraRotation
{
    public float X;
    public float Y;
    public float Z;
    public float W;
}
struct ScreenshotInfo
{
    public string Name;
    public int Level;
    public CameraPosition Position;
    public CameraRotation Rotation;
    public ImageRoute ImagePath;
}

struct ImageRoute
{
    public string route;
}

public class ErrorViewerManager : Singleton<ErrorViewerManager>
{
    private string camerainfoRoute;
    private List<ScreenshotInfo> _screenshotinfo_list;

    int screenshot_index = 0;
    [HideInInspector]
    public bool testing_started = false;
    [HideInInspector]
    public bool loading_next_level = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (testing_started)
        {
            DisableCinemachine();
            SetCamera();

            ShowImage();

        }
    }

    private void ShowImage()
    {
        StartCoroutine(GetImage());
    }
    IEnumerator GetImage()
    {
        string url = _screenshotinfo_list[screenshot_index].ImagePath.route;

        Texture2D tex;
        tex = new Texture2D(8, 8, TextureFormat.DXT1, false);
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            ErrorViewerCanvas.Instance.ImageViewer.GetComponent<RawImage>().texture = tex;
        }
    }

    public void NextError()
    {
        if (CheckValidList() && !loading_next_level)
        {
            screenshot_index = (screenshot_index + 1) % _screenshotinfo_list.Count;
            StartCoroutine(LoadNextError());
        }
    }

    public void PrevError()
    {
        if (CheckValidList() && !loading_next_level)
        {
            screenshot_index--;
            if (screenshot_index < 0)
                screenshot_index = _screenshotinfo_list.Count - 1;

            StartCoroutine(LoadNextError());
        }
    }

    private bool CheckValidList()
    {
        return _screenshotinfo_list != null && _screenshotinfo_list.Count > 0;
    }

    public void StartTest()
    {
        camerainfoRoute = Application.dataPath + "/Resources/camerainfo.json";
        if (File.Exists(camerainfoRoute))
        {
            string fileText = File.ReadAllText(camerainfoRoute);
            Dictionary<string, object> cameraInfoDict = Json.Deserialize(fileText) as Dictionary<string, object>;

            CreateScreenshotList(cameraInfoDict);
        }

        testing_started = true;

        if (CheckValidList() && !loading_next_level)
            StartCoroutine(LoadNextError());
    }

    private void DisableCinemachine()
    {
        CinemachineBrain CB = FindObjectOfType<Cinemachine.CinemachineBrain>();

        if (CB && CB.GetComponent<Cinemachine.CinemachineBrain>())
            CB.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
    }

    // Update is called once per frame
    private void SetCamera()
    {
        if (CheckValidList())
        {
            Camera.main.transform.position = new Vector3(_screenshotinfo_list[screenshot_index].Position.X,
                _screenshotinfo_list[screenshot_index].Position.Y,
                _screenshotinfo_list[screenshot_index].Position.Z);

            Camera.main.transform.rotation = new Quaternion(_screenshotinfo_list[screenshot_index].Rotation.X,
                _screenshotinfo_list[screenshot_index].Rotation.Y,
                _screenshotinfo_list[screenshot_index].Rotation.Z,
                _screenshotinfo_list[screenshot_index].Rotation.W);
        }
    }

    IEnumerator LoadNextError()
    {
        loading_next_level = true;

        AsyncOperation load = SceneManager.LoadSceneAsync("Level" + _screenshotinfo_list[screenshot_index].Level);

        while (!load.isDone)
            yield return null;

        loading_next_level = false;
    }


    private void CreateScreenshotList(Dictionary<string, object> cameraInfoDict)
    {
        _screenshotinfo_list = new List<ScreenshotInfo>();

        foreach (KeyValuePair<string, object> info in cameraInfoDict)
        {
            ScreenshotInfo new_screenshot = new ScreenshotInfo();
            new_screenshot.Name = info.Key;

            object level;
            if ((info.Value as Dictionary<string, object>).TryGetValue("Level", out level))
            {
                new_screenshot.Level = int.Parse(level.ToString()) - 1;
            }

            object cam_pos;
            if ((info.Value as Dictionary<string, object>).TryGetValue("Camera_Position", out cam_pos))
            {
                object x, y, z;

                (cam_pos as Dictionary<string, object>).TryGetValue("X", out x);
                (cam_pos as Dictionary<string, object>).TryGetValue("Y", out y);
                (cam_pos as Dictionary<string, object>).TryGetValue("Z", out z);

                new_screenshot.Position = new CameraPosition()
                {
                    X = float.Parse(x.ToString()),
                    Y = float.Parse(y.ToString()),
                    Z = float.Parse(z.ToString())
                };
            }

            object cam_rot;
            if ((info.Value as Dictionary<string, object>).TryGetValue("Camera_Rotation", out cam_rot))
            {
                object x, y, z, w;

                (cam_rot as Dictionary<string, object>).TryGetValue("X", out x);
                (cam_rot as Dictionary<string, object>).TryGetValue("Y", out y);
                (cam_rot as Dictionary<string, object>).TryGetValue("Z", out z);
                (cam_rot as Dictionary<string, object>).TryGetValue("W", out w);

                new_screenshot.Rotation = new CameraRotation()
                {
                    X = float.Parse(x.ToString()),
                    Y = float.Parse(y.ToString()),
                    Z = float.Parse(z.ToString()),
                    W = float.Parse(w.ToString())
                };
            }

            object image_route;
            if ((info.Value as Dictionary<string, object>).TryGetValue("Image_Directory", out image_route))
            {
                object directory;

                (image_route as Dictionary<string, object>).TryGetValue("Image", out directory);

                new_screenshot.ImagePath = new ImageRoute()
                {
                    route = directory.ToString()
                };
            }

            _screenshotinfo_list.Add(new_screenshot);
        }
    }
}
