using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class MainMenu : MonoBehaviour
{
    public HowToScreen HowToScreen;

    public void HowToButton() {
        HowToScreen.Setup();
    }
    public void StartButton()
    {

        SceneManager.LoadScene("Game");
    }
    
}
