namespace ScenarioBuilder
{
    using System.Threading.Tasks;
    using AutoMapper;

    /// <summary>
    /// Represents an event to be executed..
    /// </summary>
    public abstract class Event
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="eventId">The ID of the event.</param>
        public Event(string eventId)
        {
            this.EventId = eventId;
        }

        /// <summary>
        /// Gets the ID of the event.
        /// </summary>
        public string EventId { get; private set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is Event))
            {
                return false;
            }

            return this.EventId.Equals(((Event)obj).EventId);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.EventId.GetHashCode();
        }

        /// <summary>
        /// Execute the event.
        /// </summary>
        /// <param name="context">The scenario context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task ExecuteAsync(ScenarioContext context);

        /// <summary>
        /// Fires the event. If the event has already been executed then this will do nothing.
        /// </summary>
        /// <param name="context">The scenario context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal virtual async Task FireAsync(ScenarioContext context)
        {
            if (context.EventHistory.Contains(this))
            {
                // Do nothing. This event has already been executed.
                return;
            }

            await this.ExecuteAsync(context);
            context.EventHistory.Push(this);
        }

        /// <summary>
        /// Represents an event builder.
        /// </summary>
        /// <typeparam name="TEvent">The type of event built by this event builder.</typeparam>
        public abstract class Builder<TEvent> : IEventBuilder<TEvent>
            where TEvent : Event
        {
            private readonly Mapper mapper;
            private readonly object[] constructorArgs;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder{TEvent}"/> class.
            /// </summary>
            /// <param name="eventFactory">The event factory.</param>
            /// <param name="eventId">The ID of the event.</param>
            /// <param name="constructorArgs">The constructor args for the event.</param>
            public Builder(EventFactory eventFactory, string eventId, object[] constructorArgs = null)
            {
                this.EventFactory = eventFactory;
                this.EventId = eventId;
                this.constructorArgs = constructorArgs;
                this.mapper = new Mapper(new MapperConfiguration(cfg =>
                {
                    cfg.ShouldMapProperty = p => false;
                    cfg.ShouldMapField = p => p.IsPrivate;
                    cfg.ShouldMapMethod = p => false;
                    cfg.CreateMap(this.GetType(), typeof(TEvent))
                        .ConstructUsing((o) => eventFactory.CreateEvent<TEvent>(eventId, this.constructorArgs));
                }));
            }

            /// <summary>
            /// Gets the event factory.
            /// </summary>
            protected EventFactory EventFactory { get; private set; }

            /// <summary>
            /// Gets the ID of the event to build.
            /// </summary>
            protected string EventId { get; }

            /// <summary>
            /// Builds the event (internal use only).
            /// </summary>
            /// <returns>The event.</returns>
            TEvent IEventBuilder<TEvent>.Build()
            {
                return this.Build();
            }

            /// <summary>
            /// Builds the event. Override this method for custom mapping.
            /// </summary>
            /// <returns>The event.</returns>
            protected virtual TEvent Build()
            {
                return this.mapper.Map<TEvent>(this);
            }
        }
    }
}