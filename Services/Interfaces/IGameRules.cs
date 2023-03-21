

namespace SCGame.Services
{
    public interface IGameService
    {
        Task Move (Game game, int xPos, int yPos);
    }
}