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
