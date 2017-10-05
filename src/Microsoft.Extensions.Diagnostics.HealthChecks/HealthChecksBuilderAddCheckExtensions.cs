using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// This type is used like this:
//  services.AddHealthChecks()
//   .AddFoo()
//   .AddBar()
//   .AddBaz()
// As a result, we put it in Microsoft.Extensions.DependencyInjection so the user actually sees the extension methods (since
// they may not have the Microsoft.Extensions.HealthChecks namespace in their usings).
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering <see cref="IHealthCheck"/> instances in an <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    public static class HealthChecksBuilderAddCheckExtensions
    {
        /// <summary>
        /// Adds a new health check with the specified name and implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="name">The name of the health check, which should indicate the component being checked.</param>
        /// <param name="check">A delegate which provides the code to execute when the health check is run.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/></returns>
        public static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, string name, Func<CancellationToken, Task<HealthCheckResult>> check)
        {
            builder.Services.AddSingleton<IHealthCheck>(services => new HealthCheck(name, check));
            return builder;
        }

        /// <summary>
        /// Adds a new health check with the specified name and implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="name">The name of the health check, which should indicate the component being checked.</param>
        /// <param name="check">A delegate which provides the code to execute when the health check is run.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/></returns>
        public static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, string name, Func<Task<HealthCheckResult>> check) =>
            builder.AddCheck(name, _ => check());
    }
}
