using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCore;
using NCore.Managers;
using UnityEngine.SceneManagement;

public class GameManagerComponent : MonoBehaviour
{
    public int defaultScene;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventManager.UpdateSettings += UpdateSettings;
    }

    private void Start()
    {
        if(defaultScene > -1)
        SceneManager.LoadScene(defaultScene);
    }

    //Default event subscribers to prevent null reference exceptions.
    public void UpdateSettings()
    {

    }
}
