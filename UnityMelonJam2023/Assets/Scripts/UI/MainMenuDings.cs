using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDings : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance?.PlaySound(AUDIOTYPE.MAINMENU_BUTTON);
        GameManager.Instance?.LoadScene(LEVEL.LEVEL_0);
    }

    public void CloseGame()
    {
        GameManager.Instance?.PlaySound(AUDIOTYPE.MAINMENU_BUTTON); // prob not good enough
        GameManager.Instance?.CloseGame();
    }
}
