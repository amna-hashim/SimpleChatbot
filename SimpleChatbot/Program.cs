using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using SimpleChatbot.Infrastructure;
using SimpleChatbot.Interfaces;
using SimpleChatbot.Services;
using System.Net.Http.Headers;
using System.Text;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var config = new ConfigurationBuilder()
  .AddUserSecrets<Program>()
  .Build();

    var connectionString = builder.Configuration["ConnectionStrings:ChatDb"] ?? throw new InvalidOperationException("ConnectionStrings:ChatDb not found in user secrets.");

    builder.Services.AddDbContext<ChatDBContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<IChatService, ChatService>();
    builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
    builder.Services.AddScoped<IMessageRepository, MessageRepository>();

    //builder.Services.AddHttpClient<IEmbeddingService, OpenAiEmbeddingService>(client =>
    //{
    //    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    //    client.DefaultRequestHeaders.Authorization =
    //        new AuthenticationHeaderValue("Bearer", builder.Configuration["OpenAI:ApiKey"]);
    //});


    var ghPat = config["GH_PAT"]
    ?? throw new InvalidOperationException("GH_PAT not found in user secrets.");

    builder.Services.AddHttpClient<IEmbeddingService, GitHubModelsEmbeddingService>(client =>
    {
        client.BaseAddress = new Uri("https://models.inference.ai.azure.com/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ghPat);
    });

    builder.Services.AddHttpClient<IChatCompletionService, OpenAiChatCompletionService>(client =>
    {
        client.BaseAddress = new Uri("https://models.inference.ai.azure.com/");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ghPat);
    });

    var JwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not found in user secrets.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(JwtKey))
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.ContentType = "application/json";
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionFeature?.Error;

            var (statusCode, message) = ex switch
            {
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                InvalidOperationException => (StatusCodes.Status404NotFound, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new { error = message });
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();   // populates HttpContext.User
    app.UseAuthorization();    // enforces [Authorize] attributes

    app.MapControllers();  // required so routes actually get wired up
    app.Run();
}
catch(Exception ex)
{

}