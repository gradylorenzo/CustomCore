using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NyxtonCore;
using NyxtonCore.Managers;
using UnityEngine.SceneManagement;

public class GameManagerComponent : MonoBehaviour
{
    public Scene defaultScene;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventManager.UpdateSettings += UpdateSettings;
    }

    private void Start()
    {
        SceneManager.LoadScene(defaultScene.name);
    }

    //Default event subscribers to prevent null reference exceptions.
    public void UpdateSettings()
    {

    }
}
