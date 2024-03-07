namespace ScenarioBuilder
{
    /// <summary>
    /// An internal interface to limit access to the <see cref="Build"/> method of the event builders.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    internal interface IEventBuilder<out TEvent>
    {
        /// <summary>
        /// Builds the event.
        /// </summary>
        /// <returns>The event.</returns>
        TEvent Build();
    }
}