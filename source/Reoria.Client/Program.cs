using Reoria.Engine.Interfaces;
using Reoria.Client.Engine;
using Reoria.Engine;

IEngineServiceProvider serviceProvider = new EngineServiceProvider();
IEngineThread clientThread = new ClientThread(serviceProvider.ServiceProvider);
clientThread.Start();
