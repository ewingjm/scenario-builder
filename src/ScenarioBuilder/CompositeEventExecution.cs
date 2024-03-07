namespace ScenarioBuilder
{
    /// <summary>
    /// Indicates how the events in an event pipeline should be executed.
    /// </summary>
    public enum CompositeEventExecution
    {
        /// <summary>
        /// Executes all events in the pipeline.
        /// </summary>
        AllEvents,

        /// <summary>
        /// Executes only the events that have been explicitly configured.
        /// </summary>
        ConfiguredEvents,

        /// <summary>
        /// Executes the events that have been explicitly configured and all events preceding them.
        /// </summary>
        ConfiguredAndPreviousEvents,
    }
}