namespace ScenarioBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents a scenario.
    /// </summary>
    public abstract class Scenario
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Scenario"/> class.
        /// </summary>
        public Scenario()
        {
            this.Context = new ScenarioContext();
        }

        /// <summary>
        /// Gets or sets the context used to generate this scenario.
        /// </summary>
        internal ScenarioContext Context { get; set; }

        /// <summary>
        /// Represents a scenario builder.
        /// </summary>
        /// <typeparam name="TScenario">The type of scenario.</typeparam>
        public abstract class Builder<TScenario>
            where TScenario : Scenario, new()
        {
            private readonly Mapper mapper;
            private readonly Dictionary<string, IEventBuilder<Event>> configuredEvents;
            private readonly Dictionary<string, ComposeUsingAttribute> composition;
            private readonly List<string> eventIds;
            private readonly Dictionary<string, Event> eventCache;

            private EventFactory eventFactory;
            private EventBuilderFactory eventBuilderFactory;
            private ServiceProvider serviceProvider;
            private CompositeEventExecution execution;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder{TCompositeEvent}"/> class.
            /// </summary>
            /// <param name="logger">A logger.</param>
            public Builder(ILogger logger = null)
            {
                this.mapper = new Mapper(new MapperConfiguration(cfg =>
                {
                    cfg.ShouldMapProperty = p => true;
                    cfg.ShouldMapField = f => false;
                    cfg.ShouldMapMethod = p => false;
                    cfg.CreateMap(this.GetType(), typeof(TScenario));
                }));

                var composeUsingAttributes = typeof(TScenario)
                    .GetCustomAttributes(typeof(ComposeUsingAttribute), false)
                    .OfType<ComposeUsingAttribute>();
                this.composition = composeUsingAttributes.ToDictionary(a => a.EventId, a => a);
                this.eventIds = composeUsingAttributes.OrderBy(a => a.Order).Select(a => a.EventId).ToList();

                this.eventCache = new Dictionary<string, Event>();
                this.configuredEvents = new Dictionary<string, IEventBuilder<Event>>();
                this.execution = CompositeEventExecution.AllEvents;
                this.Logger = logger;
            }

            /// <summary>
            /// Gets a logger.
            /// </summary>
            protected ILogger Logger { get; }

            private IServiceProvider ServiceProvider
            {
                get
                {
                    if (this.serviceProvider is null)
                    {
                        this.serviceProvider = this
                            .InitializeServiceCollection()
                            .BuildServiceProvider();
                    }

                    return this.serviceProvider;
                }
            }

            private EventFactory EventFactory
            {
                get
                {
                    if (this.eventFactory is null)
                    {
                        this.eventFactory = new EventFactory(this.ServiceProvider);
                    }

                    return this.eventFactory;
                }
            }

            private EventBuilderFactory EventBuilderFactory
            {
                get
                {
                    if (this.eventBuilderFactory is null)
                    {
                        this.eventBuilderFactory = new EventBuilderFactory(this.EventFactory);
                    }

                    return this.eventBuilderFactory;
                }
            }

            /// <summary>
            /// Builds upon an existing scenario.
            /// </summary>
            /// <param name="scenario">The existing scenario.</param>
            /// <returns>The updated scenario.</returns>
            public async Task<TScenario> BuildAsync(TScenario scenario = null)
            {
                if (scenario != null && scenario.Context == null)
                {
                    throw new InvalidOperationException("The provided scenario does not have a context.");
                }

                scenario = scenario ?? new TScenario();

                var eventsToExecute = this.eventIds;

                if (this.execution == CompositeEventExecution.ConfiguredAndPreviousEvents)
                {
                    eventsToExecute = eventsToExecute
                        .Take(eventsToExecute.FindLastIndex(e => this.configuredEvents.ContainsKey(e)) + 1)
                        .ToList();
                }

                foreach (var eventId in eventsToExecute)
                {
                    await this.GetEvent(eventId).FireAsync(scenario.Context);
                }

                return this.MapContextToScenario(scenario);
            }

            /// <summary>
            /// Override this method to initialize services your events depend on.
            /// </summary>
            /// <returns>The initialized service collection.</returns>
            protected IServiceCollection InitializeServiceCollection()
            {
                return this.InitializeServices(new ServiceCollection());
            }

            /// <summary>
            /// Initializes services on the service collection.
            /// </summary>
            /// <param name="serviceCollection">The service collection.</param>
            /// <returns>The initialized service collection.</returns>
            protected virtual IServiceCollection InitializeServices(ServiceCollection serviceCollection)
            {
                return serviceCollection;
            }

            /// <summary>
            /// Configures an event in the scenario event pipeline with the specified builder configurator.
            /// </summary>
            /// <typeparam name="TEvent">The type of event.</typeparam>
            /// <typeparam name="TEventBuilder">The type of event builder.</typeparam>
            /// <param name="eventId">The event ID.</param>
            /// <param name="configurator">The configurator function.</param>
            protected void ConfigureEvent<TEvent, TEventBuilder>(string eventId, Action<TEventBuilder> configurator = null)
                where TEvent : Event
                where TEventBuilder : Event.Builder<TEvent>
            {
                var composeUsingAttribute = this.composition[eventId];

                var builder = this.EventBuilderFactory
                    .CreateEventBuilder<TEventBuilder, TEvent>(eventId, composeUsingAttribute.ConstructorArgs);

                configurator?.Invoke(builder);

                this.configuredEvents[eventId] = builder;
                this.eventCache.Remove(eventId);

                this.execution = CompositeEventExecution.ConfiguredAndPreviousEvents;
            }

            /// <summary>
            /// Configures an event in the scenario event pipeline.
            /// </summary>
            /// <typeparam name="TEvent">The type of event.</typeparam>
            /// <param name="eventId">The event ID.</param>
            protected void ConfigureEvent<TEvent>(string eventId)
                where TEvent : Event, new()
            {
                this.configuredEvents[eventId] = null;

                this.execution = CompositeEventExecution.ConfiguredAndPreviousEvents;
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

                    @event = this.EventFactory.CreateEvent(
                        composeUsing.ChildEventType,
                        composeUsing.EventId,
                        composeUsing.ConstructorArgs);
                }

                this.eventCache.Add(eventId, @event);

                return @event;
            }

            private TScenario MapContextToScenario(TScenario scenario)
            {
                this.mapper.Map(scenario.Context.Variables, scenario);

                return scenario;
            }
        }
    }
}