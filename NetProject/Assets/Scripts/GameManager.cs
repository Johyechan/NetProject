using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class GameManager : Singleton<GameManager>
{
    public TMP_Text levelText;

    public Slider Hp;
    public Slider timeline;
    public int level = 1;
    public bool powerUp = false;
    public float attackPower = 0.1f;
    public int bestStage = 1;

    private void Update()
    {
        levelText.text = "Level " + level;
        if (UIManager.Instance.game)
        {
            timeline.value -= Time.deltaTime / 10;
            if (Hp.value > 0 && timeline.value <= 0)
            {
                UIManager.Instance.game = false;
                UIManager.Instance.EndPanel();
                AuthManager.Instance.SaveAttackPower();
                AuthManager.Instance.SaveBestStage();
            }
            if (Hp.value <= 0 && timeline.value > 0)
            {
                powerUp = true;
                Hp.value = 1;
                timeline.value = 1;
                level += 1;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Hp.value -= (attackPower / level);
                Debug.Log($"{Hp.value}");
            }
        }
    }
}
