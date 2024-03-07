namespace ScenarioBuilder
{
    using System;

    /// <summary>
    /// Add an event to the scenario or composite event pipelines.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ComposeUsingAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposeUsingAttribute"/> class.
        /// </summary>
        /// <param name="order">The order of the event in the execution pipeline.</param>
        /// <param name="eventId">A unique identifier for the event.</param>
        /// <param name="childEventType">The type of event to execute.</param>
        /// <param name="constructorArgs">The constructor args used to instantiate the event.</param>
        /// <exception cref="ArgumentNullException">Thrown if the child event type is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the child event type is not a subclass of <see cref="Event"/>.</exception>
        public ComposeUsingAttribute(int order, string eventId, Type childEventType, object[] constructorArgs = null)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                throw new ArgumentException($"'{nameof(eventId)}' cannot be null or empty.", nameof(eventId));
            }

            if (childEventType is null)
            {
                throw new ArgumentNullException(nameof(childEventType));
            }

            if (!childEventType.IsSubclassOf(typeof(Event)))
            {
                throw new ArgumentException("The provided type must be a subclass of Event.", nameof(childEventType));
            }

            this.Order = order;
            this.EventId = eventId;
            this.ChildEventType = childEventType;
            this.ConstructorArgs = constructorArgs;
        }

        /// <summary>
        /// Gets the order of the event in the execution pipeline.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets the event ID.
        /// </summary>
        public string EventId { get; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public Type ChildEventType { get; }

        /// <summary>
        /// Gets the constructor args used to instantiate the event.
        /// </summary>
        public object[] ConstructorArgs { get; }
    }
}