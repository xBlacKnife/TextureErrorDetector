using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using MiniJSON;
using System;
using Cinemachine;
using UnityEngine.UI;
using System.Diagnostics;

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
    public string Scene;
    public CameraPosition Position;
    public CameraRotation Rotation;
    public ImageRoute ImagePath;
    public int Width;
    public int Height;
    public int NumOfObjects;
    public List<KeyValuePair<int, int>> ObjectsPositions;
}

struct ImageRoute
{
    public string route;
}
public class ErrorViewerManager : Singleton<ErrorViewerManager>
{
    public GameObject error_circle_go;

    private string camera_info_route;
    private string error_info_route;
    private List<ScreenshotInfo> _screenshotinfo_list;

    int screenshot_index = 0;
    [HideInInspector]
    public bool testing_started = false;
    [HideInInspector]
    public bool loading_next_level = false;

    private int numImages = 0;

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

            SetErrorCircle();

            ShowImage();
        }
    }


    public void TakeNextCompareImage()
    {
        ScreenShotManager.Instance.TakeCompareImages(numImages.ToString());
        numImages++;
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
        camera_info_route = Application.dataPath + "../../../Error Detector/camerainfo.json";
        error_info_route = Application.dataPath + "../../../Error Detector/texture_error.json";
        if (File.Exists(camera_info_route) && File.Exists(error_info_route))
        {
            string fileText_0 = File.ReadAllText(camera_info_route);
            Dictionary<string, object> cameraInfoDict = Json.Deserialize(fileText_0) as Dictionary<string, object>;

            string fileText_1 = File.ReadAllText(error_info_route);
            Dictionary<string, object> errorInfoDict = Json.Deserialize(fileText_1) as Dictionary<string, object>;

            CreateScreenshotList(cameraInfoDict, errorInfoDict);
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

    private void SetErrorCircle()
    {
        if (CheckValidList())
        {
            for (int i = 0; i < _screenshotinfo_list[screenshot_index].NumOfObjects; i++)
            {
                GameObject GO = Instantiate(error_circle_go, Camera.main.transform);
                GO.transform.parent = null;

                Vector3 screenPos = new Vector3(
                    (_screenshotinfo_list[screenshot_index].ObjectsPositions[i].Value * Camera.main.pixelWidth) / _screenshotinfo_list[screenshot_index].Width,
                    Camera.main.pixelHeight - ((_screenshotinfo_list[screenshot_index].ObjectsPositions[i].Key * Camera.main.pixelHeight) / _screenshotinfo_list[screenshot_index].Height), 0);

                Vector3 newPos = Camera.main.ScreenToWorldPoint(screenPos);

                GO.transform.position = newPos;
            }
        }
    }

    IEnumerator LoadNextError()
    {
        loading_next_level = true;

        AsyncOperation load = SceneManager.LoadSceneAsync(_screenshotinfo_list[screenshot_index].Scene);

        while (!load.isDone)
            yield return null;

        loading_next_level = false;
    }


    private void CreateScreenshotList(Dictionary<string, object> cameraInfoDict, Dictionary<string, object> errorInfoDict)
    {
        _screenshotinfo_list = new List<ScreenshotInfo>();

        foreach (KeyValuePair<string, object> info in cameraInfoDict)
        {
            if (errorInfoDict.ContainsKey(info.Key))
            {
                object image_info, error_percent;
                errorInfoDict.TryGetValue(info.Key, out image_info);

                (image_info as Dictionary<string, object>).TryGetValue("Porcentaje_Fallo", out error_percent);
                float error = float.Parse(error_percent.ToString());

                if (error > 0)
                {
                    ScreenshotInfo new_screenshot = new ScreenshotInfo();
                    new_screenshot.Name = info.Key;

                    // ESCENA
                    object scene;
                    if ((info.Value as Dictionary<string, object>).TryGetValue("Scene", out scene))
                    {
                        new_screenshot.Scene = scene.ToString();
                    }

                    // POSICION DE LA CAMARA
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

                    // ROTACION DE LA CAMARA
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

                    object width, height;
                    if ((info.Value as Dictionary<string, object>).TryGetValue("Width", out width))
                    {
                        new_screenshot.Width = int.Parse(width.ToString());
                    }

                    if ((info.Value as Dictionary<string, object>).TryGetValue("Height", out height))
                    {
                        new_screenshot.Height = int.Parse(height.ToString());
                    }

                    // RUTA DE LA IMAGEN
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

                    // PIXELES MARCADOS

                    object num_obj;
                    (image_info as Dictionary<string, object>).TryGetValue("Numero_Objetos", out num_obj);
                    new_screenshot.NumOfObjects = int.Parse(num_obj.ToString());

                    object pos_obj;
                    (image_info as Dictionary<string, object>).TryGetValue("Posicion_Objetos", out pos_obj);

                    new_screenshot.ObjectsPositions = new List<KeyValuePair<int, int>>();
                    foreach(object pos in (pos_obj as List<object>))
                    {
                        object x, y;
                        (pos as Dictionary<string, object>).TryGetValue("x", out x);
                        (pos as Dictionary<string, object>).TryGetValue("y", out y);

                        new_screenshot.ObjectsPositions.Add(new KeyValuePair<int, int>(int.Parse(x.ToString()), int.Parse(y.ToString())));
                    }

                    _screenshotinfo_list.Add(new_screenshot);
                }
            }
        }
    }

    public bool ExeBatchFile(string directory, string filename)
    {
        Process foo = new Process();
        foo.StartInfo.FileName = filename;
        foo.StartInfo.WorkingDirectory = directory;
        foo.StartInfo.CreateNoWindow = false;
        foo.Start();
        foo.WaitForExit();
        foo.Dispose();

        return true;
    }
}
