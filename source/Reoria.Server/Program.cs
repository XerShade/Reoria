using Microsoft.Extensions.DependencyInjection;
using Reoria.Engine.Interfaces;
using Reoria.Server.Engine;

IEngineServiceProvider serviceProvider = new ServerServiceProvider();
IEngineThread serverThread = serviceProvider.ServiceProvider.GetRequiredService<ServerThread>();
serverThread.Start();
