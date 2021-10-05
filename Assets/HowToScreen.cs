using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class HowToScreen : MonoBehaviour
{


    public void Setup()
    {
        gameObject.SetActive(true);
        
    }


    public void BackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
