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
        private static Dictionary<BigInteger, double> memoization = new Dictionary<BigInteger, double>();
        public Tetrominoe? hold = null;
        public int TotalLinesCleared { get; private set; } = 0;
        public int Tetris { get; private set; } = 0;
        public int Single { get; private set; } = 0;
        public int Double { get; private set; } = 0;
        public int Triple { get; private set; } = 0;

        private bool[,] matrix = new bool[22, 10];
        public double Objective { get; private set; } = 0.0;
        public double PonderedLinesCleared { get; private set; } = 0.0;

        public Board() { }

        public Board(Board other)
        {
            for (int i = 0; i < 22; i++)
                for (int j = 0; j < 10; j++)
                    matrix[i, j] = other.matrix[i, j];

            hold = other.hold;

            TotalLinesCleared = other.TotalLinesCleared;
            Tetris = other.Tetris;
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
        }

        private double calculateStateObjective()
        {
            Objective = 0.0;

            clearLines();

            Objective += Single * -10;

            Objective += Double * 5;

            Objective += Triple * 10;

            Objective += Tetris * 40;

            Objective += getBoardHeight() * -1;

            Objective += countBlocked() * -200;

            Objective += countPeaks() * -20;

            Objective += contingeneous() * 5;

            Objective += firstColumn() * -10;

            if (hold.HasValue && hold.Value.Shape == Tetrominoe.ShapeType.Line)
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

        private int contingeneous()
        {
            int count = 0;

            for(int i = 0; i < 22; i++)
                for(int j = 2; j < 10; j++)
                    if(matrix[i, j - 1] && matrix[i, j])
                        count++;

            return count;
        }

        private int countPeaks()
        {
            int count = 0;

            for (int i = 4; i < 22; i++)
            {
                for (int j = 1; j < 10; j++)
                {
                    if (!matrix[i, j] &&
                         matrix[i - 1, j - 1])
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
                         matrix[i - 1, j + 1])
                    {
                        for (int ii = i - 1; ii >= 0; ii--)
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

            for (int i = 0; i < 22 - 8; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (matrix[i, j])
                        count += (22 - 4 - i) * (22 - 4 - i) * (22 - 4 - i);
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
