
namespace SCGame.Services{
    public class PlayRoomHub : Hub, IPlayRoomHub
    {
        private PlayRoomRegistry _registry;
        private readonly GameDbConnection _context;

        public PlayRoomHub(PlayRoomRegistry registry, GameDbConnection context)
        {
            _registry = registry;
            _context = context;
        }
        public async Task<List<OutputMessage>> JoinRoom(RoomRequest request)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, request.Room);

            return _registry.GetMessages(request.Room)
                .Select(m => m.Output)
                .ToList();
        }

        public Task LeaveRoom(RoomRequest request)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, request.Room);
        }

        public Task SendMessage(InputMessage message, Player player)
        {
            
            var userMessage = new UserMessage(
                player,
                message.Message,
                message.Room,
                DateTimeOffset.Now
            );

            _registry.AddMessage(message.Room, userMessage);
            return Clients.GroupExcept(message.Room, new[] { Context.ConnectionId })
                .SendAsync("send_message", userMessage.Output);
        }
    }
}