using IdGenerator.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddOptions<IdGeneratorOptions>()
    .BindConfiguration("IdGenerator")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IdGenerator.Api.IdGenerator>();

var app = builder.Build();

app.MapGet("/id", (IdGenerator.Api.IdGenerator idGenerator) => idGenerator.NewId());

app.Run();


