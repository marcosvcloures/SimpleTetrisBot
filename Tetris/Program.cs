// See https://aka.ms/new-console-template for more information

using static Tetris.Tetrominoe;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Tetris // Note: actual namespace depends on the project name.
{
    public class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        static List<Tuple<Color, Tetrominoe>> tetrominoes = new List<Tuple<Color, Tetrominoe>>();
        
        static IntPtr bluestacks = IntPtr.Zero;

        static Board board = new Board();
         
        static Random random = new Random(10);

        static Tetrominoe current = new Tetrominoe(ShapeType.Square);

        static bool canHold = true;

        static object mutex = new object();

        static Bitmap bmpScreenshot = new Bitmap(1920, 1080);

        [STAThread]
        public static void Main(string[] args)
        {
            var rand = new Random(0);

            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 251, 233, 187), new Tetrominoe(ShapeType.Square)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 179, 227, 231), new Tetrominoe(ShapeType.Line)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 140, 100, 178), new Tetrominoe(ShapeType.T)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 179, 243, 173), new Tetrominoe(ShapeType.S)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 239, 164, 183), new Tetrominoe(ShapeType.Z)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 149, 178, 230), new Tetrominoe(ShapeType.J)));
            tetrominoes.Add(Tuple.Create(Color.FromArgb(255, 239, 182, 130), new Tetrominoe(ShapeType.L)));

            for (int i = 0; (i < 60) && (bluestacks == IntPtr.Zero); i++)
            {
                Thread.Sleep(500);

                var processes = Process.GetProcessesByName("HD-Player");
                //var processes = Process.GetProcessesByName("notepad");

                if (processes.Length > 0)
                    bluestacks = processes[0].MainWindowHandle;
            }

            Play();
        }

        static void Play()
        {
            lock (mutex)
            {
                SetForegroundWindow(bluestacks);

                new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_C);
                Thread.Sleep(20);
                new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_C);
                Thread.Sleep(600);

                List<Tetrominoe> tetrominoesQueue = new List<Tetrominoe>();
                int currentPosition = -1;

                var nextTetrominoes = getNextTetrominos();

                tetrominoesQueue.Add(nextTetrominoes.Item1);
                tetrominoesQueue.Add(nextTetrominoes.Item2);
                tetrominoesQueue.Add(nextTetrominoes.Item3);

                while (bluestacks != IntPtr.Zero)
                {
                    SetForegroundWindow(bluestacks);

                    Thread.Sleep(300);

                    var updateTetrominoesQueue = void () =>
                    {
                        nextTetrominoes = getNextTetrominos();

                        if (nextTetrominoes.Item1.Shape == tetrominoesQueue[tetrominoesQueue.Count - 3].Shape &&
                            nextTetrominoes.Item2.Shape == tetrominoesQueue[tetrominoesQueue.Count - 2].Shape &&
                            nextTetrominoes.Item3.Shape == tetrominoesQueue[tetrominoesQueue.Count - 1].Shape)
                            return;

                        if (nextTetrominoes.Item1.Shape == tetrominoesQueue[tetrominoesQueue.Count - 2].Shape &&
                            nextTetrominoes.Item2.Shape == tetrominoesQueue[tetrominoesQueue.Count - 1].Shape)
                        {
                            tetrominoesQueue.Add(nextTetrominoes.Item3);

                            return;
                        }

                        if (nextTetrominoes.Item1.Shape == tetrominoesQueue[tetrominoesQueue.Count - 3].Shape)
                        {
                            tetrominoesQueue.Add(nextTetrominoes.Item2);
                            tetrominoesQueue.Add(nextTetrominoes.Item3);

                            return;
                        }

                        tetrominoesQueue.Add(nextTetrominoes.Item1);
                        tetrominoesQueue.Add(nextTetrominoes.Item2);
                        tetrominoesQueue.Add(nextTetrominoes.Item3);

                        return;
                    };

                    updateTetrominoesQueue();

                    Console.WriteLine($"Current: {current.Shape}");
                    Console.WriteLine($"Hold: {board.hold?.Shape}");

                    Console.Write("Next tetronimoes:");
                    for(int i = currentPosition + 1; i < tetrominoesQueue.Count; i++)
                        Console.Write($"{tetrominoesQueue[i].Shape} ");
                    Console.Write("\n");

                    var nextMovement = movement(board, current, tetrominoesQueue[currentPosition + 1], canHold);

                    if (nextMovement.Item3)
                    {
                        canHold = false;

                        Console.WriteLine($"movement: hold {current.Shape}");

                        if (board.hold.HasValue)
                        {
                            var temp = board.hold.Value;

                            board.hold = current;
                            current = temp;
                        }
                        else
                        {
                            board.hold = current;

                            currentPosition++;

                            current = tetrominoesQueue[currentPosition];
                        }

                        new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_W);
                        Thread.Sleep(20);
                        new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_W);
                        Thread.Sleep(20);
                    }
                    else
                    {
                        int rotationPresses = 0;

                        canHold = true;

                        switch (nextMovement.Item4)
                        {
                            case Rotation.Anticlockwise:
                                rotationPresses = 1;
                                break;
                            case Rotation.UpsideDown:
                                rotationPresses = 2;
                                break;
                            case Rotation.Clockwise:
                                rotationPresses = 3;
                                break;
                        }

                        int leftPresses = 0, rightPresses = 0;

                        var tetrominoe = current.rotate(nextMovement.Item4);

                        if (nextMovement.Item2 < tetrominoe.Position)
                            leftPresses = tetrominoe.Position - nextMovement.Item2;
                        else if (nextMovement.Item2 > tetrominoe.Position)
                            rightPresses = nextMovement.Item2 - tetrominoe.Position;

                        Console.WriteLine($"movement: place\n{current.Shape}");
                        Console.WriteLine($"rotate: {rotationPresses}, left: {leftPresses}, right: {rightPresses}");

                        int totalTetrises = board.Tetris;

                        board.placeTetrominoe(tetrominoe, nextMovement.Item2);

                        currentPosition++;

                        current = tetrominoesQueue[currentPosition];

                        for (int i = 0; i < rotationPresses; i++)
                        {
                            new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_Q);
                            Thread.Sleep(20);
                            new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_Q);

                            Thread.Sleep(20);
                        }

                        for (int i = 0; i < leftPresses; i++)
                        {
                            new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_A);
                            Thread.Sleep(20);
                            new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_A);

                            Thread.Sleep(20);
                        }

                        for (int i = 0; i < rightPresses; i++)
                        {
                            new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_D);
                            Thread.Sleep(20);
                            new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);

                            Thread.Sleep(20);
                        }

                        new WindowsInput.InputSimulator().Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.SPACE);
                        Thread.Sleep(20);
                        new WindowsInput.InputSimulator().Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.SPACE);

                        Thread.Sleep(20);

                        //Console.WriteLine(board.ToString());

                        if (board.Tetris > totalTetrises)
                        {
                            Thread.Sleep(200);
                        }
                    }

                    Console.WriteLine("-----------------------------");
                    //PostMessage(bluestacks, 0x104, 0x43, 0);
                }
            }
        }

        static Tuple<double, int, bool, Rotation> movement(Board board, Tetrominoe tetrominoe, Tetrominoe? next, bool canSwap)
        {
            int bestColumn = 0;
            Rotation bestRotation = Rotation.Default;
            double bestFO = double.MinValue;

            object mutex = new object();

            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (tetrominoe.Shape == ShapeType.Square && rotation > 1)
                    break;

                if ((tetrominoe.Shape == ShapeType.S ||
                     tetrominoe.Shape == ShapeType.Z ||
                     tetrominoe.Shape == ShapeType.Line) && rotation > 2)
                    break;

                var tempTetrominoe = tetrominoe.rotate((Rotation)rotation);

                Parallel.For(0, 11 - tempTetrominoe.Width, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, j =>
                {
                    var tempBoard = new Board(board);

                    double newFO = tempBoard.placeTetrominoe(tempTetrominoe, j);

                    if (next.HasValue)
                    {
                        newFO = movement(tempBoard, next.Value, null, true).Item1;
                    }

                    lock (mutex)
                    {
                        if (newFO > bestFO)
                        {
                            //Console.WriteLine($"Place { tempTetrominoe.Shape } at { j }: { newFO }");

                            bestFO = newFO;
                            bestRotation = (Rotation)rotation;
                            bestColumn = j;
                        }
                    }
                });
            }

            if (!canSwap)
                return Tuple.Create(bestFO, bestColumn, false, bestRotation);

            var tempBoard = new Board(board);

            tempBoard.hold = tetrominoe;

            var dontSwap = Tuple.Create(bestFO, bestColumn, false, bestRotation);
            var moveResult = Tuple.Create(double.MinValue, bestColumn, false, bestRotation);

            if (board.hold.HasValue)
                moveResult = movement(tempBoard, board.hold.Value, next, false);
            else if (next.HasValue)
                moveResult = movement(tempBoard, next.Value, null, false);

            if (moveResult.Item1 > bestFO)
                return Tuple.Create(moveResult.Item1, moveResult.Item2, true, moveResult.Item4);

            return dontSwap;
        }

        static Tuple<Tetrominoe, Tetrominoe, Tetrominoe> getNextTetrominos()
        {
            //return new Tetrominoe((ShapeType) random.Next(Enum.GetValues(typeof(ShapeType)).Length));

            using (var g = Graphics.FromImage(bmpScreenshot))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmpScreenshot.Size, CopyPixelOperation.SourceCopy);
            }

            var getTetronimoeByPixel = (Color pixel) => {
                var closest = double.MaxValue;
                Tetrominoe? tetronimoe = null;

                foreach (var tuple in tetrominoes)
                {
                    double distance = Math.Pow(pixel.R - tuple.Item1.R, 2) + Math.Pow(pixel.G - tuple.Item1.G, 2) + Math.Pow(pixel.B - tuple.Item1.B, 2);
                    
                    if (distance < closest)
                    {
                        closest = distance;
                        tetronimoe = tuple.Item2;
                    }
                }

                return tetronimoe.Value;
            };

            var firstTetronimoe = getTetronimoeByPixel(bmpScreenshot.GetPixel(535, 287));
            var secondTetronimoe = getTetronimoeByPixel(bmpScreenshot.GetPixel(535, 360));
            var thirdTetronimoe = getTetronimoeByPixel(bmpScreenshot.GetPixel(535, 433));

            return Tuple.Create(firstTetronimoe, secondTetronimoe, thirdTetronimoe);
        }
    }
}