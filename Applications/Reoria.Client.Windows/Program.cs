using Reoria.Client.Core.Application;
using Reoria.Engine.Application;

using ClientApplication application = new AppBootStrapper(args).CreateApplication<ClientApplication>();
application.Run();
