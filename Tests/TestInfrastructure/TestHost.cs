using Application;
using Application.Common;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.TestInfrastructure;

public sealed class TestHost : IDisposable
{
    public FakeCurrentUserService CurrentUser { get; } = new();

    public ServiceProvider Services { get; }

    public TestHost(TestDatabaseFixture fixture)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddApplication();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(fixture.ConnectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService>(_ => CurrentUser);

        services.Configure<LearningPlatformOptions>(_ => { });

        Services = services.BuildServiceProvider();
    }

    public async Task<Result<T>> SendAsync<T>(
        IRequest<Result<T>> request)
    {
        using var scope = Services.CreateScope();

        var mediator =
            scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(request);
    }

    public async Task<Result> SendAsync(IRequest<Result> request)
    {
        using var scope = Services.CreateScope();

        var mediator =
            scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(request);
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> action)
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await action(context);
    }

    public Task ExecuteAsync(
        Func<ApplicationDbContext, Task> action)
    {
        return ExecuteAsync(async context =>
        {
            await action(context);
            return true;
        });
    }

    public void Dispose()
    {
        Services.Dispose();
        GC.SuppressFinalize(this);
    }
}
