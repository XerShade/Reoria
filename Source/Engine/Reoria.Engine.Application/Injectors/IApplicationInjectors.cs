using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reoria.Engine.Application.Configuration.Interfaces;
using Reoria.Engine.Application.Phases;

namespace Reoria.Engine.Application.Injectors;

/// <summary>
/// Legacy interface - all functionality moved to specific phase interfaces.
/// </summary>
[Obsolete("Use specific phase interfaces: IBootstrapConfiguration, IBootstrapServices, IApplicationStart, IApplicationStop")]
public interface IApplicationInjector : IPhaseParticipant
{
    // All functionality inherited from IPhaseParticipant
}

/// <summary>
/// Legacy interface - use IApplicationStart and IApplicationStop instead.
/// </summary>
[Obsolete("Use IApplicationStart and IApplicationStop from Reoria.Engine.Application.Phases instead.")]
public interface IApplicationLifecycleInjector : IApplicationStart, IApplicationStop
{
    // All functionality inherited from IApplicationStart and IApplicationStop
}

/// <summary>
/// Legacy interface - use IBootstrapConfiguration instead.
/// </summary>
[Obsolete("Use IBootstrapConfiguration from Reoria.Engine.Application.Phases instead.")]
public interface IApplicationConfigurationInjector : IBootstrapConfiguration
{
    // All functionality inherited from IBootstrapConfiguration
}

/// <summary>
/// Legacy interface - use IBootstrapLogging instead.
/// </summary>
[Obsolete("Use IBootstrapLogging from Reoria.Engine.Application.Phases instead.")]
public interface IApplicationLoggingInjector : IBootstrapLogging
{
    // All functionality inherited from IBootstrapLogging
}

/// <summary>
/// Legacy interface - use IBootstrapServices and IBootstrapPostConfigure instead.
/// </summary>
[Obsolete("Use IBootstrapServices and IBootstrapPostConfigure from Reoria.Engine.Application.Phases instead.")]
public interface IApplicationServicesInjector : IBootstrapServices, IBootstrapPostConfigure
{
    // All functionality inherited from IBootstrapServices and IBootstrapPostConfigure
}