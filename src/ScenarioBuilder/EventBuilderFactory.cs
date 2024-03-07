namespace ScenarioBuilder
{
    using System;
    using System.Linq;

    /// <summary>
    /// Factory class for event builders.
    /// </summary>
    public class EventBuilderFactory
    {
        private readonly EventFactory eventFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBuilderFactory"/> class.
        /// </summary>
        /// <param name="eventFactory">The event factory.</param>
        public EventBuilderFactory(EventFactory eventFactory)
        {
            this.eventFactory = eventFactory;
        }

        /// <summary>
        /// Creates an event builder.
        /// </summary>
        /// <typeparam name="TEventBuilder">The type of the event builder.</typeparam>
        /// <typeparam name="TEvent">The type of the event built by the event builder.</typeparam>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="constructorArgs">Constructor args for the event.</param>
        /// <returns>The event builder.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no suitable constructors are found.</exception>
        internal TEventBuilder CreateEventBuilder<TEventBuilder, TEvent>(string eventId, params object[] constructorArgs)
            where TEventBuilder : Event.Builder<TEvent>
            where TEvent : Event
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException($"'{nameof(eventId)}' cannot be null or empty.", nameof(eventId));
            }

            var eventBuilderType = typeof(TEventBuilder);

            var constructor = eventBuilderType.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                return (TEventBuilder)Activator.CreateInstance(eventBuilderType);
            }

            var parameters = constructor.GetParameters();
            var resolvedArgs = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;

                if (parameterType == typeof(EventBuilderFactory))
                {
                    resolvedArgs[i] = this;
                    continue;
                }
                else if (parameterType == typeof(EventFactory))
                {
                    resolvedArgs[i] = this.eventFactory;
                    continue;
                }
                else if (parameterType == typeof(string) && parameters[i].Name.Equals(nameof(eventId), StringComparison.InvariantCultureIgnoreCase))
                {
                    resolvedArgs[i] = eventId;
                    continue;
                }
                else if (parameters[i].ParameterType == typeof(object[]) && parameters[i].Name.Equals(nameof(constructorArgs), StringComparison.InvariantCultureIgnoreCase))
                {
                    resolvedArgs[i] = constructorArgs;
                    continue;
                }

                throw new InvalidOperationException($"The constructor parameter {parameters[i].Name} of type {parameters[i].ParameterType} could not be resolved for event builder {eventBuilderType.Name}.");
            }

            return (TEventBuilder)Activator.CreateInstance(eventBuilderType, resolvedArgs);
        }
    }
}