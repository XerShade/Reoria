using Reoria.Engine;
using Reoria.Engine.Interfaces;
using Reoria.Server.Engine;

IEngineServiceProvider serviceProvider = new EngineServiceProvider();
IEngineThread serverThread = new ServerThread(serviceProvider.ServiceProvider);
serverThread.Start();
