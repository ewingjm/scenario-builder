namespace ScenarioBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a composite event.
    /// </summary>
    public abstract class CompositeEvent : Event
    {
        private readonly Dictionary<string, IEventBuilder<Event>> configuredEvents;
        private readonly Dictionary<string, ComposeUsingAttribute> composition;
        private readonly List<string> eventIds;
        private readonly Dictionary<string, Event> eventCache;
        private readonly EventFactory eventFactory;

        private CompositeEventExecution execution;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeEvent"/> class.
        /// </summary>
        /// <param name="eventsFactory">The events factory.</param>
        /// <param name="eventId">The ID of the event.</param>
        public CompositeEvent(EventFactory eventsFactory, string eventId)
            : base(eventId)
        {
            this.eventFactory = eventsFactory ?? throw new ArgumentNullException(nameof(eventsFactory));

            var composeUsingAttributes = this.GetType()
                .GetCustomAttributes(typeof(ComposeUsingAttribute), false)
                .OfType<ComposeUsingAttribute>();
            this.composition = composeUsingAttributes.ToDictionary(a => a.EventId, a => a);
            this.eventIds = composeUsingAttributes.OrderBy(a => a.Order).Select(a => a.EventId).ToList();
            this.eventCache = new Dictionary<string, Event>();
            this.configuredEvents = new Dictionary<string, IEventBuilder<Event>>();
            this.execution = CompositeEventExecution.AllEvents;
        }

        /// <summary>
        /// Executes the composite event by firing all child events.
        /// </summary>
        /// <param name="context">The scenario context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteAsync(ScenarioContext context)
        {
            var eventsToExecute = this.eventIds;

            if (this.execution == CompositeEventExecution.ConfiguredEvents)
            {
                eventsToExecute = eventsToExecute.Where(e => this.configuredEvents.ContainsKey(e)).ToList();
            }
            else if (this.execution == CompositeEventExecution.ConfiguredAndPreviousEvents)
            {
                eventsToExecute = eventsToExecute
                    .Take(eventsToExecute.FindLastIndex(e => this.configuredEvents.ContainsKey(e)) + 1)
                    .ToList();
            }

            foreach (var eventId in eventsToExecute)
            {
                await this.GetEvent(eventId).FireAsync(context);
            }
        }

        /// <summary>
        /// Fires the event. This will always result in the <see cref="ExecuteAsync(ScenarioContext)"/> method being called which will fire all child events.
        /// </summary>
        /// <param name="context">The scenario context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal override async Task FireAsync(ScenarioContext context)
        {
            // We always need to fire child events for composite events. We don't want to add composite events to the history.
            await this.ExecuteAsync(context);
        }

        /// <summary>
        /// Configures an event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="builder">The builder.</param>
        internal void ConfigureEvent(string eventId, IEventBuilder<Event> builder = null)
        {
            this.configuredEvents[eventId] = builder;
        }

        /// <summary>
        /// Sets the execution type.
        /// </summary>
        /// <param name="execution">The execution type.</param>
        internal void SetExecution(CompositeEventExecution execution)
        {
            this.execution = execution;
        }

        private Event GetEvent(string eventId)
        {
            if (this.eventCache.TryGetValue(eventId, out Event cachedEvent))
            {
                return cachedEvent;
            }

            Event @event;
            if (this.configuredEvents.TryGetValue(eventId, out IEventBuilder<Event> eventBuilder) && eventBuilder != null)
            {
                @event = eventBuilder.Build();
            }
            else
            {
                var composeUsing = this.composition[eventId];

                @event = this.eventFactory.CreateEvent(
                    composeUsing.ChildEventType,
                    $"{this.EventId}_{composeUsing.EventId}",
                    composeUsing.ConstructorArgs);
            }

            this.eventCache.Add(eventId, @event);

            return @event;
        }

        /// <summary>
        /// Represents a composite event builder.
        /// </summary>
        /// <typeparam name="TCompositeEventType">The type of composite event.</typeparam>
        public new abstract class Builder<TCompositeEventType> : Event.Builder<TCompositeEventType>
            where TCompositeEventType : CompositeEvent
        {
            private readonly EventBuilderFactory eventBuilderFactory;

            private readonly Dictionary<string, IEventBuilder<Event>> configuredEvents;
            private readonly Dictionary<string, ComposeUsingAttribute> composition;
            private CompositeEventExecution execution;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder{TCompositeEvent}"/> class.
            /// </summary>
            /// <param name="eventBuilderFactory">The event builder factory.</param>
            /// <param name="eventFactory">The event factory.</param>
            /// <param name="eventId">The ID of the event.</param>
            public Builder(EventBuilderFactory eventBuilderFactory, EventFactory eventFactory, string eventId)
                : base(eventFactory, eventId)
            {
                this.eventBuilderFactory = eventBuilderFactory ?? throw new ArgumentNullException(nameof(eventBuilderFactory));

                var composeUsingAttributes = typeof(TCompositeEventType)
                    .GetCustomAttributes(typeof(ComposeUsingAttribute), false)
                    .OfType<ComposeUsingAttribute>();
                this.composition = composeUsingAttributes.ToDictionary(a => a.EventId, a => a);

                this.configuredEvents = new Dictionary<string, IEventBuilder<Event>>();
                this.execution = CompositeEventExecution.AllEvents;
            }

            /// <summary>
            /// Indicates that all events prior to the configured events should be executed.
            /// </summary>
            public void AndAllPreviousSteps()
            {
                this.execution = CompositeEventExecution.ConfiguredAndPreviousEvents;
            }

            /// <summary>
            /// Indicates that all events prior to and after the configured events should be executed.
            /// </summary>
            public void AndAllOtherSteps()
            {
                this.execution = CompositeEventExecution.AllEvents;
            }

            /// <summary>
            /// Configures an event in the composite event pipeline with the given builder configurator.
            /// </summary>
            /// <typeparam name="TEvent">The type of event.</typeparam>
            /// <typeparam name="TEventBuilder">The type of event builder.</typeparam>
            /// <param name="eventId">The name of the event.</param>
            /// <param name="configurator">The configurator function.</param>
            protected void ConfigureEvent<TEvent, TEventBuilder>(string eventId, Action<TEventBuilder> configurator = null)
                where TEvent : Event
                where TEventBuilder : Event.Builder<TEvent>
            {
                var composeUsingAttribute = this.composition[eventId];

                var builder = this.eventBuilderFactory
                    .CreateEventBuilder<TEventBuilder, TEvent>($"{this.EventId}_{eventId}", composeUsingAttribute.ConstructorArgs);

                configurator?.Invoke(builder);

                this.configuredEvents[eventId] = builder;

                this.execution = CompositeEventExecution.ConfiguredEvents;
            }

            /// <summary>
            /// Configures an event in the composite event pipeline.
            /// </summary>
            /// <typeparam name="TEvent">The type of event.</typeparam>
            /// <param name="eventId">The name of the event.</param>
            protected void ConfigureEvent<TEvent>(string eventId)
                where TEvent : Event
            {
                this.configuredEvents[eventId] = null;
                this.execution = CompositeEventExecution.ConfiguredEvents;
            }

            /// <summary>
            /// Builds the composite event.
            /// </summary>
            /// <returns>The composite event.</returns>
            protected override TCompositeEventType Build()
            {
                return this.EventFactory.CreateCompositeEvent<TCompositeEventType>(this.EventId, this.configuredEvents, this.execution);
            }
        }
    }
}