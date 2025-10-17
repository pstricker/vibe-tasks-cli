using Spectre.Console.Cli;
using Microsoft.Extensions.DependencyInjection;
using VibeTasks.Core;
using VibeTasks.Cli.Services;

namespace VibeTasks;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public TypeRegistrar()
    {
        _services.AddSingleton<AppConfig>(_ => AppConfig.Load());
        _services.AddSingleton<DataStore>();
        _services.AddSingleton<SearchService>();
        _services.AddSingleton<RollForwardService>();
        _services.AddSingleton<GitIntegration>();
    }

    public ITypeResolver Build() => new TypeResolver(_services.BuildServiceProvider());

    public void Register(Type service, Type implementation) => _services.AddSingleton(service, implementation);
    public void RegisterInstance(Type service, object implementation) => _services.AddSingleton(service, implementation);
    public void RegisterLazy(Type service, Func<object> factory) => _services.AddSingleton(service, _ => factory());
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly ServiceProvider _provider;
    public TypeResolver(ServiceProvider provider) { _provider = provider; }
    public object? Resolve(Type? type)
    {
        if (type is null) return null;
        return _provider.GetService(type);
    }

    public void Dispose() => _provider.Dispose();
}
