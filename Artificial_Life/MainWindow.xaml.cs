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
using System.Threading.Tasks;
using System.IO;
using System.Text;

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
        private int generation = 0;
        private int LifeTimeOfLastGen = 0; // хранит время жизни последнего поколения
        //private int startLifeCount = 0; нужен для отладки, чтобы программно уменьшать кол-во живых клеток
        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("Зеленые - живые клетки\nЖелтые - мутировавшие клетки\n" +
                "Красные - яд, при соприкосновении с ними живая клетка становится ядом\n" +
                "Черные клетки - стены\nСиние - еда", "Пояснение", MessageBoxButton.OK);
            timer = new DispatcherTimer();
            cells = new List<Cell>();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += Timer_Tick;
            Title = $"Искусственная жизнь {generation++}";
            textBlockLifeTimeOfLastGen.Text = $"Время жизни последнего поколения: {LifeTimeOfLastGen}";
            var sw = new StreamWriter("graficsForGen.csv", false, Encoding.Unicode);
            sw.WriteLine("Поколение\tВремя жизни");
            sw.Close();
            Print();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (cells.Count >= 8)
            {
                Logic();
                LifeTimeOfLastGen++;
            }
            else
            {
                if (generation == 0)
                    File.AppendAllText("genome.txt", $"Время жизни первого поколения: {LifeTimeOfLastGen}\n");
                textBlockLifeTimeOfLastGen.Text = $"Время жизни последнего поколения: {LifeTimeOfLastGen}";
                using (StreamWriter sw = new StreamWriter("graficsForGen.csv", true, Encoding.Unicode))
                {
                    sw.WriteLine($"{generation}\t{LifeTimeOfLastGen}");
                };
                LifeTimeOfLastGen = 0;
                int aWidth = Convert.ToInt32(Math.Ceiling(CanvasMap.Width / size) - 1);
                int aHeight = Convert.ToInt32(Math.Ceiling(CanvasMap.Height / size));
                var r = new Cell[cells.Count];
                cells.CopyTo(r);

                for (int i = 0; i < aHeight; i++)
                    for (int j = 0; j < aWidth; j++)
                        if (rand.NextDouble() < 0.005 && field[i, j] == -1)
                            field[i, j] = 4; // спавню еду


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
                for (int j = 0; j < cells.Count; j++)
                    cells[j].Rectangle.Fill = Brushes.LightGreen;
                for (int j = 8; j < cells.Count; j++)
                {
                    cells[j].Health = 15;
                    int x = rand.Next(0, Convert.ToInt32(CanvasMap.Height / size)) % field.GetLength(0);
                    int y = rand.Next(0, Convert.ToInt32(CanvasMap.Width / size)) % field.GetLength(1);
                    while (field[x, y] != -1)
                    {
                        x = rand.Next(0, Convert.ToInt32(CanvasMap.Height / size)) % field.GetLength(0);
                        y = rand.Next(0, Convert.ToInt32(CanvasMap.Width / size)) % field.GetLength(1);
                    }
                    cells[j].X = x;
                    cells[j].Y = y;
                }
                for (int i = 8; i > 1; i--)
                {
                    cells[cells.Count - i].Mutation();
                    cells[cells.Count - i].Rectangle.Fill = Brushes.Yellow; // мутировавшие клетки
                }
                Title = $"Искусственная жизнь {generation++}";
            }
            PrintNewGeneration();
        }
        private void PrintNewGeneration()
        {
            
            CanvasMap.Children.Clear();            
            int aWidth = Convert.ToInt32(Math.Ceiling(CanvasMap.Width / size) - 1); 
            int aHeight = Convert.ToInt32(Math.Ceiling(CanvasMap.Height / size)); 
            int count = 0;
            for (int i = 0; i < aHeight; i++)
            {
                for (int j = 0; j < aWidth; j++)
                {

                    if (field[i, j] == 0 && count != cells.Count)
                    {
                        Canvas.SetLeft(cells[count].Rectangle, j * size);
                        Canvas.SetTop(cells[count].Rectangle, i * size);
                        CanvasMap.Children.Add(cells[count].Rectangle);
                        count++;
                    }
                    else
                    {
                        var rectangle = new Rectangle();
                        rectangle.Width = size - 1.0;
                        rectangle.Height = size - 1.0;
                        rectangle.Opacity = 0.5;
                        if (field[i, j] == 1)
                            rectangle.Fill = Brushes.Black;
                        else if (field[i, j] == 4)
                            rectangle.Fill = Brushes.Blue;
                        else if (field[i, j] == 3)
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
                while (!cells[i].Logic()) { }
        }

        /// <summary>
        /// Отрисовывает первое поколение
        /// </summary>
        private void Print()
        {
            int aWidth = Convert.ToInt32(Math.Ceiling(CanvasMap.Width / size) - 1); // получаю сколько в ширину могу уместить квадратов
            int aHeight = Convert.ToInt32(Math.Ceiling(CanvasMap.Height / size)); // получаю сколько в высоту могу уместить квадратов 
            field = new int[aHeight, aWidth];
            CanvasMap.Children.Clear();
            cells.Clear();
            int count = 0;
            for (int i = 0; i < aHeight; i++)
            {
                for (int j = 0; j < aWidth; j++)
                {
                    if (rand.Next(0, 100) > 96)  // заселение живыми клетками = 0
                    {
                        cells.Add(new Cell(0, size, this));
                        Canvas.SetLeft(cells[count].Rectangle, j * size);
                        Canvas.SetTop(cells[count].Rectangle, i * size);
                        CanvasMap.Children.Add(cells[count].Rectangle);
                        cells[count].X = i;
                        cells[count].Y = j;
                        count++;
                        field[i, j] = 0;
                        if (cells.Count == 1)
                            File.AppendAllText("genome.txt", $"Поколение {generation++} геном: {cells[0].ToString()}\n"); // заношу геном
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
                            field[i, j] = 4;
                        }
                        else if (rand.Next(1, 15) == 1)
                        {
                            rectangle.Fill = Brushes.Black; // расстановка стен = 1
                            field[i, j] = 1;
                        }
                        else if (rand.Next(1, 45) == 1)
                        {
                            rectangle.Fill = Brushes.Red; // расстановка яда = 3
                            field[i, j] = 3;
                        }
                        else
                        {
                            rectangle.Fill = Brushes.LightGray; // просто поле = -1
                            field[i, j] = -1;
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
            //startLifeCount = 0;
            if (cells.Count != 0)
            {
                File.AppendAllText("genome.txt", $"Поколение {generation} геном: {cells[0].ToString()}\n"); // заношу геном
                File.AppendAllText("genome.txt", $"Время жизни последнего поколения: {LifeTimeOfLastGen}\n");
                File.AppendAllText("genome.txt", "<---------------------------------------------->\n");
            }
            generation = 0;
            LifeTimeOfLastGen = 0;
            Print();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            if (cells.Count != 0)
            {
                File.AppendAllText("genome.txt", $"Поколение {generation++} геном: {cells[0].ToString()}\n"); // заношу геном
                File.AppendAllText("genome.txt", $"Время жизни последнего поколения: {LifeTimeOfLastGen}\n");
                File.AppendAllText("genome.txt", "<---------------------------------------------->\n");
            }
        }
    }
}
