namespace ScenarioBuilder.IntegrationTests;

using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Moq;
using ScenarioBuilder.TestImplementation;
using ScenarioBuilder.TestImplementation.Services;

/// <summary>
/// Integration tests for the <see cref="ApplicationScenario.Builder"/> class.
/// </summary>
[TestClass]
public class ScenarioBuilderTests
{
    private readonly Mock<IOrganizationServiceAsync2> mockServiceClient;
    private readonly Mock<IServiceClientFactory> mockServiceClientFactory;

    private readonly ApplicationScenario.Builder builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioBuilderTests"/> class.
    /// </summary>
    public ScenarioBuilderTests()
    {
        this.mockServiceClient = new Mock<IOrganizationServiceAsync2>();
        this.mockServiceClientFactory = new Mock<IServiceClientFactory>();
        this.builder = new ApplicationScenario.Builder(this.mockServiceClientFactory.Object);

        this.ConfigureMockDefaults();
    }

    /// <summary>
    /// Tests that context variables with names matching the scenario property names are mapped to the property.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_ScenarioPropertyMatchesContextVariableName_MapsContextVariableToScenarioProperty()
    {
        var applicationId = Guid.NewGuid();
        this.mockServiceClient.Setup(s => s.CreateAsync(It.IsAny<Entity>())).Returns(Task.FromResult(applicationId));

        var scenario = await this.builder
            .PortalUserSubmitsApplication()
            .BuildAsync();

        scenario.ApplicationSubmission!.SubmittedApplicationId.Should().Be(applicationId);
    }

    /// <summary>
    /// Tests that events after the configured root events do not execute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_RootEventConfigured_EventsAfterConfiguredEventsDoNotExecute()
    {
        var scenario = await this.builder
            .PortalUserSubmitsApplication()
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().BeNull();
    }

    /// <summary>
    /// Tests that events after the configured root events do not execute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_RootEventConfigured_EventsPrecedingConfiguredEventExecute()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication()
            .BuildAsync();

        scenario.ApplicationSubmission.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that all child events within a composite event will execute if not explicitly configured.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_CompositeEventChildEventsNotConfigured_AllChildEventsExecute()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication()
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that only configured child events within a composite event will execute if explicitly configured.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_CompositeEventChildEventConfigured_ConfiguredChildEventExecutes()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication(a => a
                .ByAssigningTheApplication())
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().BeNull();
    }

    /// <summary>
    /// Tests that multiple configured child events within a composite event will all execute.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_CompositeEventChildEventsConfigured_ConfiguredChildEventsExecutes()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication(a => a
                .ByAssigningTheApplication()
                .BySettingApplicationApproval())
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that configuring a composite event with 'AndAllPreviousSteps' will execute events prior to the configured events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_CompositeEventConfiguredWithAndAllPreviousSteps_EventsPrecedingConfiguredEventsWillExecute()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication(a => a
                .BySettingApplicationApproval()
                .AndAllPreviousSteps())
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that configuring a composite event with 'AndAllOtherSteps' will execute events prior to and after the configured events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_CompositeEventConfiguredWithAndAllOtherSteps_EventsPriorToAndAfterConfiguredEventsWillExecute()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication(a => a
                .ByAssigningTheApplication()
                .AndAllOtherSteps())
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that configuring an event builder will map the configured fields to the event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_EventBuilderConfigured_MapsBuilderFieldsToEvent()
    {
        var scenario = await this.builder
            .CaseworkerProcessesApplication(a => a
                .BySettingApplicationApproval(b => b
                    .WithApproval(false))
                .AndAllPreviousSteps())
            .BuildAsync();

        scenario.ApplicationProcessing_ApplicationApproval?.Approved.Should().BeFalse();
    }

    /// <summary>
    /// Tests that multiple calls to <see cref="Scenario.Builder{TScenario}.BuildAsync(TScenario)"/> will generate multiple different scenarios.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_MultipleCalls_GeneratesMultipleScenarios()
    {
        var scenarioBuilder = this.builder.CaseworkerProcessesApplication();

        var scenario1 = await scenarioBuilder.BuildAsync();
        var scenario2 = await scenarioBuilder.BuildAsync();

        scenario1.ApplicationSubmission?.SubmittedApplicationId.Should().NotBe(scenario2.ApplicationSubmission?.SubmittedApplicationId.ToString());
    }

    /// <summary>
    /// Tests that configuring an event builder will map the configured fields to the event.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_ScenarioProvided_ExtendsExistingScenario()
    {
        var scenarioBuilder = this.builder.CaseworkerProcessesApplication();

        var scenario1 = await scenarioBuilder.BuildAsync();
        var scenario2 = await scenarioBuilder.BuildAsync(scenario1);

        scenario1.Should().Be(scenario2);
    }

    /// <summary>
    /// Tests that configuring no events on the scenario builder will run all events.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TestMethod]
    public async Task BuildAsync_NoEventsConfigured_RunsAllEvents()
    {
        var scenario = await this.builder.BuildAsync();

        scenario.ApplicationSubmission.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationAssignment.Should().NotBeNull();
        scenario.ApplicationProcessing_ApplicationApproval.Should().NotBeNull();
    }

    private void ConfigureMockDefaults()
    {
        this.mockServiceClientFactory.SetReturnsDefault(this.mockServiceClient.Object);
        this.mockServiceClient
            .Setup(c => c.ExecuteAsync(It.IsAny<WhoAmIRequest>()))
            .Returns(Task.FromResult((OrganizationResponse)new WhoAmIResponse { Results = { { "UserId", Guid.NewGuid() } } }));
        this.mockServiceClient
            .Setup(c => c.CreateAsync(It.IsAny<Entity>()))
            .Returns(() => Task.FromResult(Guid.NewGuid()));
    }
}