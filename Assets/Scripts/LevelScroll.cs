using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelScroll : MonoBehaviour
{

    #region Variables
    [Header("Prefabs")]
    //префаб панели
    public GameObject lpPrefab;
    //префаб текста заблокированного уровня
    public GameObject textlockPrefab;
    //префаб звёздочки
    public GameObject starPrefab;
    //префаб объекта GlobalVars
    public GameObject glvrPrefab;

    [Header("Space between panels")]
    [Range(0, 600)]
    //расстояние между панелями
    public int lpOffset;

    [Header("Scroll speed")]
    [Range(0f, 20f)]
    //скорость скроллинга
    public float scrollSpeed;

    [Header("Start increase panel when distance to center smaller then")]
    [Range(0f, 5f)]
    //расстояниее от центра, при котором начинается увеличение панелек 
    public float scaleOffset;

    [Header("Scroll View's rect")]
    //Rect Transform объекта ScrollView
    public ScrollRect scrollrect; 

    //список всех картинок
    Sprite[] images;
    //массив панелей
    GameObject[] lpGameObjects;
    //массив текста о блокировке уровня
    GameObject[] textlockGameObjects;
    //массив звёздочек
    GameObject[,] starGameObjects;
    //количество панелей
    int lpCount;
    //RectTransform текущего объекта
    RectTransform lpRect;
    //позиция панелей
    Vector2[] lpPos;
    //номер ближней панели
    int nearestID;
    //есть скролл или нет
    bool isScrolling = false;
    //переменная для подгон панелек под центр экрана
    Vector2 lpVector;
    //маштаб панелек
    Vector3[] lpScale;
    //ссылка на объект GlobalVars
    GlobalVars glvr;
    #endregion
    void Start()
    {
        //если нет объекта GlobalVars, то инстациируем его
        if (GameObject.Find("GlobalVars")==null)
        {
           GameObject gv = GameObject.Instantiate(glvrPrefab);
            //перемеименуем его, ибо имя будет с приставкой (Clone)
            gv.name = "GlobalVars";
        }
        //переменная для отображения/неотображения звёзд над картинкой
        bool stars;
        //получаем ссылку rectTransform
        lpRect = GetComponent<RectTransform>();
        //получаем все изображения в указанной папке
        images = Resources.LoadAll<Sprite>("Sprites/Puzzle");
        //сохраняем кол-во картинок
        lpCount = images.Length;
        
        //инициализируем переменные с размером [lpCount] для дальнейшей работы
        lpPos = new Vector2[lpCount];
        lpScale = new Vector3[lpCount];
        lpGameObjects = new GameObject[lpCount];
        textlockGameObjects = new GameObject[lpCount];
        starGameObjects = new GameObject[lpCount, 5];
        
        //получаем ссылку на GlobalVars
        glvr = GameObject.Find("GlobalVars").GetComponent<GlobalVars>();
        //Загружаем информации о уровнях из файла
        glvr.LoadInfo();

        for (int i = 0; i < lpCount; i++)
        {
            //по умолчанию звёзды не показываются
            stars = false;
            //создаём звёзды
            for (int j = 0; j < 5; j++)
                starGameObjects[i, j] = Instantiate(starPrefab, transform, false);
            //создаём панель
            lpGameObjects[i] = Instantiate(lpPrefab, transform, false);
            //создаём текст неоткрытого уровня
            textlockGameObjects[i] = Instantiate(textlockPrefab, transform, false);
            //присваиваем спрайт картинки панели
            lpGameObjects[i].GetComponent<Image>().overrideSprite = images[i];
            //первый уровень всегда доступен, поэтому его игнорим (i!=0)
            if (i != 0)
            {
                //если уровень недоступен
                if (glvr.GetLevel(images[i].name).isLock)
                {
                    //пишем что уровень не доступен
                    textlockGameObjects[i].GetComponent<Text>().text = "Complete at least three levels of the previous picture";
                    //затемняем спрайт панели
                    lpGameObjects[i].GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
                    //выключаем компонент Button
                    lpGameObjects[i].GetComponent<Button>().enabled = false;
                }
                else //если уровень доступен, то просто включаем звёзды
                    stars = true;

                //находим позицию предыдущей панели
                Transform prevTrans = lpGameObjects[i - 1].transform;
                //исходя из значений предыдущей панели, перемещаем текущую панель
                lpGameObjects[i].transform.localPosition = new Vector2(prevTrans.localPosition.x + lpOffset
                    + lpPrefab.GetComponent<RectTransform>().sizeDelta.x * lpPrefab.GetComponent<RectTransform>().localScale.x,
                    lpGameObjects[i - 1].transform.localPosition.y);
                //изменяем маштаб панели
                lpGameObjects[i].transform.localScale = new Vector3(0.4f, 0.4f, 1);
                //изменяем позицию объекта с текстом о недоступности
                textlockGameObjects[i].transform.localPosition = lpGameObjects[i].transform.localPosition;
                //запоминаем позицию панели
                lpPos[i] = -lpGameObjects[i].transform.localPosition;
            }
            //если звёзды включены или это первый уровень
            if (stars || i == 0)
                for (int j = 0; j < 5; j++)
                {
                    //вычисляем и задаём позицию звезды
                    starGameObjects[i, j].transform.localPosition = new Vector3(-lpPos[i].x + 80 * (-2 + j), -lpPos[i].y, 10);
                    //???????????????? подгоняем маштаб звёзд
                    starGameObjects[i, j].transform.localScale = new Vector3(65, 65, 1);
                    //задаём спрайт зелёный звезды, если уровень сложности пройден
                    if (j < glvr.GetLevel(images[i].name).difficult)
                        starGameObjects[i, j].GetComponent<Image>().overrideSprite = Resources.Load<Sprite>("Sprites/star");
                }
        }
    }
    void FixedUpdate()
    {
        //если пользователь не скроллит
        if (!isScrolling)
            //если позиция текущего объекта выходит за позицию первой или последней панели
            if (lpRect.anchoredPosition.x >= lpPos[0].x || lpRect.anchoredPosition.x <= lpPos[lpPos.Length - 1].x)
                //то выключаем инерцию (для того что бы по инерции не было вылета элементов далеко за границы)
                scrollrect.inertia = false;
        //переменная поиска ближайшей переменной
        float nearestPos = float.MaxValue;
        //цикл по всем панелям
        for (int i = 0; i < lpCount; i++)
        {
            //вычисляем абсолютную дистанцию панели до центра экрана 
            float distance = Mathf.Abs(lpRect.anchoredPosition.x - lpPos[i].x);
            //если позиция меньше остальныех, то запоминаем её и номер панели
            if (distance < nearestPos)
            {
                nearestPos = distance;
                nearestID = i;
            }

            //тут же вычисляем скейл панелей
            //скейл зависит от позиции элементов и плавно регулируется в диапазоне между 0.4 и 0.6
            float scale = Mathf.Clamp(1 / (distance / lpOffset) * scaleOffset, 0.4f, 0.6f);
            lpScale[i].x = Mathf.SmoothStep(lpGameObjects[i].transform.localScale.x, scale, 10 * Time.fixedDeltaTime);
            lpScale[i].y = Mathf.SmoothStep(lpGameObjects[i].transform.localScale.y, scale, 10 * Time.fixedDeltaTime);
            lpScale[i].z = 1;
            //Присваиваем скейл к панели и тексту о недоступности
            lpGameObjects[i].transform.localScale = lpScale[i];
            textlockGameObjects[i].transform.localScale = lpScale[i];
        }

        //запоминаем абсолютную скорость движения по x
        float scrollVelocity = Mathf.Abs(scrollrect.velocity.x);
        
        //опускаем поднимаем звёздочки
        for (int i = 0; i < lpCount; i++)
            //если уровень доступен, то продолжаем
            if (!glvr.GetLevel(images[i].name).isLock)
            {
                //позиция звёзд по y
                float ystar;
                //если пользователь скроллит, или скорость >2  или это не ближний объект
                if (isScrolling || scrollVelocity > 2 || nearestID != i)
                    //то звёзды будут опущегы
                    ystar = Mathf.SmoothStep(starGameObjects[i, 0].transform.localPosition.y, -lpPos[i].y, 10 * Time.fixedDeltaTime);
                else
                    //иначе звёзды поднять на высоту 290f
                    ystar = Mathf.SmoothStep(starGameObjects[i, 0].transform.localPosition.y, -lpPos[i].y + 290f, 10 * Time.fixedDeltaTime);

                //Задаём позицию звёзд
                for (int j = 0; j < 5; j++)
                    starGameObjects[i, j].transform.localPosition = new Vector3(starGameObjects[i, j].transform.localPosition.x, ystar, 10f);
            }

        //если скорость меньше 400 и пользователь не скроллит, то выключаем инерцию, что бы не было дёрганий панелей
        if (scrollVelocity < 400 && !isScrolling) scrollrect.inertia = false;
        //если пользователь скроллит или скорость больше 400, то выходим
        if (isScrolling || scrollVelocity > 400) return;
        //иначе подгоняем позицию таким образом, что бы ближайшая панель была по центу экрана
        lpVector.x = Mathf.SmoothStep(lpRect.anchoredPosition.x, lpPos[nearestID].x, scrollSpeed * Time.fixedDeltaTime);
        lpRect.anchoredPosition = lpVector;
    }

    //включение/выключение скролла
    public void Scrolling(bool flag)
    {
        isScrolling = flag;
        //если пользователь скроллит, то включаем инерцию
        if (flag) scrollrect.inertia = true;
    }
}
