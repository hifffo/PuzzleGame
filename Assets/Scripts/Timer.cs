using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    #region Variables
    [Header("Text On Pause")]
    //ссылка на текст "Your Time" на панели паузы
    public Text TextPauseYourVar; 

    [Header("Text time")]
    //ссылка на текст времени игры
    public Text timertext;

    [Header("Pause Canvas")]
    //ссылка на объект панели паузы
    public GameObject pauseCanvas;
    
    //время игры
    float sec = 0;
    //начата ли игра
    bool ison = false;
    //стоит ли пауза
    bool isgamepaused = false;
    #endregion

    void FixedUpdate()
    {
        //если игра начата, то считаем время игры
        if (ison) sec += Time.deltaTime;
        //выводим текущее время
        timertext.text = "TIME: " + GetGoodTime(sec);
        //если нажата Back и пауза не стоит
        if (ison && Input.GetKeyDown(KeyCode.Escape) && !isgamepaused)
            PauseOnOff(true);
        else// иначе, если нажата back и стоит пауза
        if (ison && Input.GetKeyDown(KeyCode.Escape) && isgamepaused)
          PauseOnOff(false);
    }

    //включение выключение паузы
    public void PauseOnOff(bool pause)
    {
        //если игра идёт и нужно включить паузу
        if (ison && pause)
        {
            //активируем панель паузы
            pauseCanvas.SetActive(true);
            //останавливаем время
            Time.timeScale = 0;
            //отмечаем что стоит пауза
            isgamepaused = true;
            //выводим текущее время в панель паузы
            TextPauseYourVar.text = GetGoodTime(sec);
        }
        else // иначе
        if (ison)
        {
            //выключаем панель паузы
            pauseCanvas.SetActive(false);
            //возобновляем время
            Time.timeScale = 1;
            //отмечаем что паузы нет
            isgamepaused = false;
        }
    }

    //отмечаем, начата ли игра или нет
    public void IsOn(bool b)
    {
        ison = b;
    }

    #region Returns
    //получить текущее время
    public float GetTime()
    {
        return sec;
    }

    //возращает время преобразованное в строку формата "mm:ss"
    public static string GetGoodTime(float sec)
    {
        //преобразуем время из float в int
        int secint = System.Convert.ToInt32(sec);

        //условия преобразования в строку
        if (secint < 10) return "00:0" + secint;
        else
            if (secint < 60) return "00:" + secint;
        else
            if (secint < 600)
        {
            if (secint % 60 < 10)
                return "0" + (secint / 60) + ":0" + (secint % 60);
            if (secint % 60 > 9)
                return "0" + (secint / 60) + ":" + (secint % 60);

        }
        else
        {
            if (secint % 60 < 10)
                return (secint / 60) + ":0" + (secint % 60);
            if (secint % 60 > 9)
                return (secint / 60) + ":" + (secint % 60);

        }
        //возвращаем строку
        return System.Convert.ToString(secint);
    }

    //возращает истину, если можно нажимать на ячейки
    //ложь, если нельзя
    public bool CanClick()
    {
        if (ison && !isgamepaused)
            return true;
        else
            return false;
    }
    #endregion
}
