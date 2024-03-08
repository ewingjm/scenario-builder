﻿namespace ScenarioBuilder.TestImplementation.Events.PortalUser
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Xrm.Sdk;
    using ScenarioBuilder.TestImplementation.Services;

    /// <summary>
    /// An event for a portal user submitting an application.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PortalUserSubmitsApplicationEvent"/> class.
    /// </remarks>
    /// <param name="clientFactory">A client factory.</param>
    /// <param name="logger">A logger.</param>
    /// <param name="eventId">The ID of the event.</param>
    public class PortalUserSubmitsApplicationEvent(IServiceClientFactory clientFactory, ILogger<PortalUserSubmitsApplicationEvent> logger, string eventId)
        : Event(eventId)
    {
        private readonly IServiceClientFactory clientFactory = clientFactory;
        private readonly ILogger logger = logger;

        private int? numberOfGoods;

        /// <inheritdoc/>
        public override async Task ExecuteAsync(ScenarioContext context)
        {
            this.logger.LogInformation("Submitting application.");

            var application = new Entity("application");
            var numberOfGoods = this.numberOfGoods ?? 10;
            application.Attributes.Add("numberofgoods", numberOfGoods);

            this.logger.LogInformation("Creating application with {NumberOfGoods} goods.", numberOfGoods);

            var applicationId = await this.clientFactory
                .GetServiceClient(Persona.PortalUser)
                .CreateAsync(new Entity("application"));

            this.logger.LogInformation("Created application with ID {ApplicationId}.", applicationId);

            context.Set(this.EventId, new Info { SubmittedApplicationId = applicationId });
        }

        /// <summary>
        /// A builder for the <see cref="PortalUserSubmitsApplicationEvent"/> event.
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="Builder"/> class.
        /// </remarks>
        /// <param name="eventFactory">An event factory.</param>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="constructorArgs">Constructor args.</param>
        public class Builder(EventFactory eventFactory, string eventId, object[] constructorArgs)
            : Builder<PortalUserSubmitsApplicationEvent>(eventFactory, eventId, constructorArgs)
        {
            private int? numberOfGoods;

            /// <summary>
            /// Overrides the number of goods on the application.
            /// </summary>
            /// <param name="numberOfGoods">The number of goods.</param>
            /// <returns>The builder.</returns>
            public Builder WithNumberOfGoods(int numberOfGoods)
            {
                this.numberOfGoods = numberOfGoods;

                return this;
            }
        }

        /// <summary>
        /// Info generated by the <see cref="PortalUserSubmitsApplicationEvent"/> event.
        /// </summary>
        public class Info
        {
            /// <summary>
            /// Gets the submitted application ID.
            /// </summary>
            public Guid SubmittedApplicationId { get; internal set; }
        }
    }
}