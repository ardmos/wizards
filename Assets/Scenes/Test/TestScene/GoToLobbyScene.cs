using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToLobbyScene : MonoBehaviour
{
    public void OnButtonClicked()
    {
        SceneManager.LoadScene("TestLobbyScene");
    }
}
