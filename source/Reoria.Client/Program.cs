using Microsoft.Extensions.DependencyInjection;
using Reoria.Client.Engine;
using Reoria.Engine.Interfaces;

IEngineServiceProvider serviceProvider = new ClientServiceProvider();
IEngineThread clientThread = serviceProvider.ServiceProvider.GetRequiredService<ClientThread>();
clientThread.Start();
