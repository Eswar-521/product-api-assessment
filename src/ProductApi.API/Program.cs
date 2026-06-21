using ProductApi.API.Extensions;
using ProductApi.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAssessmentApi(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
});

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .AllowAnonymous();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", checkedAt = DateTime.UtcNow }))
    .AllowAnonymous()
    .WithName("Health");

app.Run();

public partial class Program
{
}
