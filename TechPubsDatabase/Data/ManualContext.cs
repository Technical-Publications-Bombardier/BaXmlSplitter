using Microsoft.EntityFrameworkCore;
using TechPubsDatabase.Models;

namespace TechPubsDatabase.Data;

/// <summary>
///     Flight and maintenance manuals' db context.
/// </summary>
/// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
public class ManualContext(DbContextOptions<ManualContext> options, CsdbContext.CsdbProgram program, string baOraConnectionString) : DbContext(options)
{
    /// <summary>
    ///     The service name table
    /// </summary>
    private static readonly Dictionary<CsdbContext.CsdbProgram, string> ServiceNameTable = new()
    {
        { CsdbContext.CsdbProgram.B_IFM, "BIFM" },
        { CsdbContext.CsdbProgram.CTALPROD, "CTALPROD" },
        { CsdbContext.CsdbProgram.CH604PROD, "CH604PRD" },
        { CsdbContext.CsdbProgram.LJ4045PROD, "LJ4045P" },
        { CsdbContext.CsdbProgram.GXPROD, "GXPROD" }
    };

    public ManualContext(CsdbContext.CsdbProgram program, string baOraConnectionString) : this(new DbContextOptions<ManualContext>(), program, baOraConnectionString)
    {
    }
    /// <summary>
    ///     The service name
    /// </summary>
    private readonly string serviceName = ServiceNameTable[program];

    // DbSet for the ANCHOR table
    /// <summary>
    ///     Gets or sets the anchor.
    /// </summary>
    /// <value>
    ///     The anchor.
    /// </value>
    public DbSet<Anchor>? Anchor { get; set; }

    // DbSet for the OBJECTATTRIBUTE table
    /// <summary>
    ///     Gets or sets the object attribute.
    /// </summary>
    /// <value>
    ///     The object attribute.
    /// </value>
    public DbSet<ObjectAttribute>? ObjectAttribute { get; set; }

    // DbSet for the OBJECTNEW table
    /// <summary>
    ///     Gets or sets the object new.
    /// </summary>
    /// <value>
    ///     The object new.
    /// </value>
    public DbSet<ObjectNew>? ObjectNew { get; set; }

    // DbSet for the DOCUMENT table
    /// <summary>
    ///     Gets or sets the document.
    /// </summary>
    /// <value>
    ///     The document.
    /// </value>
    public DbSet<Document>? Document { get; set; }

    /// <summary>
    ///     Gets or sets the inner sub query for keys.
    /// </summary>
    /// <value>
    ///     The inner sub query for keys.
    /// </value>
    public DbSet<InnerSubQueryForKeys>? InnerSubQueryForKeys { get; set; }

    /// <summary>
    ///     Gets or sets the sub query for keys.
    /// </summary>
    /// <value>
    ///     The sub query for keys.
    /// </value>
    public DbSet<SubQueryForKeys>? SubQueryForKeys { get; set; }

    /// <summary>
    ///     Gets or sets the manuals per key.
    /// </summary>
    /// <value>
    ///     The manuals per key.
    /// </value>
    public DbSet<ManualsPerKey>? ManualsPerKey { get; set; }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            _ = optionsBuilder.UseOracle($"{baOraConnectionString}/{serviceName}");
        optionsBuilder.LogTo(Console.WriteLine); // TODO: remove in production
        base.OnConfiguring(optionsBuilder);
    }

    //
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entityType.GetProperties())
                property.SetColumnName(property.GetColumnName().ToUpper());
        modelBuilder.Entity<Anchor>()
            .HasKey(a => a.AnchorRef);
        modelBuilder.Entity<ObjectAttribute>()
            .HasKey(oa => oa.ObjectRef);

        modelBuilder.Entity<ObjectNew>()
            .HasKey(on => on.ObjectRef);

        modelBuilder.Entity<Document>()
            .HasKey(d => d.DocumentRef);

        modelBuilder.Entity<Anchor>().ToTable("ANCHOR");
        modelBuilder.Entity<ObjectAttribute>().ToTable("OBJECTATTRIBUTE");
        modelBuilder.Entity<ObjectNew>().ToTable("OBJECTNEW");
        modelBuilder.Entity<Document>().ToTable("DOCUMENT");
        modelBuilder.Entity<InnerSubQueryForKeys>().HasNoKey().ToView(null);
        modelBuilder.Entity<SubQueryForKeys>().HasNoKey().ToView(null);
        modelBuilder.Entity<ManualsPerKey>().HasNoKey().ToView(null);
        modelBuilder.Entity<ObjectNew>()
            .HasMany(on => on.ObjectAttributes)
            .WithOne(oa => oa.ObjectNew)
            .HasForeignKey(oa => oa.ObjectRef);
    }
}