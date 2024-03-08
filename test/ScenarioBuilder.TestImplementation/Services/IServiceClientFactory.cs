namespace ScenarioBuilder.TestImplementation.Services
{
    using Microsoft.PowerPlatform.Dataverse.Client;

    /// <summary>
    /// A service client factory.
    /// </summary>
    public interface IServiceClientFactory
    {
        /// <summary>
        /// Gets a service client for the given persona.
        /// </summary>
        /// <param name="persona">The persona.</param>
        /// <returns>The service client.</returns>
        IOrganizationServiceAsync2 GetServiceClient(Persona persona);

        /// <summary>
        /// Gets a service client for the given user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The service client.</returns>
        IOrganizationServiceAsync2 GetServiceClient(Guid userId);
    }
}
