using System;

namespace SideScrollerShooter
{
    /// <summary>
    /// Application entry point. Creates and runs the Game1 instance.
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Instantiate and run the game loop
            using var game = new Game1(Array.Exists(args,arg=>arg=="--smoke-test"));
            game.Run();
        }
    }
}
