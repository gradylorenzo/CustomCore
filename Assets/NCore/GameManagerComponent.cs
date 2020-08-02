using UnityEngine;
using NCore.Managers;
using NCore.Settings;

public class GameManagerComponent : MonoBehaviour
{
    [Header("Load the scene at index by default if > 0")]
    public int defaultScene = 1;

    private bool showNotice = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameManager.Awake();
    }

    private void Start()
    {
        if (Settings.IO.SettingsExists())
        {
            GameManager.Start(defaultScene);
        }
    }
}