using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reoria.Engine.Interfaces;
public interface IEngineServiceProvider
{
    IConfigurationRoot Configuration { get; }
    ServiceProvider ServiceProvider { get; }
}