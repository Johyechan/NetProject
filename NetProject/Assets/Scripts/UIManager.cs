using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject playUI;
    public GameObject user;
    public GameObject FirebaseEvent;

    public void LoginPanel()
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }

    public void RegisterPanel()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void CloseLogin()
    {
        loginUI.SetActive(false);
        user.SetActive(true);
        playUI.SetActive(true);
    }

    public void StartGame()
    {
        FirebaseEvent.SetActive(true);
    }
}
