using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NSaveListButton : MonoBehaviour
{
    public NMainMenuController mainMenu;
    public Text name;
    public Text timestamp;

    public string fileName;

    public void OnClick()
    {
        mainMenu.SetSelectedSave(fileName);
    }
}
