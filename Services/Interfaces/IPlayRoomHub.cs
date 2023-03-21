namespace SCGame.Services
{
    public interface IPlayRoomHub
    {
        Task<List<OutputMessage>> JoinRoom(RoomRequest request);
        Task LeaveRoom(RoomRequest request);
        Task SendMessage(InputMessage message, Player player);
    }
}