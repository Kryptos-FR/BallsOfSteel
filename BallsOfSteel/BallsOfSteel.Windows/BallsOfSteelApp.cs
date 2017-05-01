using SiliconStudio.Xenko.Engine;

namespace BallsOfSteel
{
    class BallsOfSteelApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
