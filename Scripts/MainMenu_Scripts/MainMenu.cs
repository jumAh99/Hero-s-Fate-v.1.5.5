using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //go to char select
    public void playGame()
    {
        SceneManager.LoadScene("CharSelect");
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    //go to multiplayer
    public void OnMultiplayer()
    {
        SceneManager.LoadScene("ConnectingScene");
    }
    //close application 
    public void quitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    public void PlaySound()
    {
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
}
