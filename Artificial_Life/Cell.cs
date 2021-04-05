using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;
using ExtensionLibrary;
using System.Threading;
using System.Windows.Markup;
using System.Text;

namespace Artificial_Life
{
    public class Cell : ICloneable
    {
        private Rectangle rectangle;
        public int X { get; set; } // координаты клетки ширина
        public int Y { get; set; } // координаты клетки высота
        public int[] genome; // геном клетки
        private int pointer; // указатель на текущее действие клетки
        private int health; // здоровье клетки
        MainWindow window;
        private Random r = new Random();
        int counterForMove = 0;
        static int MaxHealth { get; set; } // максимальное здоровье клетки
        static Cell()
        {
            MaxHealth = 30;
        }
        /// <summary>
        /// Создает клетку, стену или хищника
        /// </summary>
        /// <param name="i"> Идентификатор окраски (живые клетки = 0, стены = 1, хищные клетки = 2)</param>
        /// <param name="size"> Размеры клетки</param>
        public Cell(int i, int size, MainWindow window)
        {
            this.window = window;
            health = 15;
            pointer = 0;            
            genome = new int[64];
            for (int k = 0; k < 64; k++)
                genome[k] = r.Next(0, 64);
            rectangle = new Rectangle();
            rectangle.Width = size - 1.0;
            rectangle.Height = size - 1.0;
            rectangle.Opacity = 0.7;
            rectangle.Fill = Brushes.LightGreen;
        }

        #region Свойства
        /// <summary>
        /// Возвращает квадрат
        /// </summary>
        public Rectangle Rectangle
        {
            get => rectangle;
        }
        /// <summary>
        /// Возвращает текущее здоровье клетки
        /// </summary>
        public int Health { get => health; set => health = value; }
        #endregion

        #region Методы
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (var e in genome)
                str.Append( e.ToString() + " ");
            return $"{str}\n";
        }
        /// <summary>
        /// Определяет поведение клетки
        /// </summary>
        public bool Logic()
        {
            counterForMove++;
            pointer %= 64;
            bool result = false;
            if (counterForMove < 8)
                if (genome[pointer] <= 7) // сделать шаг
                {
                    if (Move(genome[pointer]))
                    {
                        health--;
                        result = true;
                    }
                }
                else if (genome[pointer] <= 15) // схватить
                {
                    if (Take(genome[pointer] % 8))
                    {
                        health--;
                        result = true;
                    }
                }
                else if (genome[pointer] <= 23) // посмотреть
                {
                    See(genome[pointer] % 8);
                }
                else // безусловный переход
                {
                    pointer = (pointer + genome[pointer]) % 64;
                }
            else
            {
                health--;
                result = true;                
                counterForMove = 0;
            }
            if (health <= 0)
            {
                window.field[this.X, this.Y] = -1;
                window.cells.Remove(this);
            }
            return result;
        }
        /// <summary>
        /// Команда просмотра след поля
        /// </summary>
        /// <param name="v"></param>
        private void See(int v)
        {
            //  |0|   1  |2|
            //  |7|клетка|3|
            //  |6|   5  |4| 
            switch (v)
            {
                case 0:
                    {
                        CheckForSee(-1, -1);
                        break;
                    }
                case 1:
                    {
                        CheckForSee(-1, 0);
                        break;
                    }
                case 2:
                    {
                        CheckForSee(-1, 1);
                        break;
                    }
                case 3:
                    {
                        CheckForSee(0, 1);
                        break;
                    }
                case 4:
                    {
                        CheckForSee(1, 1);
                        break;
                    }
                case 5:
                    {
                        CheckForSee(1, 0);
                        break;
                    }
                case 6:
                    {
                        CheckForSee(1, -1);
                        break;
                    }
                case 7:
                    {
                        CheckForSee(0, -1);
                        break;
                    }
            }
        }
        /// <summary>
        /// Функция, сдвигающая указатель при просмотре след поля
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void CheckForSee(int x, int y)
        {
            int Xcoord = (X + x + window.field.GetLength(0)) % window.field.GetLength(0);
            int Ycoord = (Y + y + window.field.GetLength(1)) % window.field.GetLength(1);
            if (window.field[Xcoord, Ycoord] == -1) // пустое поле
                pointer += 5;
            else if (window.field[Xcoord, Ycoord] == 0) // бот
                pointer += 3;
            else if (window.field[Xcoord, Ycoord] == 1) // стена
                pointer += 2;
            else if (window.field[Xcoord, Ycoord] == 3) // яд
                pointer++;
            else if (window.field[Xcoord, Ycoord] == 4) // еда
                pointer += 4;
        }
        private bool Take(int v)
        {
            //  |0|   1  |2|
            //  |7|клетка|3|
            //  |6|   5  |4| 
            bool result = false;
            switch (v)
            {
                case 0:
                    {
                        result = CheckForTake(-1, -1);
                        break;
                    }
                case 1:
                    {
                        result = CheckForTake(-1, 0);
                        break;
                    }
                case 2:
                    {
                        result = CheckForTake(-1, 1);
                        break;
                    }
                case 3:
                    {
                        result = CheckForTake(0, 1);
                        break;
                    }
                case 4:
                    {
                        result = CheckForTake(1, 1);
                        break;
                    }
                case 5:
                    {
                        result = CheckForTake(1, 0);
                        break;
                    }
                case 6:
                    {
                        result = CheckForTake(1, -1);
                        break;
                    }
                case 7:
                    {
                        result = CheckForTake(0, -1);
                        break;
                    }
            }
            return result;
        }
        /// <summary>
        /// Функция, сдвигающая указатель при взятии
        /// Возвращает false, если ход не завершен, иначе true
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CheckForTake(int x, int y)
        {
            int Xcoord = (X + x + window.field.GetLength(0)) % window.field.GetLength(0);
            int Ycoord = (Y + y + window.field.GetLength(1)) % window.field.GetLength(1);
            if (window.field[Xcoord, Ycoord] == -1) // пустое поле
                pointer += 5;
            else if (window.field[Xcoord, Ycoord] == 0) // бот
                pointer += 3;
            else if (window.field[Xcoord, Ycoord] == 1) // стена
                pointer += 2;
            else if (window.field[Xcoord, Ycoord] == 3) // яд
            {
                pointer++;
                window.field[Xcoord, Ycoord] = 4; // бот преобразует яд в еду
                return true;
            }
            else if (window.field[Xcoord, Ycoord] == 4) // еда
            {
                if (health + 10 <= MaxHealth)
                {
                    health += 10;
                    window.field[Xcoord, Ycoord] = -1;
                }
                pointer += 4;
                return true;
            }
            return false;
        }
        private bool Move(int v)
        {
            //  |0|   1  |2|
            //  |7|клетка|3|
            //  |6|   5  |4| 
            bool result = false;
            switch (v)
            {
                case 0:
                    {
                        result = CheckForMove(-1, -1);
                        break;
                    }
                case 1:
                    {
                        result = CheckForMove(-1, 0);
                        break;
                    }
                case 2:
                    {
                        result = CheckForMove(-1, 1);
                        break;
                    }
                case 3:
                    {
                        result = CheckForMove(0, 1);
                        break;
                    }
                case 4:
                    {
                        result = CheckForMove(1, 1);
                        break;
                    }
                case 5:
                    {
                        result = CheckForMove(1, 0);
                        break;
                    }
                case 6:
                    {
                        result = CheckForMove(1, -1);
                        break;
                    }
                case 7:
                    {
                        result = CheckForMove(0, -1);
                        break;
                    }
            }
            return result;

        }
        /// <summary>
        /// Функция, сдвигающая указатель при передвижении на след поле
        /// Возвращает false, если ход не завершен, иначе true
        /// </summary>
        /// <returns></returns>
        private bool CheckForMove(int x, int y)
        {
            int Xcoord = (X + x + window.field.GetLength(0)) % window.field.GetLength(0); // отвечает за расположение по вертикали
            int Ycoord = (Y + y + window.field.GetLength(1)) % window.field.GetLength(1); // отвечает за расположение по горизонтали
            if (window.field[Xcoord, Ycoord] == -1) // пустое поле
            {
                pointer += 5;
                window.field[X, Y] = -1;
                X = Xcoord;
                Y = Ycoord;
                window.field[X, Y] = 0;
                return true;
            }
            else if (window.field[Xcoord, Ycoord] == 0) // бот
                pointer += 3;
            else if (window.field[Xcoord, Ycoord] == 1) // стена
                pointer += 2;
            else if (window.field[Xcoord, Ycoord] == 3) // яд
            {
                pointer++;
                window.field[X, Y] = 3; // бот становится ядом
                health = 0;
                return true;
            }
            else if (window.field[Xcoord, Ycoord] == 4) // еда
            {
                window.field[X, Y] = -1;
                X = Xcoord;
                Y = Ycoord;
                window.field[X, Y] = 0;
                pointer += 4;
                if (health + 10 <= MaxHealth)
                    health += 10;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Меняет один случайный ген
        /// </summary>
        public void Mutation()
        {
            var r = new Random();
            int i = r.Next(0, 64);
            genome[i] = r.Next(0, 64);
        }
        public object Clone()
        {
            var e = new Cell(0, window.size, window);
            e.rectangle = XamlReader.Parse(XamlWriter.Save(this.rectangle)) as Rectangle;
            e.X = this.X;
            e.Y = this.Y;
            e.pointer = this.pointer;
            e.genome = (int[])this.genome.Clone();
            e.health = this.health;
            e.counterForMove = this.counterForMove;
            return e;
        }
        #endregion
    }
}
