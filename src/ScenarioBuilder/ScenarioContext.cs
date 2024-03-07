namespace ScenarioBuilder
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a scenario context.
    /// </summary>
    public class ScenarioContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioContext"/> class.
        /// </summary>
        public ScenarioContext()
        {
            this.EventHistory = new Stack<Event>();
            this.Variables = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the variables set by the events which are mapped to the scenario data.
        /// </summary>/
        internal IDictionary<string, object> Variables { get; private set; }

        /// <summary>
        /// Gets the event execution history.
        /// </summary>
        internal Stack<Event> EventHistory { get; private set; }

        /// <summary>
        /// Sets a variable in the context.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        public void Set(string name, object value)
        {
            if (this.Variables.ContainsKey(name))
            {
                this.Variables[name] = value;
            }
            else
            {
                this.Variables.Add(name, value);
            }
        }

        /// <summary>
        /// Gets a variable from the context.
        /// </summary>
        /// <typeparam name="T">The type of the variable.</typeparam>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The value of the variable.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the variable hasn't been set.</exception>
        /// <exception cref="InvalidCastException">Thrown if the provided type doesn't match the variable type.</exception>
        public T Get<T>(string name)
        {
            if (!this.Variables.TryGetValue(name, out var value))
            {
                throw new KeyNotFoundException($"A value for {name} was not found in the scenario context.");
            }

            if (value is T t)
            {
                return t;
            }

            if (value is null)
            {
                return (T)value;
            }

            throw new InvalidCastException($"The value of {name} is not of the expected type. Found {value.GetType()} but expected {typeof(T)}");
        }
    }
}