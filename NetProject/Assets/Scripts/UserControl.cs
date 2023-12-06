using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserControl : MonoBehaviour
{
    const float speed = 5.0f;
    const int MAX_HP = 100;
    const int DROP_HP = 1; //drop hp per second

    //public bool bMovable = false;
    public GameObject hpBar;

    //È¿°ú
    //public ParticleSystem fxParticle;

    public Vector3 targetPos = Vector3.zero;
    Vector3 orgPos;
    public bool isRemote = false;


    int currentHP;
    int maxHP;

    bool bMoving;


    void Start()
    {
        maxHP = MAX_HP;
        SetHP(MAX_HP);

        InvokeRepeating(nameof(DropSec), 1, 1);
    }

    // Update is called once per frame
    void Update()
    {

        if (!isRemote && Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                SetTargetPos();
            }
        }

        if (!isRemote && Input.GetMouseButtonDown(1))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (GetHP() > 0)
                {
                    GameManager.Instance.Attack();
                }
            }
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }


    public void SetTargetPos()
    {
        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameManager.Instance.SendCommand("#Move#" + targetPos.x + ',' + targetPos.y);
    }

    public void SetHP(int hp)
    {
        hp = Mathf.Clamp(hp, 0, maxHP);
        currentHP = hp;
        float ratio = (float)currentHP / (float)maxHP;
        hpBar.transform.localScale = new Vector3(ratio, 1f, 1f);
    }

    public void DropHP(int drop)
    {
        currentHP -= drop;
        SetHP(currentHP);
    }

    private void DropSec()
    {
        currentHP -= DROP_HP;
        SetHP(currentHP);
    }

    public void Revive()
    {
        SetHP(MAX_HP);
    }

    public int GetHP()
    {
        return currentHP;
    }
}
