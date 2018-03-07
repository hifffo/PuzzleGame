using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour {

    //нажатие на панель
    public void OnDown()
    {
        //выбираем спрайт и переходим в окно выбора сложности
        GameObject.Find("GlobalVars").GetComponent<GlobalVars>().SelectSprite(GetComponent<Image>().overrideSprite);
    }

}
