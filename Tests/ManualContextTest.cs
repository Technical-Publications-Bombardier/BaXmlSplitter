using System.Security.Cryptography;
using System.Text;
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

    // Arrange
    private static readonly DbContextOptions<ManualContext> Options = new DbContextOptionsBuilder<ManualContext>()
        .UseInMemoryDatabase("TestDatabase") // Use in-memory database for testing
        .Options;

    [Fact]
    public void ConnectionStringTest()
    {
        Assert.False(string.IsNullOrEmpty(BaOraConnectionString));
        Assert.Equal("BBB59842DBE8ABBB1F0B2C249C4D1EBC",Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(BaOraConnectionString))));
    }
    [Fact]
    public async Task AnchorTableTest()
    {
        // Insert seed data into the database using one instance of the context
        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
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
        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
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

    [Fact]
    public async Task DocumentTableTest()
    {
        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.Document?.Add(new Document
            {
                DocumentRef = 1,
                ObjectPath = "Test",
                ObjectRef = 1
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.Document != null)
            {
                var firstDocument = await context.Document.FirstAsync();
                Assert.NotNull(firstDocument);
                Assert.Equal(1, firstDocument.DocumentRef);
                Assert.Equal("Test", firstDocument.ObjectPath);
                Assert.Equal(1, firstDocument.ObjectRef);
            }
        }
    }

    [Fact]
    public async Task ObjectAttributeTableTest()
    {
        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.ObjectAttribute?.Add(new ObjectAttribute
            {
                ObjectRef = 1,
                AttributeName = "TestAttribute",
                AttributeValue = "TestValue"
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.ObjectAttribute != null)
            {
                var firstObjectAttribute = await context.ObjectAttribute.FirstAsync();
                Assert.NotNull(firstObjectAttribute);
                Assert.Equal(1, firstObjectAttribute.ObjectRef);
                Assert.Equal("TestAttribute", firstObjectAttribute.AttributeName);
                Assert.Equal("TestValue", firstObjectAttribute.AttributeValue);
            }
        }
    }

    [Fact]
    public async Task ObjectNewTableTest()
    {
        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            context.ObjectNew?.Add(new ObjectNew
            {
                ObjectRef = 1,
                ObjectId = 1,
                CreateTime = DateTime.Now,
                UserRef = 1,
                ModifyTime = DateTime.Now,
                ValidTime = DateTime.Now,
                ObsoleteTime = DateTime.Now,
                OfflineTime = DateTime.Now,
                ObjectType = 1
            });
            await context.SaveChangesAsync();
        }

        await using (var context = new ManualContext(Options, CsdbContext.CsdbProgram.B_IFM, BaOraConnectionString!))
        {
            if (context.ObjectNew != null)
            {
                var firstObjectNew = await context.ObjectNew.FirstAsync();
                Assert.NotNull(firstObjectNew);
                Assert.Equal(1, firstObjectNew.ObjectRef);
                Assert.Equal(1, firstObjectNew.ObjectId);
                // Add additional assertions for other properties as needed
            }
        }
    }
}