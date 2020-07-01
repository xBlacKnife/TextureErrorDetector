using MiniJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageComparatorManager : MonoBehaviour
{
    public RawImage OriginalImage, ComparedImage, ResultImage;
    private List<string []> _screenshotinfo_list;
    string GenericPath, jsonInfo;

    private int index = 0;
    private bool showingImage = false;

    private void Start()
    {
        GenericPath = Application.dataPath + "../../../ImageCompare/";

        jsonInfo = GenericPath + "CompareLog.json";

        if (File.Exists(jsonInfo))
        {
            string fileText_0 = File.ReadAllText(jsonInfo);
            Dictionary<string, object> compare_info_dict = Json.Deserialize(fileText_0) as Dictionary<string, object>;

            CreateScreenshotList(compare_info_dict);
        }

        ShowImage();

    }

    public void Next()
    {
        index++;
        if (index > _screenshotinfo_list.Count - 1)
            index = 0;

        ShowImage();
    }

    public void Prev()
    {
        index++;
        if (index < 0)
            index = _screenshotinfo_list.Count - 1;

        ShowImage();
    }


    private void ShowImage()
    {
        if(!showingImage)
            StartCoroutine(GetImage());
    }
    IEnumerator GetImage()
    {
        showingImage = true;

        string comparedRoute = GenericPath + "images/Captures/";
        string originalsRoute = GenericPath + "images/Original/";
        string resultRoute = GenericPath + "result/";

        ///Original Texture
        Texture2D originalTex;
        originalTex = new Texture2D(8, 8, TextureFormat.DXT1, false);
        using (WWW www = new WWW(originalsRoute + _screenshotinfo_list[index][0]))
        {
            yield return www;
            www.LoadImageIntoTexture(originalTex);
            
        }

        ///Compared Texture
        Texture2D comparedTex;
        comparedTex = new Texture2D(8, 8, TextureFormat.DXT1, false);
        using (WWW www = new WWW(comparedRoute + _screenshotinfo_list[index][0]))
        {
            yield return www;
            www.LoadImageIntoTexture(comparedTex);
          
        }

        ///Result Texture
        Texture2D resultTex;
        resultTex = new Texture2D(8, 8, TextureFormat.DXT1, false);
        using (WWW www = new WWW(resultRoute + _screenshotinfo_list[index][1] + ".jpg"))
        {
            yield return www;
            www.LoadImageIntoTexture(resultTex);
        }

        OriginalImage.texture = originalTex;
        ComparedImage.texture = comparedTex;
        ResultImage.texture = resultTex;

        showingImage = false;
    }


    private void CreateScreenshotList(Dictionary<string, object> InfoDict)
    {
        _screenshotinfo_list = new List<string []>();     

        foreach (KeyValuePair<string, object> info in InfoDict)
        {
            string [] s = new string[2];

            object ImageName;
            if ((info.Value as Dictionary<string, object>).TryGetValue("Image", out ImageName))
            {
                s[0] = ImageName.ToString();
                s[1] = info.Key.ToString();
            }
            _screenshotinfo_list.Add(s);

        }

    }

}
