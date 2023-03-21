namespace SCGame.Model{
    public class Invite {

        public int InviteId {get; init;}
        public int GameId {get; init;}
        public string FirstPlayer {get; init;}
        public string? SecondPlayer {get; set;}
        public Game Game {get;set;}

    }
}