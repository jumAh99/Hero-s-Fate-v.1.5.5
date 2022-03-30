using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    Resolution[] resolutionArr;
    public AudioMixer audioMixer;
    public Dropdown resolutionDropDown;
    void Start()
    {
        //get an array of all available resolutions on the host machines
        resolutionArr = Screen.resolutions;
        //delete the old options
        resolutionDropDown.ClearOptions();
        //create a list of strings because the add option takes only strings 
        List<string> optionsAvailable = new List<string>();
        //curent resolution
        int currentResolutionIndex = 0;
        //get all current options
        for (int i = 0; i < resolutionArr.Length; i++)
        {
            string optionResolution = resolutionArr[i].width + " x " + resolutionArr[i].height + " " + resolutionArr[i].refreshRate + "hz";
            optionsAvailable.Add(optionResolution);
            //select the current resolution
            if (resolutionArr[i].width == Screen.width && resolutionArr[i].height == Screen.height && resolutionArr[i].refreshRate == Screen.currentResolution.refreshRate)
            {
                currentResolutionIndex = i;
            }
        }
        //add the new options for the current screen 
        resolutionDropDown.AddOptions(optionsAvailable);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }
    //whenever the volume slider is moved this function will be called
    public void SetVolumeSlider(float volume)
    {
        //set the float level as the audio mixer current level using the slider
        audioMixer.SetFloat("volume", volume);
    }
    public void SetQuality(int qualityPreset)
    {
        //set the index to the quality present default to unity 0 low, 1 medium and 2 high
        QualitySettings.SetQualityLevel(qualityPreset);
        Debug.LogError("Current quality preset: " + QualitySettings.GetQualityLevel());
    }
    //set the bool value for the current screen state
    public void SetFullScreen(bool isFullScreen)
    {
        //the value will be true or false depending on what the user selects from the options menu
        Screen.fullScreen = isFullScreen;
    }
    public void SetResolution(int resolutionIndex)
    {
        Screen.SetResolution(resolutionArr[resolutionIndex].width, resolutionArr[resolutionIndex].height, Screen.fullScreen);
    }
}
