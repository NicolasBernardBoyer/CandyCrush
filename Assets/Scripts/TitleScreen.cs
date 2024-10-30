using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    
    public void GoToLevelOne()
    {
        Debug.Log("Loading Level One");
        SceneManager.LoadScene(1);
    }

    public void GoToLevelTwo()
    {
        Debug.Log("Loading Level Two");
        SceneManager.LoadScene(2);
    }
}
