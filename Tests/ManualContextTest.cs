using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechPubsDatabase.Data;
using TechPubsDatabase.Models;

namespace Tests;

public class ManualContextTest
{
    private static readonly IConfiguration Configuration =
        new ConfigurationBuilder().AddUserSecrets<ManualContextTest>().Build();

    private static readonly string? BaOraConnectionString = Configuration.GetConnectionString("BaOraConnectionString");

    [Fact]
    public async Task AnchorTableTest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ManualContext>()
            .UseInMemoryDatabase("TestDatabase") // Use in-memory database for testing
            .Options;

        // Insert seed data into the database using one instance of the context
        await using (var context = new ManualContext(options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.Anchor?.Add(new Anchor
            {
                AnchorRef = 1, ManualObjectRef = 1, SeqNo = 1, ObjectPath = "Test", ObjectRef = 1, Key = "Test",
                Chg = 'A',
                RevDate = "Test"
            });
            await context.SaveChangesAsync();
        }

        // Act
        // Use a clean instance of the context to run the test
        await using (var context = new ManualContext(options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.Anchor != null)
            {
                var firstAnchor = await context.Anchor.FirstAsync();

                // Assert
                Assert.NotNull(firstAnchor);
                Assert.Equal(1, firstAnchor.AnchorRef);
            }
        }
    }
}