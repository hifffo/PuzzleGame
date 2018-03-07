using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Puzzle : MonoBehaviour
{
    #region Variables

    //Текст на экранах паузы и победы
    [Header("Text On Pause && Win")]
    public Text TextWinBestVar;
    public Text TextWinYourVar;
    public Text TextPauseBestVar;
    public Text TextNewRecord;

    //Всё что связано с временем
    [Header("Timer Object")]
    public Timer timerObject;     //ссылка на компонент "Timer (Script)"
    bool isgamestarted = false; //отвечает начата ли игра (true после перемешивания ячеек)
    bool finish; //проверка завершения игры (false после сложения пазла)
    bool showfinish = false; //проверка, показан ли экран победы
    float timer; //таймер для задержки перед перемешиванием

    //Всё что относится к пазлам
    [Header("Cell Prefab")]
    public GameObject cellPrefab;
    Sprite image;
    Vector2Int cellscount = new Vector2Int(3, 3); //количество ячеек 
    GameObject[,] cells = new GameObject[5, 5]; //ссылки на объекты ячеек
    Vector2Int[,] cellspos = new Vector2Int[5, 5]; //позиция ячеек 
    Vector2 imagesize; //размер картинки в целом
    Vector2 cellsize; //размер одной ячейки
    Vector2 posstart; //позиция начальной ячейки
    Cell[,] classcell; //ссылка на компонент "Cell (Script)" каждой ячейки
    GameObject firstclick = null; //первая нажатая ячейка
    GameObject secondclick = null; //вторая нажатая ячейка

    //Прочее
    [Header("Cell Prefab")]
    public GameObject CanvasWin; //Панель победы
    GlobalVars gv; //Ссылка на GlobalVars 

    #endregion

    void Start()
    {
        //используемые переменные
        int i, j;
        //спрайт отдельной ячейки
        Sprite newspr = new Sprite();
        //позиция отдельной ячейки
        Vector2 pos = new Vector2();

        //присвоение ссылки на компонент GlobalVars
        gv = GameObject.Find("GlobalVars").GetComponent<GlobalVars>();
        //получение спрайта текущего уровня
        image = gv.GetSprite();
        //получение количества ячеек
        cellscount = gv.GetCellsCount();
        //сохранение размера картинки
        imagesize.x = image.rect.width;
        imagesize.y = image.rect.height;
        //вычисление и сохранение размера ячейки
        cellsize.x = imagesize.x / cellscount.x;
        cellsize.y = imagesize.y / cellscount.y;
        //выделение памяти под нужное кол-во ссылок
        classcell = new Cell[cellscount.x, cellscount.y];

        //задаём начальные позиции ячеек
        Shuffle("sort");
        //определение позиции левой нижней ячейки 
        posstart = new Vector2(0 - imagesize.x / image.pixelsPerUnit / 2 + cellsize.x / image.pixelsPerUnit / 2, 0 - imagesize.y / image.pixelsPerUnit / 2 + cellsize.y / image.pixelsPerUnit / 2);

        //создание ячеек 
        for (i = 0; i < cellscount.x; i++)
            for (j = 0; j < cellscount.y; j++)
            {
                //вычисление позиции ячейки и её создание 
                pos = new Vector2(posstart.x + (cellsize.x / image.pixelsPerUnit) * cellspos[i, j].x, posstart.y + (cellsize.y / image.pixelsPerUnit) * cellspos[i, j].y);
                cells[i, j] = Instantiate(cellPrefab, new Vector3(pos.x, pos.y, 10), Quaternion.identity);
                //вырезаем спрайт из целого изображения и присваиваем спрайт его ячейке  
                newspr = Sprite.Create(image.texture, new Rect(i * cellsize.x, j * cellsize.y, cellsize.x, cellsize.y), new Vector2(0.5f, 0.5f));
                cells[i, j].GetComponent<SpriteRenderer>().sprite = newspr;
                //сохраняем первоначальную позицию ячейки
                cells[i, j].GetComponent<Cell>().SetPos(new Vector2Int(i, j));
                //сохраняем ссылку на этот скрипт в ячейке
                cells[i, j].GetComponent<Cell>().SetGOHelp(gameObject);
                //сохраняем ссылку на скрипт Cell ячейки
                classcell[i, j] = cells[i, j].GetComponent<Cell>();
            }

        //задаём значение для текстовых полей
        if (gv.GetBestTime() == 0)
        {
            TextPauseBestVar.text = "--:--";
            TextWinBestVar.text = "--:--";
        }
        else
        {
            TextPauseBestVar.text = Timer.GetGoodTime(gv.GetBestTime());
            TextWinBestVar.text = Timer.GetGoodTime(gv.GetBestTime());
        }
    }

    void FixedUpdate()
    {
        //true если задержка перед началом игры не прошла(игра не начата)
        if (!isgamestarted)
        {
            //задержка 1 секунда, если таймер меньше, то прибавляем время шага к таймеру
            if (timer < 1)
                timer += Time.deltaTime;
            //если задержка уже больше 1 секунды
            if (timer > 1)
            {
                //перемешиваем ячейки
                Shuffle("unsort");
                //включаем счётчик игрового времени
                timerObject.IsOn(true);
                //отмечаем что игра начата
                isgamestarted = true;
            }
        }
        else //если игра начата
        {
            //пусть изначально все ячейки на своих местах
            bool isAllAtPos = true;

            for (int i = 0; i < cellscount.x; i++)
                for (int j = 0; j < cellscount.y; j++)
                    if (cells[cellspos[i, j].x, cellspos[i, j].y] != cells[i, j]) //если найдена ячейка не на своём месте
                        isAllAtPos = false; //то отмечаем что не все на своих местах

            //если все на своих местах и ещё не было финиша
            if (isAllAtPos && !finish)
            {
                //останавливаем игровой счётчик
                timerObject.IsOn(false);
                //задаём значение таймера = 0
                timer = 0;
                //отмечаем финиш
                finish = true;
            }
        }

        //если
        if (finish && !showfinish)
        {
            //задержка две секунды, если таймер меньше, то прибавляем время шага к таймеру
            if (timer < 2)
                timer += Time.deltaTime;
            //если задержка уже больше двух секунд
            if (timer > 2)
            {
                //отправляем в globalvars время прохождения и ссылку на текст "NewRecord"
                gv.LevelComplete(timerObject.GetTime(), TextNewRecord);
                //активируем панель победы
                CanvasWin.SetActive(true);
                //заносим время игры в текст
                TextWinYourVar.text = Timer.GetGoodTime(timerObject.GetTime());
            }
        }
    }

    //перемешивание позиций ячеек
    void Shuffle(string str) //на вход "sort" или "unsort"
    {
        //используемые переменные
        int i, j, ish, jsh;
        Vector2Int tmppos;
        bool flag;

        //сортировка
        if (str == "sort")
            for (i = 0; i < cellscount.x; i++)
                for (j = 0; j < cellscount.y; j++)
                    cellspos[i, j] = new Vector2Int(i, j);

        //перемешивание
        if (str == "unsort")
            do
            {
                //проверка, были ли перестановки 
                flag = true;
                
                for (i = 0; i < cellscount.x; i++)
                    for (j = 0; j < cellscount.y; j++)
                        //если ячейка находится на начальной позиции
                        if (cells[cellspos[i, j].x, cellspos[i, j].y] == cells[i, j])
                        {
                            //отмечаем что перестановки были
                            flag = false;
                            //выбираем рандомную позицию
                            ish = Random.Range(0, cellscount.x);
                            jsh = Random.Range(0, cellscount.y);
                            //делаем перестановку
                            tmppos = cellspos[ish, jsh];
                            cellspos[ish, jsh] = cellspos[i, j];
                            cellspos[i, j] = tmppos;
                        }
            } while (!flag); //делать пока есть перестановки (есть элементы на своей начальной позиции)
    }

    //нажатие на ячейку
    public void CellClick(GameObject clickedcell) //на вход ячейка, на которую был выполнен клик
    {
        //елси это была первая нажатая ячейка
        if (firstclick == null)
            //то запоминаем её
            firstclick = clickedcell;
        else //иначе
        {
            //записываем вторую ячейку
            secondclick = clickedcell;
            //выполняем обмен позиций первой нажатой и второй нажатой ячеек
            Vector2Int tmp = cellspos[firstclick.GetComponent<Cell>().GetPos().x, firstclick.GetComponent<Cell>().GetPos().y];
            cellspos[firstclick.GetComponent<Cell>().GetPos().x, firstclick.GetComponent<Cell>().GetPos().y] = cellspos[secondclick.GetComponent<Cell>().GetPos().x, secondclick.GetComponent<Cell>().GetPos().y];
            cellspos[secondclick.GetComponent<Cell>().GetPos().x, secondclick.GetComponent<Cell>().GetPos().y] = tmp;
            //стираем сохраннённые ячейки
            firstclick = null;
            secondclick = null;
        }
    }

    #region Returns
    //возврат первой нажатой ячейки
    public GameObject GetFirstClicked()
    {
        return firstclick;
    }

    //возврат позиции левой нижней ячейки(стартовой)
    public Vector2 GetStartPos()
    {
        return posstart;
    }

    //возврат размера ячейки
    public Vector2 GetCellSize()
    {
        return new Vector2(cellsize.x / image.pixelsPerUnit, cellsize.y / image.pixelsPerUnit);
    }

    //возврат позиции ячейки
    public Vector2Int GetPos(Vector2Int pos)
    {
        return cellspos[pos.x, pos.y];
    }
    #endregion

    #region Buttons
    //нажатие на кнопку Exit
    public void OnExitClick()
    {
        Time.timeScale = 1; //возобновление времени
        GameObject.Find("GlobalVars").GetComponent<GlobalVars>().BackToDifficult();
    }

    //нажатие на кнопку Restart
    public void OnRestartClick()
    {
        Time.timeScale = 1; //возобновление времени
        GameObject.Find("GlobalVars").GetComponent<GlobalVars>().RestartClick();
    }
    
    //нажатие на кнопку Next
    public void OnNextClick()
    {
        Time.timeScale = 1; //возобновление времени
        GameObject.Find("GlobalVars").GetComponent<GlobalVars>().NextClick();
    }
    #endregion
}
