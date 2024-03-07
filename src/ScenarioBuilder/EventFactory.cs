namespace ScenarioBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Factory class for events.
    /// </summary>
    public class EventFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public EventFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a composite event.
        /// </summary>
        /// <typeparam name="TCompositeEvent">The type of composite event.</typeparam>
        /// <param name="eventId">The event ID.</param>
        /// <param name="configuredEvents">The configured events and associated builders.</param>
        /// <param name="execution">The execution type.</param>
        /// <returns>The composite event.</returns>
        internal TCompositeEvent CreateCompositeEvent<TCompositeEvent>(string eventId, Dictionary<string, IEventBuilder<Event>> configuredEvents, CompositeEventExecution execution)
            where TCompositeEvent : CompositeEvent
        {
            var eventType = typeof(TCompositeEvent);
            var constructor = eventType.GetConstructors().FirstOrDefault();
            var resolvedArgs = this.ResolveEventConstructorArgs(constructor.GetParameters(), eventType, eventId, null, configuredEvents, execution);

            var compositeEvent = (TCompositeEvent)constructor.Invoke(resolvedArgs);

            compositeEvent.SetExecution(execution);
            foreach (var configuredEvent in configuredEvents)
            {
                compositeEvent.ConfigureEvent(configuredEvent.Key, configuredEvent.Value);
            }

            return compositeEvent;
        }

        /// <summary>
        /// Creates an event with constructor args.
        /// </summary>
        /// <typeparam name="TEvent">The type of event.</typeparam>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="constructorArgs">THe constructor args.</param>
        /// <returns>The event.</returns>
        internal TEvent CreateEvent<TEvent>(string eventId, object[] constructorArgs)
            where TEvent : Event
        {
            return (TEvent)this.CreateEvent(typeof(TEvent), eventId, constructorArgs);
        }

        /// <summary>
        /// Creates an event with constructor args.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="eventId">The event ID.</param>
        /// <param name="constructorArgs">The constructor args.</param>
        /// <returns>The event.</returns>
        internal Event CreateEvent(Type eventType, string eventId, object[] constructorArgs)
        {
            var constructor = eventType.GetConstructors().FirstOrDefault();
            var resolvedArgs = this.ResolveEventConstructorArgs(constructor.GetParameters(), eventType, eventId, constructorArgs);

            return (Event)constructor.Invoke(resolvedArgs);
        }

        private object[] ResolveEventConstructorArgs(ParameterInfo[] parameters, Type eventType, string eventId, object[] constructorArgs, params object[] additionalArgs)
        {
            var resolvedArgs = new object[parameters.Length];

            if (parameters.Length < constructorArgs?.Length)
            {
                throw new InvalidOperationException($"There are {parameters.Length} constructor parameters for the {eventType.Name} event but {constructorArgs.Length} were specified in the {nameof(ComposeUsingAttribute)} attribute.");
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i < constructorArgs?.Length)
                {
                    resolvedArgs[i] = constructorArgs[i];
                    continue;
                }

                if (parameters[i].ParameterType == typeof(EventFactory))
                {
                    resolvedArgs[i] = this;
                    continue;
                }

                if (parameters[i].ParameterType == typeof(string) && parameters[i].Name.Equals(nameof(eventId), StringComparison.InvariantCultureIgnoreCase))
                {
                    resolvedArgs[i] = eventId;
                    continue;
                }

                var service = this.serviceProvider.GetService(parameters[i].ParameterType);
                if (service != null)
                {
                    resolvedArgs[i] = service;
                    continue;
                }

                if (additionalArgs.Any(o => o.GetType() == parameters[i].ParameterType))
                {
                    resolvedArgs[i] = additionalArgs.FirstOrDefault(o => o.GetType() == parameters[i].ParameterType);
                    continue;
                }

                throw new InvalidOperationException($"The constructor parameter {parameters[i].Name} of type {parameters[i].ParameterType} could not be resolved for event {eventType.Name}. It was not provided by the {nameof(ComposeUsingAttribute)} or the service collection.");
            }

            return resolvedArgs;
        }

        private List<Event> CreateEvents(Type eventType, string eventId = null)
        {
            var composeAttributes = eventType
                    .GetCustomAttributes(typeof(ComposeUsingAttribute), false)
                    .OfType<ComposeUsingAttribute>();

            var duplicates = composeAttributes
                .GroupBy(a => a.Order)
                .Where(a => a.Count() > 1);

            if (duplicates.Any())
            {
                throw new InvalidOperationException($"Multiple events have the same order: {string.Join(", ", duplicates.Select(a => a.Key))}.");
            }

            return composeAttributes
                .Select(c => this.CreateEvent(c.ChildEventType, $"{(eventId != null ? $"{eventId}_" : string.Empty)}{c.EventId}", c.ConstructorArgs))
                .ToList();
        }
    }
}
