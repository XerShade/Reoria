using Reoria.Engine.Application;
using Reoria.Engine.Application.Interfaces;
using Reoria.Server.Application;

using IApplication application = new AppBootStrapper(args).CreateApplication<ServerApplication>();
application.Run();
