using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    public struct Tetrominoe
    {
        public enum ShapeType
        {
            L,
            J,
            S,
            Z,
            T,
            Line,
            Square,
        };

        public enum Rotation
        {
            Default,
            Anticlockwise,
            UpsideDown,
            Clockwise,
        };

        public bool[,] matrix = new bool[4, 4];

        private int hashCode = 0;

        public int Position { get; private set; } = 0;

        public ShapeType Shape { get; private set; } = ShapeType.Line;

        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;

        public Tetrominoe(ShapeType shape)
        {
            Shape = shape;

            switch (shape)
            {
                case ShapeType.L:
                    matrix[0, 2] = true;
                    matrix[1, 0] = true;
                    matrix[1, 1] = true;
                    matrix[1, 2] = true;
                    break;

                case ShapeType.J:
                    matrix[0, 0] = true;
                    matrix[1, 0] = true;
                    matrix[1, 1] = true;
                    matrix[1, 2] = true;
                    break;

                case ShapeType.S:
                    matrix[0, 1] = true;
                    matrix[0, 2] = true;
                    matrix[1, 0] = true;
                    matrix[1, 1] = true;
                    break;

                case ShapeType.Z:
                    matrix[0, 0] = true;
                    matrix[0, 1] = true;
                    matrix[1, 1] = true;
                    matrix[1, 2] = true;
                    break;

                case ShapeType.Square:
                    matrix[0, 0] = true;
                    matrix[0, 1] = true;
                    matrix[1, 0] = true;
                    matrix[1, 1] = true;
                    break;

                case ShapeType.T:
                    matrix[0, 1] = true;
                    matrix[1, 0] = true;
                    matrix[1, 1] = true;
                    matrix[1, 2] = true;
                    break;

                case ShapeType.Line:
                    matrix[0, 0] = true;
                    matrix[0, 1] = true;
                    matrix[0, 2] = true;
                    matrix[0, 3] = true;
                    break;
            }

            switch (Shape)
            {
                case ShapeType.L:
                case ShapeType.J:
                case ShapeType.S:
                case ShapeType.Z:
                case ShapeType.T:
                case ShapeType.Line:
                    Position = 3;
                    break;

                case ShapeType.Square:
                    Position = 4;
                    break;
            }

            fixEmptySpaces();

            calculateHashCode();
        }

        public Tetrominoe rotate(Rotation rotation)
        {
            var newTetrominoe = new Tetrominoe(Shape);

            switch (rotation)
            {
                case Rotation.Anticlockwise:
                    newTetrominoe.transpose();

                    if (Shape == ShapeType.Line)
                        newTetrominoe.Position = 4;

                    break;

                case Rotation.UpsideDown:
                    newTetrominoe.transpose();
                    newTetrominoe.transpose();
                    break;

                case Rotation.Clockwise:
                    newTetrominoe.transpose();
                    newTetrominoe.transpose();
                    newTetrominoe.transpose();

                    if (Shape == ShapeType.Line)
                        newTetrominoe.Position = 5;

                    if (Shape == ShapeType.S ||
                        Shape == ShapeType.Z ||
                        Shape == ShapeType.T ||
                        Shape == ShapeType.J ||
                        Shape == ShapeType.L)
                        newTetrominoe.Position = 4;

                    break;
            }

            newTetrominoe.fixEmptySpaces();

            newTetrominoe.calculateHashCode();

            return newTetrominoe;
        }

        private void calculateHashCode()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (matrix[i, j])
                        hashCode |= 1 << (i * 4 + j);
        }

        private void fixEmptySpaces()
        {
            int firstColumn = 4;
            int firstRow = 4;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (matrix[i, j])
                    {
                        firstRow = Math.Min(firstRow, i);
                        firstColumn = Math.Min(firstColumn, j);
                    }
                }
            }

            bool[,] temp_matrix = new bool[4, 4];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    temp_matrix[i, j] = matrix[i, j];

            Width = Height = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = temp_matrix[(i + firstRow) % 4, (j + firstColumn) % 4];

                    if (matrix[i, j])
                    {
                        Width = Math.Max(Width, j + 1);
                        Height = Math.Max(Height, i + 1);
                    }
                }
            }
        }

        private void transpose()
        {
            bool[,] temp_matrix = new bool[4, 4];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    temp_matrix[j, i] = matrix[i, 3 - j];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    matrix[i, j] = temp_matrix[i, j];
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    stringBuilder.Append(matrix[i, j] ? 'X' : ' ');
                }

                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        public int getPosition()
        {
            return 0;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return GetHashCode() == ((Tetrominoe) obj).GetHashCode();
        }

        public bool this[int row, int col]
        {
            get
            {
                return matrix[row, col];
            }
        }
    }
}