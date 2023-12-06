using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseEvent : MonoBehaviour
{
    public GameObject particleSnow;
    public GameObject particleRain;

    // Start is called before the first frame update
    void Start()
    {
        //³¯¾¾ ÀÌº¥Æ®
        string strWeather = AuthManager.Instance.GetWeather();
        Console.WriteLine(strWeather);
        switch (strWeather)
        {
            case "Snow":
                particleSnow.SetActive(true);
                break;
            case "Rain":
                particleRain.SetActive(true);
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
