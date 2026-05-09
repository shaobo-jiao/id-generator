using IdGen.Api;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddOptions<IdOptions>()
    .BindConfiguration("IdOptions")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<IdOptions>, IdOptionsValidator>();




builder.Services.AddOptions<IdGeneratorOptions>()
    .BindConfiguration("IdGenerator")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<IdGenerator>();

var app = builder.Build();

app.MapGet("/newid", (IdGenerator idGenerator) => idGenerator.NewId());

app.Run();


