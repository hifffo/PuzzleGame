using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Cell : MonoBehaviour
{

    #region Variables
    //позиция ячейки (1..5,1..5)
    Vector2Int pos;
    //ссылка на goHelper
    GameObject goHelper;
    //ссылка на LineRenderer
    LineRenderer boxlight;
    //ссылка на таймер
    Timer timer;
    //ссылка на скрипт Puzzle
    Puzzle ps;
    //ссылка на компонент BoxCollider
    BoxCollider2D boxcol;
    //ссылка на spriteRenderer
    SpriteRenderer sprrend;
    //положение ячейки на экране
    Vector3 moveto;
    #endregion

    private void Start()
    {
        //получаем требуемые ссылки для дальнейшей работы
        ps = goHelper.GetComponent<Puzzle>();
        boxcol = GetComponent<BoxCollider2D>();
        sprrend = GetComponent<SpriteRenderer>();
        boxlight = transform.GetComponent<LineRenderer>();
    }
    void FixedUpdate()
    {
        //вычисляем позицию ячейки
        moveto = new Vector3(ps.GetStartPos().x + ps.GetCellSize().x * ps.GetPos(pos).x, ps.GetStartPos().y + ps.GetCellSize().y * ps.GetPos(pos).y, 10);
        //заставляем двигаться ячейку к её позиции
        transform.position = Vector3.MoveTowards(transform.position, moveto, Time.deltaTime * 20f);
        //если ячейка заняла свою позицию
        if (pos == ps.GetPos(pos))
            //то присваиваем ей маштаб 1
            transform.localScale = new Vector3(1f, 1f, 1);
        else
            //иначе изменяем маштаб на 0.9
            transform.localScale = new Vector3(0.9f, 0.9f, 1);
        //изменяем размер boxcollider'а
        boxcol.size = sprrend.size;
        
        //подсвечиваем объект, если он является первыйм нажатым
        if (ps.GetFirstClicked() == gameObject)
        {
            boxlight.positionCount = 5;
            boxlight.SetPosition(0, new Vector3(moveto.x + (ps.GetCellSize().x / 2.23f * -1), moveto.y + ps.GetCellSize().y / 2.23f, 9));
            boxlight.SetPosition(1, new Vector3(moveto.x + ps.GetCellSize().x / 2.23f, moveto.y + ps.GetCellSize().y / 2.23f, 9));
            boxlight.SetPosition(2, new Vector3(moveto.x + ps.GetCellSize().x / 2.23f, moveto.y + (ps.GetCellSize().y / 2.23f * -1), 9));
            boxlight.SetPosition(3, new Vector3(moveto.x + (ps.GetCellSize().x / 2.23f * -1), moveto.y + (ps.GetCellSize().y / 2.23f * -1), 9));
            boxlight.SetPosition(4, new Vector3(moveto.x + (ps.GetCellSize().x / 2.23f * -1), moveto.y + ps.GetCellSize().y / 2.23f, 9));

        }
        else
            //иначе убираем всю подсветку
            boxlight.positionCount = 0;
    }

    #region Other
    //задаём позицию ячейке
    public void SetPos(Vector2Int vector)
    {
        pos = vector;
    }
    
    //получаем позцию ячейки
    public Vector2Int GetPos()
    {
        return pos;
    }

    //задаём ссылки на хелпер и таймер для ячейки
    public void SetGOHelp(GameObject tmp)
    {
        goHelper = tmp;
        timer = goHelper.GetComponent<Timer>();
    }

    //при нажатии на ячейку
    public void OnMouseDown()
    {
        //если она стоит не на своём месте и ничего не препятствует нажатию
        if (pos != goHelper.GetComponent<Puzzle>().GetPos(pos) && timer.CanClick())
            //то отправляем в хелпер нажатие на ячейку
            goHelper.GetComponent<Puzzle>().CellClick(gameObject);
    }
    #endregion
}
