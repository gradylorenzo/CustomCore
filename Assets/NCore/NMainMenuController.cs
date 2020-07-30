using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NMainMenuController : MonoBehaviour
{
    public enum MenuState
    {
        none,
        resume,
        confirm_resume,
        new_game,
        confirm_new_game,
        all_saves,
        confirm_delete_save,
        settings,
        credit,
        exit
    }
    public MenuState currentState;
    public MenuState previousState;

}
