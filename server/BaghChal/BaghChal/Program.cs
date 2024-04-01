using BaghChal.Contracts;
using BaghChal.Infrastructure;
using BaghChal.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<PostgresOptions>(
    builder.Configuration.GetSection(nameof(PostgresOptions)));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IBoardRepository, BoardRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
// builder.Services.AddReact();
//builder.Services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName).AddChakraCore();
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", policy =>
{
    policy.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true);
}));
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() /* || app.Environment.IsProduction()*/ )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseDeveloperExceptionPage();
 
/*app.UseReact(config => { });
app.UseDefaultFiles();
app.UseStaticFiles();*/

//app.UseHttpsRedirection();
app.MapHub<BoardHub>("/board");

app.UseCors("CorsPolicy");
//app.UseAuthorization();

app.MapControllers();

app.Run();