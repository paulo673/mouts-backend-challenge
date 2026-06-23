using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var app = builder.Build();

            ApplyMigrations(app);

            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

        WaitForDatabase(context);

        var pendingMigrations = context.Database.GetPendingMigrations().ToList();

        if (pendingMigrations.Count > 0)
        {
            Log.Information(
                "Applying {Count} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            context.Database.Migrate();
        }
        else
        {
            Log.Information("Database schema is up to date. No pending migrations.");
        }

        var stillPending = context.Database.GetPendingMigrations().ToList();
        if (stillPending.Count > 0)
        {
            throw new InvalidOperationException(
                $"Database is not up to date. Pending migrations: {string.Join(", ", stillPending)}");
        }

        Log.Information("Database migrations validated successfully.");
    }

    private static void WaitForDatabase(DefaultContext context)
    {
        const int maxAttempts = 2;
        var delay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (context.Database.CanConnect())
            {
                Log.Information("Database connection established (attempt {Attempt}).", attempt);
                return;
            }

            Log.Warning(
                "Database not ready (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}s...",
                attempt, maxAttempts, delay.TotalSeconds);
            Thread.Sleep(delay);
        }

        throw new InvalidOperationException(
            $"Could not connect to the database after {maxAttempts} attempts.");
    }
}
