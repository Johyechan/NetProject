using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class GameManager : Singleton<GameManager>
{
    public Slider Hp;
    public Slider timeline;
    public int level = 10;
    public bool powerUp = false;

    private void Update()
    {
        if(UIManager.Instance.game)
        {
            timeline.value -= Time.deltaTime / 10;
            if (Hp.value > 0 && timeline.value <= 0)
            {
                UIManager.Instance.game = false;
                UIManager.Instance.EndPanel();
            }
            if (Hp.value <= 0 && timeline.value > 0)
            {
                powerUp = true;
                Hp.value = 1;
                timeline.value = 1;
                level *= 2;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Hp.value -= (1.0f / level);
                Debug.Log($"{Hp.value}");
            }
        }
    }
}
