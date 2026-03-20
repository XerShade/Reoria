using Reoria.Engine.Application;
using Reoria.Engine.Application.Threads;

IGameThread engine = new AppBuilder(args).Build();
engine.Run();
