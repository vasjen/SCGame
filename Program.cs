

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddTransient<IConfiguration>(sp =>
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json")
                                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                .AddUserSecrets<Program>();
            return configurationBuilder.Build();
        });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Swagger SCGame Documentation",
            Version = "v1",
            Description = "API Documentation for the TicTakToe game",
            Contact = new OpenApiContact
            {
                Name = "Vasilii Mukhin",
                Email = "vasjenm@gmail.com"
            },
            Extensions = new Dictionary<string, IOpenApiExtension>
            {
              {"x-logo", new OpenApiObject
                {
                   {"url", new OpenApiString($"{AppDomain.CurrentDomain.BaseDirectory}/logo.png")},
                   { "altText", new OpenApiString("TicTak logo")}
                }
              }
            }
           
        });
    options.EnableAnnotations();
});
builder.Services.AddDbContext<GameDbConnection>();
builder.Services.AddIdentityCore<IdentityUser>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<GameDbConnection>();
builder.Services.AddScoped<ITokenCreationService, JwtService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer =  builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
        
    });
builder.Services.AddSignalR();
builder.Services.AddSingleton<PlayRoomRegistry>();
builder.Services.AddScoped<IGameService,GameRules>();
builder.Services.AddScoped<IPlayRoomHub,PlayRoomHub>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => 
        options.SwaggerEndpoint("/swagger/v1/swagger.json",
        "Swagger SCGame Documentation v1"));
    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Swagger SCGame Documentation v1";
        options.SpecUrl = "/swagger/v1/swagger.json";
    });
}
app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<PlayRoomHub>("/playroom");
                endpoints.MapDefaultControllerRoute();
            });
app.Run();



