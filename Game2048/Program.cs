using System;

namespace Game2048
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Run();
        }
    }

    public class Game
    {
        private Board board;
        private UndoManager undoManager;

        public Game()
        {
            board = new Board();
            undoManager = new UndoManager();

            board.GenerateTile();
            board.GenerateTile();
        }

        public void Run()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();

                board.Display();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("W - Up");
                Console.WriteLine("S - Down");
                Console.WriteLine("A - Left");
                Console.WriteLine("D - Right");
                Console.WriteLine("U - Undo");
                Console.WriteLine("Q - Quit");
                Console.ResetColor();

                if (board.HasWon())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nCongratulations! You reached 2048!");
                    Console.ResetColor();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();

                    break;
                }

                if (board.IsGameOver())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nGame Over!");
                    Console.ResetColor();

                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    break;
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Q)
                {
                    running = false;
                    continue;
                }

                if (key == ConsoleKey.U)
                {
                    if (undoManager.CanUndo())
                    {
                        GameState previousState = undoManager.Undo();

                        if (previousState != null)
                        {
                            board.RestoreState(previousState);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No moves to undo.");
                        Console.ReadKey();
                    }

                    continue;
                }

                GameState currentState = board.GetState();

                bool moved = false;

                switch (key)
                {
                    case ConsoleKey.W:
                        moved = board.MoveUp();
                        break;

                    case ConsoleKey.S:
                        moved = board.MoveDown();
                        break;

                    case ConsoleKey.A:
                        moved = board.MoveLeft();
                        break;

                    case ConsoleKey.D:
                        moved = board.MoveRight();
                        break;
                }

                if (moved)
                {
                    undoManager.SaveState(currentState);
                    board.GenerateTile();
                }
            }
        }
    }
}
public class Board
{
    private const int Size = 4;

    private int[,] grid;
    private Random random;

    public int Score { get; private set; }

    public Board()
    {
        grid = new int[Size, Size];
        random = new Random();
        Score = 0;
    }

    public void Display()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=========== 2048 ===========");
        Console.WriteLine($"Score: {Score}");
        Console.WriteLine("============================");
        Console.ResetColor();

        Console.WriteLine();

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                PrintTile(grid[i, j]);
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private void PrintTile(int value)
    {
        switch (value)
        {
            case 0:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;

            case 2:
                Console.ForegroundColor = ConsoleColor.White;
                break;

            case 4:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;

            case 8:
                Console.ForegroundColor = ConsoleColor.Green;
                break;

            case 16:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case 32:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;

            case 64:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;

            case 128:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;

            case 256:
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                break;

            case 512:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case 1024:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;

            case 2048:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                break;

            default:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;
        }

        string text = value == 0 ? "." : value.ToString();

        Console.Write($"{text,6}");

        Console.ResetColor();
    }

    public void GenerateTile()
    {
        int emptyCount = 0;

        foreach (int value in grid)
        {
            if (value == 0)
            {
                emptyCount++;
            }
        }

        if (emptyCount == 0)
        {
            return;
        }

        int target = random.Next(emptyCount);

        int counter = 0;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (grid[i, j] == 0)
                {
                    if (counter == target)
                    {
                        grid[i, j] =
                            random.Next(10) < 9 ? 2 : 4;

                        return;
                    }

                    counter++;
                }
            }
        }
    }

    public bool MoveLeft()
    {
        bool changed = false;

        for (int i = 0; i < Size; i++)
        {
            int[] row = new int[Size];

            for (int j = 0; j < Size; j++)
            {
                row[j] = grid[i, j];
            }

            int[] processed = ProcessLine(row);

            for (int j = 0; j < Size; j++)
            {
                if (grid[i, j] != processed[j])
                {
                    changed = true;
                }

                grid[i, j] = processed[j];
            }
        }

        return changed;
    }

    public bool MoveRight()
    {
        ReverseRows();

        bool moved = MoveLeft();

        ReverseRows();

        return moved;
    }

    public bool MoveUp()
    {
        RotateLeft();

        bool moved = MoveLeft();

        RotateRight();

        return moved;
    }

    public bool MoveDown()
    {
        RotateRight();

        bool moved = MoveLeft();

        RotateLeft();

        return moved;
    }

    private int[] ProcessLine(int[] line)
    {
        int[] temp = new int[Size];

        int index = 0;

        foreach (int value in line)
        {
            if (value != 0)
            {
                temp[index++] = value;
            }
        }

        for (int i = 0; i < Size - 1; i++)
        {
            if (temp[i] != 0 &&
                temp[i] == temp[i + 1])
            {
                temp[i] *= 2;

                Score += temp[i];

                temp[i + 1] = 0;
            }
        }

        int[] result = new int[Size];

        index = 0;

        foreach (int value in temp)
        {
            if (value != 0)
            {
                result[index++] = value;
            }
        }

        return result;
    }
    private void ReverseRows()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size / 2; j++)
            {
                int temp = grid[i, j];

                grid[i, j] = grid[i, Size - 1 - j];

                grid[i, Size - 1 - j] = temp;
            }
        }
    }

    private void RotateLeft()
    {
        int[,] newGrid = new int[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                newGrid[Size - 1 - j, i] = grid[i, j];
            }
        }

        grid = newGrid;
    }

    private void RotateRight()
    {
        int[,] newGrid = new int[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                newGrid[j, Size - 1 - i] = grid[i, j];
            }
        }

        grid = newGrid;
    }

    public GameState GetState()
    {
        return new GameState(
            (int[,])grid.Clone(),
            Score
        );
    }

    public void RestoreState(GameState state)
    {
        grid = (int[,])state.Matrix.Clone();
        Score = state.Score;
    }

    public bool HasWon()
    {
        foreach (int value in grid)
        {
            if (value == 2048)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsGameOver()
    {
        foreach (int value in grid)
        {
            if (value == 0)
            {
                return false;
            }
        }

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size - 1; j++)
            {
                if (grid[i, j] == grid[i, j + 1])
                {
                    return false;
                }

                if (grid[j, i] == grid[j + 1, i])
                {
                    return false;
                }
            }
        }

        return true;
    }
}

public class GameState
{
    public int[,] Matrix { get; private set; }

    public int Score { get; private set; }

    public GameState(int[,] matrix, int score)
    {
        Matrix = matrix;
        Score = score;
    }
}
public class UndoManager
{
    private GenericStack<GameState> history;

    public UndoManager()
    {
        history = new GenericStack<GameState>();
    }

    public void SaveState(GameState state)
    {
        history.Push(state);
    }

    public GameState Undo()
    {
        if (history.IsEmpty())
        {
            return null;
        }

        return history.Pop();
    }

    public bool CanUndo()
    {
        return !history.IsEmpty();
    }
}

public class GenericStack<T>
{
    private Node<T> top;

    public GenericStack()
    {
        top = null;
    }

    public void Push(T item)
    {
        Node<T> newNode = new Node<T>(item);

        newNode.Next = top;
        top = newNode;
    }

    public T Pop()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException(
                "Stack is empty."
            );
        }

        T value = top.Data;

        top = top.Next;

        return value;
    }

    public T Peek()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException(
                "Stack is empty."
            );
        }

        return top.Data;
    }

    public bool IsEmpty()
    {
        return top == null;
    }
}

public class Node<T>
{
    public T Data { get; set; }

    public Node<T> Next { get; set; }

    public Node(T data)
    {
        Data = data;
        Next = null;
    }
}
