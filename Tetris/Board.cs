using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Tetris
{
    public struct Board
    {
        public Tetrominoe? hold = null;
        public int TotalLinesCleared { get; private set; } = 0;
        public int Single { get; private set; } = 0;
        public int Double { get; private set; } = 0;
        public int Triple { get; private set; } = 0;
        public int Tetris { get; private set; } = 0;
        public int AllClear { get; private set; } = 0;

        private bool[,] matrix = new bool[22, 10];
        public double Objective { get; private set; } = 0.0;

        public Board() { }

        public Board(Board other)
        {
            for (int i = 0; i < 22; i++)
                for (int j = 0; j < 10; j++)
                    matrix[i, j] = other.matrix[i, j];

            hold = other.hold;

            TotalLinesCleared = other.TotalLinesCleared;

            Single = other.Single;
            Double = other.Double;
            Triple = other.Triple;
            Tetris = other.Tetris;

            AllClear = other.AllClear;
        }

        private int getAvaliableLine(Tetrominoe tetrominoe, int startingColumn)
        {
            for (int line = tetrominoe.Height - 1; line < 22; line++)
            {
                for (int i = 0; i < tetrominoe.Height; i++)
                {
                    for (int j = 0; j < tetrominoe.Width; j++)
                    {
                        if (!tetrominoe[tetrominoe.Height - i - 1, j])
                            continue;

                        if (!matrix[line - i, startingColumn + j])
                            continue;

                        return line - 1;
                    }
                }
            }

            return 21;
        }

        public double placeTetrominoe(Tetrominoe tetrominoe, int startingColumn)
        {
            //for (int i = 14; i < 21; i++)
                //for (int j = 0; j < 10; j++)
                    //matrix[i, j] = true;

            int line = getAvaliableLine(tetrominoe, startingColumn);

            if (line < tetrominoe.Height)
            {
                return double.MinValue;
            }

            for (int i = tetrominoe.Height - 1; i >= 0; i--)
            {
                for (int j = 0; j < tetrominoe.Width; j++)
                {
                    matrix[line - i, startingColumn + j] |= tetrominoe[tetrominoe.Height - i - 1, j];
                }
            }

            calculateStateObjective();

            return Objective;
        }

        private void clearLines()
        {
            int linesCleared = 0;
            List<int> validLines = new List<int>();

            for (int i = 21; i >= 0; i--)
            {
                bool lineClear = true;

                for (int j = 0; j < 10; j++)
                {
                    if (!matrix[i, j])
                    {
                        lineClear = false;
                        break;
                    }
                }

                if (lineClear)
                {
                    linesCleared++;
                }
                else
                {
                    validLines.Add(i);
                }
            }

            for (int i = 0; i <= linesCleared; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    matrix[i, j] = false;
                }
            }

            int currentLine = 21;

            foreach(var line in validLines)
            { 
                for (int j = 0; j < 10; j++)
                {
                    matrix[currentLine, j] = matrix[line, j];
                }

                currentLine--;
            }

            TotalLinesCleared += linesCleared;

            Tetris += linesCleared == 4 ? 1 : 0;
            Triple += linesCleared == 3 ? 1 : 0;
            Double += linesCleared == 2 ? 1 : 0;
            Single += linesCleared == 2 ? 1 : 0;

            if(linesCleared > 0)
            {
                for (int j = 0; j < 10; j++)
                    if (!matrix[21, j])
                        return;

                AllClear += 1;
            }
        }

        private double calculateStateObjective()
        {
            Objective = 0.0;

            clearLines();

            Objective += Single * -100;

            Objective += Double * -80;

            Objective += Triple * -60;

            Objective += Tetris * 1000;

            Objective += AllClear * 20000;

            Objective += getBoardHeight() * -50;

            Objective += countBlocked() * -2000;

            Objective += countPeaks() * -2000;

            Objective += nonContingeneous() * -2;

            Objective += firstColumn() * -500;

            if (hold.HasValue && 
                (hold.Value.Shape == Tetrominoe.ShapeType.Line || hold.Value.Shape == Tetrominoe.ShapeType.T))
                Objective += 10;

            return Objective;
        }

        private int firstColumn()
        {
            int count = 0;


            for (int i = 0; i < 22; i++)
            {
                if (matrix[i, 0])
                    count++;
            }

            return count;
        }

        private int nonContingeneous()
        {
            int count = 0;

            for (int i = 0; i < 22; i++)
            {
                bool valid = false;
                int empty = 0;

                for (int j = 0; j < 10; j++)
                {
                    if (!matrix[i, j])
                        empty++;
                    else
                        valid = true;
                }

                if (valid)
                    count += empty;
            }

            return count;
        }

        private int countPeaks()
        {
            int count = 0;

            for (int i = 4; i < 22; i++)
            {
                for (int j = 2; j < 10; j++)
                {
                    if (!matrix[i, j] &&
                         matrix[i - 2, j - 1])
                    {
                        for (int ii = i - 1; ii >= 0; ii--)
                            if (matrix[ii, j - 1])
                                count++;
                            else
                                break;
                    }
                }

                for (int j = 1; j < 9; j++)
                {
                    if (!matrix[i, j] &&
                         matrix[i - 2, j + 1])
                    {
                        for (int ii = i - 2; ii >= 0; ii--)
                            if (matrix[ii, j + 1])
                                count++;
                            else
                                break;
                    }
                }
            }

            return count;
        }

        private int getBoardHeight()
        {
            int count = 0;

            for (int i = 0; i < 22 - 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i, j])
                        count += (22 - 9 - i) * (22 - 9 - i) * (22 - 9 - i);
                }
            }

            return count;
        }

        private int countBlocked()
        {
            int count = 0;

            for (int i = 1; i < 22; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (!matrix[i, j] && matrix[i - 1, j])
                    {
                        for (int ti = i; ti < 22; ti++)
                            if (!matrix[ti, j])
                                count++;
                            else
                                break;

                        for (int ti = i - 1; ti >= 0; ti--)
                            if (matrix[ti, j])
                                count++;
                            else
                                break;
                    }
                }
            }

            return count;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"Objective: {Objective}");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append($"Total lines cleared: {TotalLinesCleared}");
            stringBuilder.Append(Environment.NewLine);

            stringBuilder.Append($"Tetris: {Tetris}");
            stringBuilder.Append(Environment.NewLine);

            //stringBuilder.Append($"Hold: \n{ hold?.ToString() ?? "None"}");
            //stringBuilder.Append(Environment.NewLine);

            for (int i = 0; i < 22; i++)
            {
                for (int j = 0; j < 10; j++)
                {

                    stringBuilder.Append(matrix[i, j] ? 'X' : ' ');
                }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}
