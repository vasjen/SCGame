

namespace SCGame.Services{
    public class GameRules : IGameService{
        private readonly GameDbConnection _context;

        public GameRules(GameDbConnection context)
        {
            _context = context;
        }
        public async Task Move (Game game, int xPos, int yPos){


        int index = 3 * (yPos - 1) + xPos;
        
        int[,] _board = new int[3,3];
    
        for (int i = 0; i < 3; i++){
            for (int j = 0; j < 3; j++){
                int tempIndex = 3 * (i) + j;
                _board[i,j] = (int)game.Board.Fields[tempIndex].FieldValue;
            }
        }
        game.Moves++; 
        if (game.Queue == false)   
        {
            _board[xPos-1,yPos-1] = 1;
            game.Board.Fields[index-1].FieldValue = 1;

        }
        else
        {
            _board[xPos-1,yPos-1] = 2;
            game.Board.Fields[index-1].FieldValue = 2;
        }

        var win = false;
        for (var i = 0; i < 3 && !win; i++)
        {
            win = IsWinningLine(_board[i, 0], _board[i, 1], _board[i, 2]);
        }
        if (!win)
        {
            for (var i = 0; i < 3 && !win; i++)
            {
                win = IsWinningLine(_board[0, i], _board[1, i], _board[2, i]);
            }
        }

        if (!win)
        {
            win = IsWinningLine(_board[0, 0], _board[1, 1], _board[2, 2]);
        }

        if (!win)
        {
            win = IsWinningLine(_board[0, 2], _board[1, 1], _board[2, 0]);
        }
           
        
        var draw = false;
        if (game.Moves is 9)
        {
            draw = true;  // we could try to look for stalemate earlier, if we wanted 
        }

        // handle end of game
      //  Dictionary<int,int> afterBoard = new Dictionary<int, int>();
        if (win || draw)
        {
            // game over
            game.Status = GameStatus.Finished;
            if (win)
            {
                switch (game.Queue){
                    case false :
                                game.Winner = game.FirstPlayerName;  
                                break;
                    case true :
                                game.Winner = game.SecondPlayerName;  
                                break;
                }
                
            }
            


        }
         //   for (int i = 0; i< 3; i++){
         //       for (int j = 0; j < 3; j++){
         //           afterBoard.Add((i) * 3 + j + 1, _board[i,j]);
         //       }
         //   }
            game.Queue=!game.Queue;
            await _context.SaveChangesAsync();
           // return afterBoard;
        }
        private static bool IsWinningLine(int i, int j, int k) => (i, j, k) switch
        {
            (1, 1, 1) => true,
            (2, 2, 2) => true,
            _ => false
        };

        
    }
}