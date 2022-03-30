using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoseWinManager : MonoBehaviour
{
    public static void OnTryAgain()
    {
        SceneManager.LoadScene("CharSelect");
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public static void OnMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
        FindObjectOfType<AudioManager>().StopPlaying("Battle_Sound");
        FindObjectOfType<AudioManager>().Play("MainMenu_Sound");
        Debug.LogError("Music stopped.");
    }
}
