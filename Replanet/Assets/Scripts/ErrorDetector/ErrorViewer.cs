using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using MiniJSON;

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
}
public class ErrorViewer : MonoBehaviour
{
    private string camerainfoRoute;
    private List<ScreenshotInfo> _screenshotinfo_list;

    int screenshot_index = 0;
    bool testing_started = false;
    // Start is called before the first frame update
    void Start()
    {
        camerainfoRoute = Application.dataPath + "/Resources/camerainfo.json";
        if (File.Exists(camerainfoRoute))
        {
            string fileText = File.ReadAllText(camerainfoRoute);
            Dictionary<string, object> cameraInfoDict = Json.Deserialize(fileText) as Dictionary<string, object>;

            CreateScreenshotList(cameraInfoDict);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_screenshotinfo_list != null && _screenshotinfo_list.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)){
                if (Input.GetKeyDown(KeyCode.T) && !testing_started)
                {
                    testing_started = true;                   
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) && testing_started)
                {
                    screenshot_index--;
                    if (screenshot_index < 0)
                        screenshot_index = _screenshotinfo_list.Count - 1;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) && testing_started)
                {
                    screenshot_index = (screenshot_index + 1) % _screenshotinfo_list.Count;
                }
                SceneManager.LoadScene("Level" + _screenshotinfo_list[screenshot_index].Level);
            }
        }
        if (testing_started)
        {
            FindObjectOfType<Cinemachine.CinemachineBrain>().GetComponent<Cinemachine.CinemachineBrain>().enabled = false;

            Camera.main.transform.position = new Vector3(_screenshotinfo_list[screenshot_index].Position.X,
                _screenshotinfo_list[screenshot_index].Position.Y,
                _screenshotinfo_list[screenshot_index].Position.Z);

            Camera.main.transform.rotation = new Quaternion(_screenshotinfo_list[screenshot_index].Rotation.X,
                _screenshotinfo_list[screenshot_index].Rotation.Y,
                _screenshotinfo_list[screenshot_index].Rotation.Z,
                _screenshotinfo_list[screenshot_index].Rotation.W);
        }
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

            _screenshotinfo_list.Add(new_screenshot);
        }
    }
}
