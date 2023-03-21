using Microsoft.EntityFrameworkCore;

namespace SCGame.Model{

    
    public class Board{
        public int BoardId {get;init;}
        public int FieldId {get;set;}
        public List<Field>? Fields {get;set;} 
    }
    
}