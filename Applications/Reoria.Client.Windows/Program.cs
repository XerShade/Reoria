using Reoria.Client.Core.Application;
using Reoria.Engine.Application;
using Reoria.Engine.Application.Enumerations;

using ClientApplication application = new AppBootStrapper(Platform.Windows, args).CreateApplication<ClientApplication>();
application.Run();
