using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }
        
        public Board(int value, int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;
            if (value == 0)
            {
                Cells = new Cell[width / cellSize, height / cellSize];
                for (int x = 0; x < Columns; x++)
                    for (int y = 0; y < Rows; y++)
                        Cells[x, y] = new Cell();

                ConnectNeighbors();
                Randomize(liveDensity);
            }
        
        else//read file
            {
                using (StreamReader r = new StreamReader("test1.txt"))
                {
                    List<string> matr = new List<string>();
                    string curstr;
                    int max = 0;
                    while ((curstr = r.ReadLine()) != null)
                    {
                        matr.Add(curstr);
                        if (curstr.Length > max) max = curstr.Length;
                    }
                    if (max == 0) return;
                    Cells = new Cell[max / cellSize, matr.Count/ cellSize];
                    for (int x = 0; x < Columns; x++)
                        for (int y = 0; y < Rows; y++)
                        {
                            if (matr[y].Length <= x)
                                Cells[x, y] = new Cell(0);
                            else if (matr[y][x] == '*') Cells[x, y] = new Cell(1);
                            else Cells[x, y] = new Cell(0);
                        }
                    ConnectNeighbors();
                }
            }
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
    }
    
    
    class Program
    {
        public class Item
        {
            public int width;
            public int height;
            public int cellSize;
            public double liveDensity;
        }
        static Board board;
        static private void Reset(int val)
        {
            using (StreamReader r = new StreamReader("life.json"))
            {
                Item items = new Item();
                string str = r.ReadToEnd();
                string tmp_str = "";
                int i = 0;
                while (str[i] != ':') i++;
                i++;
                while (str[i] != ',')
                {
                    tmp_str = tmp_str + str[i];
                    i++;
                }
                items.width = Int32.Parse(tmp_str);
                while (str[i] != ':') i++;
                i++;
                tmp_str = "";
                while (str[i] != ',')
                {
                    tmp_str = tmp_str + str[i];
                    i++;
                }
                items.height = Int32.Parse(tmp_str);
                while (str[i] != ':') i++;
                i++;
                tmp_str = "";
                while (str[i] != ',')
                {
                    tmp_str = tmp_str + str[i];
                    i++;
                }
                items.cellSize = Int32.Parse(tmp_str);
                while (str[i] != ':') i++;
                i++;
                tmp_str = "";
                while (str[i] != '.')
                {
                    tmp_str = tmp_str + str[i];
                    i++;
                }
                i++;
                items.liveDensity = Int32.Parse(tmp_str);
                tmp_str = "";
                while (str[i] != '\r')
                {
                    tmp_str = tmp_str + str[i];
                    i++;
                }
                int st = 1;
                for (int j = 0; j < tmp_str.Length; j++) st = st * 10;
                items.liveDensity = items.liveDensity + (float)Int32.Parse(tmp_str) / st;
                board = new Board(
                    val,
                    width: items.width,
                    height: items.height,
                    cellSize: items.cellSize,
                    liveDensity: items.liveDensity);
            }
        }
        static void Render()
        {
            using (StreamWriter r = new StreamWriter("test_1.txt"))
            {
                for (int row = 0; row < board.Rows; row++)
                {
                    for (int col = 0; col < board.Columns; col++)
                    {
                        var cell = board.Cells[col, row];
                        if (cell.IsAlive)
                        {
                            Console.Write('*');
                            r.Write('*');
                        }
                        else
                        {
                            Console.Write(' ');
                            r.Write(' ');
                        }
                    }
                    Console.Write('\n');
                    r.Write('\n');
                }
            }
                    static void Main(string[] args)
        {
            Reset();
            while(true)
            {
                Console.Clear();
                Render();
                board.Advance();
                Thread.Sleep(1000);
            }
        }
    }
}
