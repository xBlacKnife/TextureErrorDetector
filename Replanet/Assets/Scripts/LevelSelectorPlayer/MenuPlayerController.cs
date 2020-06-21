using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPlayerController : MonoBehaviour
{
    [SerializeField] private float aimSensitivity = 10f;
    [Range(0f, 1f)] [SerializeField] private float buildUpRot;
    [SerializeField] private float minInput;    

    // Aim
    private Vector3 aimTarget;
    private Vector3 forward;
    private Vector3 right;

    private MenuInputHandler playerInput;    
    

    #region Gets
    public float GetMinInput() { return minInput; }
    public float GetBuildUpRot() { return buildUpRot; }

    private string levelToChange = null;
    private int levelToChangeIdx = -1;

    public MessageFade message;
    
    #endregion

    void Awake()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        playerInput = GetComponent<MenuInputHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out LevelSelector result))
        {
            levelToChange = result.getLevel();
            levelToChangeIdx = result.getLevelIdx();
            if (levelToChangeIdx != -1)
            {
                message.FadeIn();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LevelSelector>() != null)
        {
            levelToChange = null;
            message.FadeOut();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            levelToChange = "Level" + 0;
            levelToChangeIdx = 0;
            Globals.maxLevel = 0;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            levelToChange = "Level" + 1;
            levelToChangeIdx = 1;
            Globals.maxLevel = 1;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            levelToChange = "Level" + 2;
            levelToChangeIdx = 2;
            Globals.maxLevel = 2;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            levelToChange = "Level" + 3;
            levelToChangeIdx = 3;
            Globals.maxLevel = 3;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            levelToChange = "Level" + 4;
            levelToChangeIdx = 4;
            Globals.maxLevel = 4;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            levelToChange = "Level" + 5;
            levelToChangeIdx = 5;
            Globals.maxLevel = 5;
            chooseLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            levelToChange = "Level" + 6;
            levelToChangeIdx = 6;
            Globals.maxLevel = 6;
            chooseLevel();
        }
    }
    public void chooseLevel()
    {
        if(levelToChange != null)
        {
            Globals.actualLevel = levelToChangeIdx + 1;
            LevelSelectorManager.instance.loadScene(levelToChange);
        }
        else
        {
            Debug.Log("levelToChange es null");
        }
    }

    public void Exit()
    {
        LevelSelectorManager.instance.exitToMenu();
    }
}
