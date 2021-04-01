using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Data;
using ExtensionLibrary;
using System.Threading;

namespace Artificial_Life
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // <---------------------------->
        // просто поле = -1
        // светло-зеленые - живые клетки = 0 
        // черные клетки - стены = 1
        // коричневые - умершие клетки = 2 их пока не будет
        // красные - яд = 3
        // синие клетки - еда = 4
        // <---------------------------->

        private DispatcherTimer timer;
        public int size = 20; // размеры клетки 
        public List<Cell> cells; // живые клетки
        public int[,] field; // все поле игры
        Random rand = new Random();
        private int TimeOfLife = 0;
        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("Зеленые - живые клетки\nКрасные - яд, при соприкосновении с ними живая клетка становится ядом\n" +
                "Черные клетки - стены\nСиние - еда", "Пояснение", MessageBoxButton.OK);
            timer = new DispatcherTimer();
            cells = new List<Cell>();
            for (int i = 0; i < 64; i++)
                cells.Add(new Cell(0, size, this));
            timer.Interval = new TimeSpan(0, 0, 0, 0, 2000);
            timer.Tick += Timer_Tick;
            Print();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (cells.Count > 8)
                Logic();
            else
            {
                var r = new Cell[cells.Count];
                cells.CopyTo(r);
                while (cells.Count < 64)
                {
                    for (int i = 0; i < r.Length; i++)
                    {
                       
                        var cell = r[i].Clone();
                        cells.Add(cell as Cell);
                    }
                }
                while (cells.Count != 64)
                    cells.Remove(cells[cells.Count - 1]);
                for (int j = 8; j < cells.Count; j++)
                {
                    cells[j].X = rand.Next(0, Convert.ToInt32(CanvasMap.Height / size)) % field.GetLength(1);
                    cells[j].Y = rand.Next(0, Convert.ToInt32(CanvasMap.Width / size)) % field.GetLength(0);
                }
                for (int i = 8; i > 1; i--)
                {
                    cells[cells.Count - i].Mutation();
                    cells[cells.Count - i].Rectangle.Fill = Brushes.Yellow;
                }
            }
            PrintNewGeneration();
        }
        private void PrintNewGeneration()
        {
            Title = $"Искусственная жизнь {TimeOfLife++}";
            CanvasMap.Children.Clear();
            int aWidth = Convert.ToInt32(Math.Ceiling(CanvasMap.Width / size) - 1); // получаю сколько в ширину могу уместить квадратов
            int aHeight = Convert.ToInt32(Math.Ceiling(CanvasMap.Height / size)); // получаю сколько в высоту могу уместить квадратов 
            int count = 0;
            for (int i = 0; i < aHeight; i++)
            {
                for (int j = 0; j < aWidth; j++)
                {
                    if (field[j, i] == 0)
                    {
                        Canvas.SetLeft(cells[count].Rectangle, j * size);
                        Canvas.SetTop(cells[count].Rectangle, i * size);
                        CanvasMap.Children.Add(cells[count].Rectangle);
                        count++;
                        continue;
                    }
                    else
                    {
                        var rectangle = new Rectangle();
                        rectangle.Width = size - 1.0;
                        rectangle.Height = size - 1.0;
                        rectangle.Opacity = 0.5;
                        if (field[j, i] == 1)
                            rectangle.Fill = Brushes.Black;
                        else if (field[j, i] == 4)
                            rectangle.Fill = Brushes.Blue;
                        else if (field[j, i] == 3)
                            rectangle.Fill = Brushes.Red;
                        else
                            rectangle.Fill = Brushes.LightGray;
                        Canvas.SetLeft(rectangle, j * size);
                        Canvas.SetTop(rectangle, i * size);
                        CanvasMap.Children.Add(rectangle);
                    }
                }                
            }
        }
        /// <summary>
        /// Управляет логикой игры "Жизнь"
        /// </summary>
        private void Logic()
        {
            for (int i = 0; i < cells.Count; i++)
                cells[i].Logic();
        }
        /// <summary>
        /// Отрисовывает первое поколение
        /// </summary>
        private void Print()
        {
            int aWidth = Convert.ToInt32(Math.Ceiling(CanvasMap.Width / size) - 1); // получаю сколько в ширину могу уместить квадратов
            int aHeight = Convert.ToInt32(Math.Ceiling(CanvasMap.Height / size)); // получаю сколько в высоту могу уместить квадратов 
            field = new int[aWidth, aHeight];
            CanvasMap.Children.Clear();
            int count = 0;
            for (int i = 0; i < aHeight; i++)
            {
                for (int j = 0; j < aWidth; j++)
                {
                    if (rand.Next(0, 100) > 96 && count != 64)  // заселение живыми клетками = 0
                    {
                        Canvas.SetLeft(cells[count].Rectangle, j * size);
                        Canvas.SetTop(cells[count].Rectangle, i * size);
                        CanvasMap.Children.Add(cells[count].Rectangle);
                        cells[count].X = i;
                        cells[count].Y = j;
                        count++;
                        field[j, i] = 0;
                    }
                    else  
                    {
                        var rectangle = new Rectangle();
                        rectangle.Width = size - 1.0;
                        rectangle.Height = size - 1.0;
                        rectangle.Opacity = 0.5;
                        if (rand.Next(0, 100) > 95)
                        {
                            rectangle.Fill = Brushes.Blue; // расстановка еды = 4
                            field[j, i] = 4;
                        }
                        else if (rand.Next(1, 15) == 1)
                        {
                            rectangle.Fill = Brushes.Black; // расстановка стен = 1
                            field[j, i] = 1;
                        }
                        else if (rand.Next(1, 45) == 1)
                        {
                            rectangle.Fill = Brushes.Red; // расстановка яда = 3
                            field[j, i] = 3;
                        }
                        else
                        {
                            rectangle.Fill = Brushes.LightGray; // просто поле = -1
                            field[j, i] = -1;
                        }
                        Canvas.SetLeft(rectangle, j * size);
                        Canvas.SetTop(rectangle, i * size);
                        CanvasMap.Children.Add(rectangle);                        
                    }
                }                
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            Print();
        }
    }
}
