using IdGenerator.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddOptions<SnowflakeIdGeneratorOptions>()
    .BindConfiguration("SnowflakeIdGenerator")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<SnowflakeIdGenerator>();

var app = builder.Build();

app.MapGet("/newid", (SnowflakeIdGenerator idGenerator) => idGenerator.NewId());

app.Run();


