using CashewPro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Infrastructure (EF Core & DbContext)
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS Policy (Localhost 5173, 5174, etc. anni support chestundi)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllLocalhost", policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CashewPro API v1");
    });
}

// CRITICAL: Middleware Order - UseCors must come before Authorization & MapControllers
app.UseRouting();

app.UseCors("AllowAllLocalhost");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();