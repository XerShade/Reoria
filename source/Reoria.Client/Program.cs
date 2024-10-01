using Reoria.Client.Engine;
using Reoria.Engine.Interfaces;

IEngineServiceProvider serviceProvider = new ClientServiceProvider();
IEngineThread clientThread = new ClientThread(serviceProvider.ServiceProvider);
clientThread.Start();
