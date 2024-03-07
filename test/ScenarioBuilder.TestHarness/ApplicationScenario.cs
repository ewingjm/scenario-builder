namespace ScenarioBuilder.TestHarness;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScenarioBuilder.TestHarness.Events.Caseworker;
using ScenarioBuilder.TestHarness.Events.Caseworker.Composites;
using ScenarioBuilder.TestHarness.Events.PortalUser;
using ScenarioBuilder.TestHarness.Services;

/// <summary>
/// An example scenario for an application.
/// </summary>
[ComposeUsing(0, EventIds.ApplicationSubmission, typeof(PortalUserSubmitsApplicationEvent))]
[ComposeUsing(1, EventIds.ApplicationProcessing, typeof(CaseworkerProcessesApplicationEvent))]
public class ApplicationScenario : Scenario
{
    /// <summary>
    /// Gets the application submission info.
    /// </summary>
    public PortalUserSubmitsApplicationEvent.Info? ApplicationSubmission { get; internal set; }

    /// <summary>
    /// Gets the application assignment info.
    /// </summary>
    public CaseworkerAssignsApplicationEvent.Info? ApplicationProcessing_ApplicationAssignment { get; internal set; }

    /// <summary>
    /// Gets the application approval info.
    /// </summary>
    public CaseworkerSetsApprovalEvent.Info? ApplicationProcessing_ApplicationApproval { get; internal set; }

    /// <summary>
    /// The event IDs for the events within the <see cref="ApplicationScenario"/>.
    /// </summary>
    public static class EventIds
    {
        /// <summary>
        /// The event ID of the application submission event.
        /// </summary>
        public const string ApplicationSubmission = nameof(ApplicationSubmission);

        /// <summary>
        /// The event ID of the application processing event.
        /// </summary>
        public const string ApplicationProcessing = nameof(ApplicationProcessing);
    }

    /// <summary>
    /// A builder for the <see cref="ApplicationScenario"/>.
    /// </summary>
    /// <param name="clientFactory">A client factory.</param>
    public class Builder(IServiceClientFactory clientFactory) : Builder<ApplicationScenario>
    {
        private readonly IServiceClientFactory clientFactory = clientFactory;

        /// <summary>
        /// Configures the portal user submitting an application event.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <returns>The builder.</returns>
        public Builder PortalUserSubmitsApplication(Action<PortalUserSubmitsApplicationEvent.Builder>? configurator = null)
        {
            this.ConfigureEvent<PortalUserSubmitsApplicationEvent, PortalUserSubmitsApplicationEvent.Builder>(EventIds.ApplicationSubmission, configurator);

            return this;
        }

        /// <summary>
        /// Configures the caseworker processing an application event.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <returns>The builder.</returns>
        public Builder CaseworkerProcessesApplication(Action<CaseworkerProcessesApplicationEvent.Builder>? configurator = null)
        {
            this.ConfigureEvent<CaseworkerProcessesApplicationEvent, CaseworkerProcessesApplicationEvent.Builder>(EventIds.ApplicationProcessing, configurator);

            return this;
        }

        /// <inheritdoc/>
        protected override IServiceCollection InitializeServices(ServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton(this.clientFactory)
                .AddLogging(b => b.AddConsole());
        }
    }
}
