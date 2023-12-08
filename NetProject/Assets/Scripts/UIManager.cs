using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject changePasswordUI;
    public GameObject showFriendsView;
    public GameObject startUI;
    public GameObject gameUI;
    public GameObject endUI;
    public GameObject startButton;

    public GameObject enemy;

    public bool show = false;
    public bool game = false;
    public bool changeColor = false;

    public void GamePanel()
    {
        Debug.Log("ddd");
        game = true;
        changeColor = true;
        GameManager.Instance.Hp.value = 1;
        GameManager.Instance.timeline.value = 1;
        GameManager.Instance.level = 10;
        startUI.SetActive(false);
        showFriendsView.SetActive(false);
        gameUI.SetActive(true);
        enemy.SetActive(true);
    }

    public void EndPanel()
    {
        enemy.SetActive(false);
        gameUI.SetActive(false);
        endUI.SetActive(true);
    }

    public void StartPanel()
    {
        endUI.SetActive(false);
        startUI.SetActive(true);
    }

    public void GameEnd()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }

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
            startButton.SetActive(false);
            show = true;
        }
        else
        {
            showFriendsView.SetActive(false);
            startButton.SetActive(true);
            show = false;
        }
    }

    public void CloseLogin()
    {
        loginUI.SetActive(false);
        startUI.SetActive(true);
    }
}
