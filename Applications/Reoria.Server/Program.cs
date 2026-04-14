using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;
using Reoria.Engine.Application.Interfaces;
using Reoria.Server.Core.Application;

using IApplication application = new AppBootStrapper(Platform.Server, args).CreateApplication<ServerApplication>();
application.Run();
