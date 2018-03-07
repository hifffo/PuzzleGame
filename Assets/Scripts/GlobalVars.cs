using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;


//класс определяющий сущность "уровень"
public class SomeLevel
{
    //спрайт пазла
    public Sprite sprite;
    //заблокирован ли уровень
    public bool isLock;
    //доступная сложность (0-4)
    public int difficult;
    //массив лучшего времени
    public int[] besttime;
    public SomeLevel()
    {
        besttime = new int[5];
    }
}

public class GlobalVars : MonoBehaviour
{
    #region Variables
    //выбранная картинка
    Sprite selectedSprite;
    //выбранная сложность
    int selectedDifficult;
    //коллекция уровней
    List<SomeLevel> levels;
    //имя файла сохранения
    string fileName = "SaveData";
    //шифровать сохраняющиеся данные или нет
    [Header("Crypt Saves?")]
    public bool isCrypt =true;
    #endregion

    private void Start()
    {
        //делаем постоянным
        DontDestroyOnLoad(this);
    }

    #region SaveLoadCrypt
    //Сохранение в файл
    void SaveFile(List<SomeLevel> levelsToSave) //на вход список уровней для сохранения
    {
        //ссылка на StreamWriter
        StreamWriter sw = new StreamWriter(Application.dataPath + "/" + fileName + ".pzl");
        //переменная для проверки прохождения трёх уровней
        bool get3 = true;

        foreach (SomeLevel level in levelsToSave)
        {
            //сохранение информации
            sw.WriteLine(Crypt(level.sprite.name + " " + "notnull"));
            sw.WriteLine(Crypt(level.sprite.name + "isLock" + " " + !get3)); //!get3 - если в предыдущей картинке пройдено 3 уровня сложности 
            sw.WriteLine(Crypt(level.sprite.name + "difficult" + " " + level.difficult));

            //вычисляем, пройдено ли 3 уровня сложности
            if (level.difficult > 2) get3 = true; else get3 = false;

            //сохраняем лучшее время
            for (int j = 0; j < 5; j++)
                sw.WriteLine(Crypt(level.sprite.name + "besttime" + j + " " + level.besttime[j]));
        }
        sw.Close();

    }
    //Загрузка информации из файла
    public void LoadInfo()
    {
        //если файла нет, то создаём новый пустой
        if (!File.Exists(Application.dataPath + "/" + fileName + ".pzl"))
            SaveFile(new List<SomeLevel>());
        //считываем все строки
        string[] rows = File.ReadAllLines(Application.dataPath + "/" + fileName + ".pzl");
        //очищаем levels
        levels = new List<SomeLevel>();
        //используемые переменные
        int _i; bool _b;
        //переменная для добавления уровня в список
        SomeLevel tmpSomeLevel = new SomeLevel();
        //переменная для сохранения предыдущего уровня
        SomeLevel prevSomeLevel = null;
        //получаем список всех картинок в указанной папке
        Sprite[] images = Resources.LoadAll<Sprite>("Sprites/Puzzle");

        foreach (Sprite tmpSprite in images)
        {
            tmpSomeLevel = new SomeLevel();
            //проверяем, есть ли запись по данной картинке
            if (GetValue(rows, tmpSprite.name) != string.Empty)
            {
                //присваиваем спрайт
                tmpSomeLevel.sprite = tmpSprite;
                //получаем остальную информацию из файла
                if (bool.TryParse(GetValue(rows, tmpSprite.name + "isLock"), out _b)) tmpSomeLevel.isLock = _b;
                if (int.TryParse(GetValue(rows, tmpSprite.name + "difficult"), out _i)) tmpSomeLevel.difficult = _i;

                for (int j = 0; j < 5; j++)
                {
                    if (int.TryParse(GetValue(rows, tmpSprite.name + "besttime" + j), out _i)) tmpSomeLevel.besttime[j] = _i;
                }
                //добавляем прочитанный уровень в общий список
                levels.Add(tmpSomeLevel);

            }
            else //если нет информации по данной картинке, то задаём стандартные значения
            {
                tmpSomeLevel.sprite = tmpSprite;
                tmpSomeLevel.isLock = true;
                tmpSomeLevel.difficult = 0;
                for (int j = 0; j < 5; j++)
                    tmpSomeLevel.besttime[j] = 0;
                if (prevSomeLevel == null)
                    tmpSomeLevel.isLock = false;
                else
                if (prevSomeLevel.difficult > 2)
                    tmpSomeLevel.isLock = false;

                //добавляем уровень в общий список
                levels.Add(tmpSomeLevel);
            }
            //запоминаем предыдущий уровень
            prevSomeLevel = tmpSomeLevel;
        }
        //сохраняем уровни(требуется для того, что бы сохранилась информации о картинках, которых не было)
        SaveFile(levels);
    }

    //функция поиска строки по заданному ключу
    string GetValue(string[] line, string pattern)
    {
        //переменная результата
        string result = "";
        //цикл по всем строкам в массиве
        foreach (string key in line)
        {
            //если строка не пуста
            if (key.Trim() != string.Empty)
            {
                string value = "";
                //проверяем, включено ли шифрование и запоминаем строку в отдельную переменную
                if (isCrypt) value = Crypt(key); else value = key;
                //если ключ равен первому слову в строке
                if (pattern == value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0])
                {
                    //то удаляем ключ(на выходе получается остальная часть строки = наше значение)
                    result = value.Remove(0, value.IndexOf(' ') + 1);
                }
            }
        }
        //возвращаем результат
        return result;
    }

    //функция шифрования строки
    string Crypt(string text)
    {
        //если шифрование выключено, то просто возвращаем строку
        if (!isCrypt) return text;
        //создаём пустую строку в которую будем добавлять по одному зашифрованному символу
        string result = string.Empty;
        foreach (char j in text)
        {
            // ((int) j ^ 49) - применение XOR к номеру символа
            // (char)((int) j ^ 49) - получаем символ из измененного номера
            result += (char)((int)j ^ 49);
        }
        //возращаем результат
        return result;
    }
    #endregion

    #region Returns
    //возврат уровня по спрайту
    public SomeLevel GetLevel(string spr)
    {
        //ищем уровень, где level.sprite.name совпадает с первым аргументом функции
        foreach (SomeLevel level in levels)
            if (level.sprite.name.Equals(spr))
                return level;
        return null;
    }
    //возврат лучшего времени по выбранной картинке и выбранной сложности
    public int GetBestTime()
    {
        //цикл по всем значениям
        foreach (SomeLevel level in levels)
        {
            //проверяем условие и при истине возвращаем значение
            if (level.sprite.name.Equals(selectedSprite.name))
                return level.besttime[selectedDifficult];
        }
        return 0;
    }
    //возврат выбранного спрайта
    public Sprite GetSprite()
    {
        return selectedSprite;
    }
    //возврат количества ячеек
    public Vector2Int GetCellsCount()
    {
        //исходя из выбранной сложности выбираем кол-во ячеек
        switch (selectedDifficult)
        {
            case 0: return new Vector2Int(3, 3);
            case 1: return new Vector2Int(3, 4);
            case 2: return new Vector2Int(4, 4);
            case 3: return new Vector2Int(4, 5);
            case 4: return new Vector2Int(5, 5);
            default: return new Vector2Int(3, 3);
        }
    }
    #endregion

    #region OtherFunc
    //при завершении уровня
    public void LevelComplete(float newtime, Text TextNewRecord)
    {
        foreach (SomeLevel level in levels)
            //ищем пройденный уровень
            if (level.sprite.name.Equals(selectedSprite.name))
            {
                //если рекорд или нет рекорда
                if (level.besttime[selectedDifficult] > newtime || level.besttime[selectedDifficult] == 0)
                {
                    //сохраняем рекорд
                    level.besttime[selectedDifficult] = System.Convert.ToInt32(newtime);
                    //выводим инфу что новый рекорд
                    TextNewRecord.text = "NEW RECORD!";
                }
                //если не последний уровень сложности в картинке
                //и пройденный уровень сложности равен последнему доступному уровню
                if (selectedDifficult!=5 && level.difficult == selectedDifficult) 
                {
                    level.difficult++;
                }
            }
        SaveFile(levels);
        LoadInfo();
    }
    //выбор спрайта
    public void SelectSprite(Sprite sprite)
    {
        //сохраняем выбранный спрайт и загружаем сцену выюора сложности
        selectedSprite = sprite;
        SceneManager.LoadScene("difficult");
    }
    //выбор сложности
    public void SelectDifficult(int dif)
    {
        //сохраняем выбранную сложность и загружаем сцену игры
        selectedDifficult = dif;
        SceneManager.LoadScene("main");
    }
    //нажатие на кнопку назад к уровням(кнопка сцены difficult)
    public void BackToLevels()
    {
        SceneManager.LoadScene("levels");
    }
    //назад к выбору сложности
    public void BackToDifficult()
    {
        SceneManager.LoadScene("difficult");
    }
    //нажатие на кнопку рестарта(кнопка сцены main)
    public void RestartClick()
    {
        SceneManager.LoadScene("main");
    }
    //нажатие на кнопку "Следующий уровень"
    public void NextClick()
    {
        //переменная для помощи поиска текущего уровня
        bool nextf = false;
        //если уровень сложности картинки не последний, то просто загружаем след. уровень сложности
        if (selectedDifficult != 4)
        {
            selectedDifficult++;
            SceneManager.LoadScene("main");
            return;
        }
        //если уровень сложности последний для этой картинки
        else
        {
            //выбираем первый уровень сложности
            selectedDifficult = 0;
            //переключаем картинку
            foreach (SomeLevel level in levels)
            {
                //если этото уровень следует за предыдущим уровнем, то загружаем уго
                if (nextf)
                {
                    selectedSprite = level.sprite;
                    SceneManager.LoadScene("main");
                    return;
                }
                //тут мы определяем текущий уровень по имени картинки
                if (level.sprite.name.Equals(selectedSprite.name))
                    nextf = true;
            }
        }
        //если это был последний уровень сложности последней картинки, то выходи в самую первую сцену
        SceneManager.LoadScene("levels");
    }
    #endregion
}
