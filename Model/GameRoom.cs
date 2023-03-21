

namespace SCGame.Model
{
    public record Player(string UserId, string UserName);

    public record RoomRequest(string Room);

    public record InputMessage(
        Game Message,
        string Room
    );

    public record OutputMessage(
        Game Message,
        string UserName,
        string Room,
        DateTimeOffset SentAt
    );

    public record UserMessage(
        Player Player,
        Game Message,
        string Room,
        DateTimeOffset SentAt
    )
    {
        public OutputMessage Output => new(Message, Player.UserName, Room, SentAt);
    }
}