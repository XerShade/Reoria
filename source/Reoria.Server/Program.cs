using Reoria.Engine.Interfaces;
using Reoria.Server.Engine;

IEngineServiceProvider serviceProvider = new ServerServiceProvider();
IEngineThread serverThread = new ServerThread(serviceProvider.ServiceProvider);
serverThread.Start();
