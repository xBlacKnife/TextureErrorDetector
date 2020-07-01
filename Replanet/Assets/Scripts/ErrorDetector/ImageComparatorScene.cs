using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImageComparatorScene : MonoBehaviour
{
    public void StartComparatorScene()
    {
        SceneManager.LoadScene("ComparatorTestScene");
    }

    public void StartMainMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

}
