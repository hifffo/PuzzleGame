using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectDifficult : MonoBehaviour {

    #region Variables
    //Ссылка на GlobalVars
    GlobalVars gv;
    //Выбранный уровень
    SomeLevel level;
    #endregion

    void Start () {
        
        //сохранение ссылки на GlobalVars
        gv = GameObject.Find("GlobalVars").GetComponent<GlobalVars>();
        //присваиеваем картинке спрайт выбранного уровня
        GameObject.Find("SelectedImage").GetComponent<Image>().sprite = gv.GetSprite();
        //получаем информацио о уровне
        level = gv.GetLevel(gv.GetSprite().name);

        //цикл по ДОСТУПНЫМ уровням сложности, именно доступным
        for (int i = 0; i < level.difficult; i++)
        {
            //если уровень сложности доступен, то меняем спрайт звезды на зелёных
            GameObject.Find("star" + i).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/star");
            //если есть рекорд, то выводим его
            if (level.besttime[i]>0)
            GameObject.Find("besttime" + i).GetComponent<Text>().text = Timer.GetGoodTime(level.besttime[i]);
            
            //открываем кнопки на одну вперёд 
            if (i + 1 < 5)
            {
                int n = i + 1;
                //меняем спрайт на зелёный
                GameObject.Find("Button" + n).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/buttondif");
                //включем компонент button
                GameObject.Find("Button" + n).GetComponent<Button>().enabled = true;
            }
        }
    }

    #region Buttons
    //выбор уровня
    public void OnLevelClick(int dif)
    {
        gv.SelectDifficult(dif);
    }
    //нажатие на стрелку назад
    public void OnBackClick()
    {
        gv.BackToLevels();
    }
    #endregion
}
