using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;


public class PlayFabLobbyManager : MonoBehaviour
{
    [SerializeField] GameObject loginPannel, registrationPannel, photonConnectPannel;
    public Text usernameRegistration, userEmailRegistration, userPasswordRegistration, confirmUserePasswordRegistration, usernameLogin, userPasswordLogin, errorSignUp, errorLogin, currentUsername;
    public TextMeshProUGUI buttonText;
    string encryptedPassword;

    public void RegistrationPannelActive()
    {
        registrationPannel.SetActive(true);
        loginPannel.SetActive(false);
        //set the error to null sp no meesages are displayed 
        errorSignUp.text = "";
        errorLogin.text = "";
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public void LoginPannelActive()
    {
        registrationPannel.SetActive(false);
        loginPannel.SetActive(true);
        //set the error to null sp no meesages are displayed 
        errorSignUp.text = "";
        errorLogin.text = "";
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    //MAKE AN ENCRYPTION FUNCTION 
    string EncryptInfo(string info)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider encryptedData = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] byteStream = System.Text.Encoding.UTF8.GetBytes(info);
        byteStream = encryptedData.ComputeHash(byteStream);
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        foreach (byte b in byteStream)
        {
            stringBuilder.Append(b.ToString("x2").ToLower());
        }
        return stringBuilder.ToString();
    }
    public void SignUp()
    {
        if (confirmUserePasswordRegistration.text == userPasswordRegistration.text)
        {
            var resgisterRequest = new RegisterPlayFabUserRequest
            {
                Email = userEmailRegistration.text,
                Password = EncryptInfo(confirmUserePasswordRegistration.text),
                Username = usernameRegistration.text
            };
            PlayFabClientAPI.RegisterPlayFabUser(resgisterRequest, RegisterSuccess, RegisterError);
        }
        else
        {
            errorSignUp.text = "Error: Password does not match!";
        }
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    public void RegisterSuccess(RegisterPlayFabUserResult result)
    {
        errorSignUp.text = "";
        errorLogin.text = "";
        Debug.LogError("Account created!");
        LoginPannelActive();
    }
    public void LoginSuccess(LoginResult result)
    {
        errorSignUp.text = "";
        errorLogin.text = "";
        StartGame();
    }
    public void RegisterError(PlayFabError error)
    {
        errorSignUp.text = error.GenerateErrorReport();
    }
    public void LoginError(PlayFabError error)
    {
        buttonText.text = "Ok";
        errorLogin.text = error.GenerateErrorReport();
    }
    void StartGame()
    {
        registrationPannel.SetActive(false);
        loginPannel.SetActive(false);
        photonConnectPannel.SetActive(true);
        //start the game or create a new hub to create the login
        currentUsername.text = usernameLogin.text;
    }
    public void Login()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = usernameLogin.text,
            Password = EncryptInfo(userPasswordLogin.text),
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, LoginSuccess, LoginError);
        buttonText.text = "Connecting. . .";
        Debug.LogError("Encrypted password: " + EncryptInfo(userPasswordLogin.text));
        FindObjectOfType<AudioManager>().Play("Button_Pressed_Sound");
    }
    //method for sending data to the leaderboard
    public void SendDataLeaderBoard(int playerScore)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{
                new StatisticUpdate{
                    StatisticName = "Hero's Score",
                    Value = playerScore
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderBoardError);
    }
    void OnLeaderBoardError(PlayFabError error)
    {
        Debug.LogError("Something went wrong while saving leaderboard score.");
    }
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.LogError("New score has been added to the leaderboard.");
    }
}
