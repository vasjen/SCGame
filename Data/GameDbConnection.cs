

namespace SCGame.Data{
    public class GameDbConnection : IdentityUserContext<IdentityUser>{
        private readonly IConfiguration _config;

        public DbSet<Game>? Games {get;set;}
        public DbSet<Invite>? Invites {get;set;}
        public DbSet<Board>? Boards {get;set;}

        public GameDbConnection(DbContextOptions<GameDbConnection> options, IConfiguration config)
        : base(options)
    {
        _config = config;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var conStrBuilder = new SqlConnectionStringBuilder(
            _config.GetConnectionString("GameDbConnection"));
            conStrBuilder.Password = _config.GetValue<string>("DbPassword");
            var connection = conStrBuilder.ConnectionString;
            optionsBuilder.UseSqlServer(connection);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Game>()
                    .Property(p=>p.Status)
                    .HasConversion<string>();
               
        
    }
  

    }
}