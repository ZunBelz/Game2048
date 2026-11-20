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
                Console.WriteLine("W - Up");
                Console.WriteLine("S - Down");
                Console.WriteLine("A - Left");
                Console.WriteLine("D - Right");
                Console.WriteLine("U - Undo");
                Console.WriteLine("Q - Quit");

                if (board.HasWon())
                {
                    Console.WriteLine("You reached 2048!");
                }

                if (board.IsGameOver())
                {
                    Console.WriteLine("Game Over!");
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
                    GameState previousState = undoManager.Undo();

                    if (previousState != null)
                    {
                        board.RestoreState(previousState);
                    }

                    continue;
                }

                bool moved = false;

                undoManager.SaveState(board.GetState());

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
                    board.GenerateTile();
                }
            }
        }
    }

    public class Board
    {
        private const int Size = 4;
        private int[,] grid;
        private Random random;

        public Board()
        {
            grid = new int[Size, Size];
            random = new Random();
        }

        public void Display()
        {
            Console.WriteLine("2048");
            Console.WriteLine();

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Console.Write($"{grid[i, j],6}");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public void GenerateTile()
        {
            int emptyCount = 0;

            foreach (int cell in grid)
            {
                if (cell == 0)
                {
                    emptyCount++;
                }
            }

            if (emptyCount == 0)
                return;

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

                int[] merged = ProcessLine(row);

                for (int j = 0; j < Size; j++)
                {
                    if (grid[i, j] != merged[j])
                    {
                        changed = true;
                    }

                    grid[i, j] = merged[j];
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
                    (grid[i, j], grid[i, Size - j - 1]) =
                        (grid[i, Size - j - 1], grid[i, j]);
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
            return new GameState((int[,])grid.Clone());
        }

        public void RestoreState(GameState state)
        {
            grid = (int[,])state.Matrix.Clone();
        }

        public bool HasWon()
        {
            foreach (int value in grid)
            {
                if (value == 2048)
                    return true;
            }

            return false;
        }

        public bool IsGameOver()
        {
            foreach (int value in grid)
            {
                if (value == 0)
                    return false;
            }

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size - 1; j++)
                {
                    if (grid[i, j] == grid[i, j + 1])
                        return false;

                    if (grid[j, i] == grid[j + 1, i])
                        return false;
                }
            }

            return true;
        }
    }

    public class GameState
    {
        public int[,] Matrix { get; }

        public GameState(int[,] matrix)
        {
            Matrix = matrix;
        }
    }

    public class UndoManager
    {
        private GenericStack<GameState> history =
            new GenericStack<GameState>();

        public void SaveState(GameState state)
        {
            history.Push(state);
        }

        public GameState Undo()
        {
            if (history.IsEmpty())
                return null;

            return history.Pop();
        }
    }

    public class GenericStack<T>
    {
        private Node<T> top;

        public void Push(T item)
        {
            Node<T> node = new Node<T>(item);

            node.Next = top;
            top = node;
        }

        public T Pop()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException(
                    "Stack is empty.");
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
                    "Stack is empty.");
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
}
