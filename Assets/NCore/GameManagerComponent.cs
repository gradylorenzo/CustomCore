using UnityEngine;
using NCore.Managers;

public class GameManagerComponent : MonoBehaviour
{
    [Header("Load the scene at index by default if > 0")]
    public int defaultScene = 0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EventManager.UpdateSettings += UpdateSettings;
    }

    private void Start()
    {
        GameManager.Start(defaultScene);
    }

    //Default event subscribers to prevent null reference exceptions.
    public void UpdateSettings()
    {

    }
}
