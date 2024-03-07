namespace ScenarioBuilder.UnitTests;

using Bogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the <see cref="ScenarioContext"/> class.
/// </summary>
[TestClass]
public class ScenarioContextTests
{
    private readonly Faker faker;
    private readonly ScenarioContext scenarioContext;
    private readonly string key;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioContextTests"/> class.
    /// </summary>
    public ScenarioContextTests()
    {
        this.faker = new Faker();
        this.scenarioContext = new ScenarioContext();
        this.key = this.faker.Random.String();
    }

    /// <summary>
    /// Tests that a null value can be set.
    /// </summary>
    [TestMethod]
    public void Set_NullValue_SetsValue()
    {
        this.scenarioContext.Set(this.key, null);

        this.scenarioContext.Get<object>(this.key).Should().BeNull();
    }

    /// <summary>
    /// Tests that a non-null value can be set.
    /// </summary>
    [TestMethod]
    public void Set_NonNullValue_SetsValue()
    {
        this.scenarioContext.Set(this.key, null);

        this.scenarioContext.Get<object>(this.key).Should().BeNull();
    }

    /// <summary>
    /// Tests that a value cannot be set with a null key.
    /// </summary>
    [TestMethod]
    public void Set_NullKey_Throws()
    {
        this.scenarioContext.Invoking(c => c.Set(null, new object())).Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that a key that hasn't been set throws a <see cref="KeyNotFoundException"/>.
    /// </summary>
    [TestMethod]
    public void Get_KeyNotFound_Throws()
    {
        this.scenarioContext.Invoking(c => c.Get<object>(this.faker.Random.String())).Should().Throw<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that a value will be returned by key.
    /// </summary>
    [TestMethod]
    public void Get_KeyFound_ReturnsValue()
    {
        var value = new object();
        this.scenarioContext.Set(this.key, value);

        this.scenarioContext.Get<object>(this.key).Should().Be(value);
    }
}