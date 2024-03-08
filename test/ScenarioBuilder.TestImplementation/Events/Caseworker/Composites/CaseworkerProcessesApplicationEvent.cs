namespace ScenarioBuilder.TestImplementation.Events.Caseworker.Composites
{
    /// <summary>
    /// A composite event for a caseworker processing an application.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CaseworkerProcessesApplicationEvent"/> class.
    /// </remarks>
    /// <param name="eventsFactory">The event factory.</param>
    /// <param name="eventId">The ID of the event.</param>
    [ComposeUsing(0, EventIds.ApplicationAssignment, typeof(CaseworkerAssignsApplicationEvent))]
    [ComposeUsing(1, EventIds.ApplicationApproval, typeof(CaseworkerSetsApprovalEvent))]
    public class CaseworkerProcessesApplicationEvent(EventFactory eventsFactory, string eventId)
        : CompositeEvent(eventsFactory, eventId)
    {
        /// <summary>
        /// The IDs for the events within the <see cref="CaseworkerProcessesApplicationEvent"/> event.
        /// </summary>
        public static class EventIds
        {
            /// <summary>
            /// The event ID of the application assignment event.
            /// </summary>
            public const string ApplicationAssignment = nameof(ApplicationAssignment);

            /// <summary>
            /// The event ID of the application approval event.
            /// </summary>
            public const string ApplicationApproval = nameof(ApplicationApproval);
        }

        /// <summary>
        /// A builder for the <see cref="CaseworkerProcessesApplicationEvent"/>.
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="Builder"/> class.
        /// </remarks>
        /// <param name="eventBuilderFactory">The event builder factory.</param>
        /// <param name="eventFactory">The event factory.</param>
        /// <param name="eventId">The ID of the event.</param>
        public class Builder(EventBuilderFactory eventBuilderFactory, EventFactory eventFactory, string eventId)
            : Builder<CaseworkerProcessesApplicationEvent>(eventBuilderFactory, eventFactory, eventId)
        {
            /// <summary>
            /// Configures the assigning of the application.
            /// </summary>
            /// <param name="configurator">The configurator.</param>
            /// <returns>The builder.</returns>
            public Builder ByAssigningTheApplication(Action<CaseworkerAssignsApplicationEvent.Builder>? configurator = null)
            {
                this.ConfigureEvent<CaseworkerAssignsApplicationEvent, CaseworkerAssignsApplicationEvent.Builder>(EventIds.ApplicationAssignment, configurator);

                return this;
            }

            /// <summary>
            /// Configures the setting of the application approval.
            /// </summary>
            /// <param name="configurator">The configurator.</param>
            /// <returns>The builder.</returns>
            public Builder BySettingApplicationApproval(Action<CaseworkerSetsApprovalEvent.Builder>? configurator = null)
            {
                this.ConfigureEvent<CaseworkerSetsApprovalEvent, CaseworkerSetsApprovalEvent.Builder>(EventIds.ApplicationApproval, configurator);

                return this;
            }
        }
    }
}
