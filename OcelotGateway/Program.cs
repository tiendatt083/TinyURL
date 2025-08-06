using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add controllers for JSON handling
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Configuration.AddJsonFile(
  "ocelot.json",
  optional: false,
  reloadOnChange: true
  );

// Configure HttpClient to accept self-signed certificates in development
builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<IgnoreSslHandler>(true);

// Add custom SSL handler for development
builder.Services.AddTransient<IgnoreSslHandler>();

//(1) config CORS   => VERY IMPORTANT
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

//(2) enable CORS    => VERY IMPORTANT
app.UseCors("CorsPolicy");

app.UseAuthorization();
app.MapControllers();
app.UseOcelot().Wait();

app.Run();

// Custom SSL handler for development
public class IgnoreSslHandler : DelegatingHandler
{
    public IgnoreSslHandler()
    {
        InnerHandler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
    }
}