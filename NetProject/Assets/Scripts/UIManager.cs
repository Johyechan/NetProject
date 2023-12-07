using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject changePasswordUI;
    public GameObject showFriendsView;

    public bool show = false;
    //public GameObject playUI;
    //public GameObject user;
    //public GameObject FirebaseEvent;

    public void LoginPanel()
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        changePasswordUI.SetActive(false);
    }

    public void RegisterPanel()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void ChangePasswordPanel()
    {
        loginUI.SetActive(false);
        changePasswordUI.SetActive(true);
    }

    public void ShowFriends()
    {
        if (showFriendsView.activeSelf == false)
        {
            showFriendsView.SetActive(true);
            show = true;
        }
        else
        {
            showFriendsView.SetActive(false);
            show = false;
        }
    }

    public void CloseLogin()
    {
        loginUI.SetActive(false);
        //user.SetActive(true);
        //playUI.SetActive(true);
    }

    //public void StartGame()
    //{
    //    FirebaseEvent.SetActive(true);
    //}
}
