namespace ScenarioBuilder.UnitTests;

using ScenarioBuilder;
using ScenarioBuilder.TestImplementation.Events.PortalUser;

/// <summary>
/// Tests for the <see cref="ComposeUsingAttribute"/> class.
/// </summary>
[TestClass]
public class ComposeUsingAttributeTests
{
    private readonly Faker faker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComposeUsingAttributeTests"/> class.
    /// </summary>
    public ComposeUsingAttributeTests()
    {
        this.faker = new Faker();
    }

    /// <summary>
    /// Tests that a null event type will throw an <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullType_ThrowsArgumentNullException()
    {
        var constructingWithNullEventType = () => new ComposeUsingAttribute(this.faker.Random.Int(min: 0), this.faker.Random.Word(), null);

        constructingWithNullEventType.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that a null event type will throw an <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void Constructor_NullEventId_ThrowsArgumentArgumentException()
    {
        var constructingWithNullEventType = () => new ComposeUsingAttribute(this.faker.Random.Int(min: 0), null, typeof(PortalUserSubmitsApplicationEvent));

        constructingWithNullEventType.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that constructing with a valid child event type and order will create an instance.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidOrderEventIdAndChildEventType_CreatesInstance()
    {
        var attribute = new ComposeUsingAttribute(this.faker.Random.Int(), this.faker.Random.Word(), typeof(PortalUserSubmitsApplicationEvent));

        attribute.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the order passed to the constructor is accessible via the <see cref="ComposeUsingAttribute.Order"/> property.
    /// </summary>
    [TestMethod]
    public void Order_ValidOrderEventIdAndChildEventType_ReturnsOrder()
    {
        var order = this.faker.Random.Int();

        var attribute = new ComposeUsingAttribute(order, this.faker.Random.Word(), typeof(PortalUserSubmitsApplicationEvent));

        attribute.Order.Should().Be(order);
    }

    /// <summary>
    /// Tests that the type passed to the constructor is accessible via the <see cref="ComposeUsingAttribute.ChildEventType"/> property.
    /// </summary>
    [TestMethod]
    public void ChildEventType_ValidOrderEventIdAndChildEventType_ReturnsType()
    {
        var type = typeof(PortalUserSubmitsApplicationEvent);

        var attribute = new ComposeUsingAttribute(this.faker.Random.Int(), this.faker.Random.Word(), type);

        attribute.ChildEventType.Should().Be(type);
    }

    /// <summary>
    /// Tests that the type passed to the constructor is accessible via the <see cref="ComposeUsingAttribute.ChildEventType"/> property.
    /// </summary>
    [TestMethod]
    public void ChildEventType_ValidOrderEventIdAndChildEventType_ReturnsEventId()
    {
        var eventId = this.faker.Random.Word();

        var attribute = new ComposeUsingAttribute(this.faker.Random.Int(), eventId, typeof(PortalUserSubmitsApplicationEvent));

        attribute.EventId.Should().Be(eventId);
    }
}