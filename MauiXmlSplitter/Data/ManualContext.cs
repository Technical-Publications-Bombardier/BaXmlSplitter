using Microsoft.EntityFrameworkCore;

namespace MauiXmlSplitter.Data;

public class ManualContext(CsdbContext.CsdbProgram program, string baOraConnectionString) : DbContext
{
    private static readonly Dictionary<CsdbContext.CsdbProgram, string> ServiceNameTable = new()
    {
        { CsdbContext.CsdbProgram.B_IFM, "BIFM" },
        { CsdbContext.CsdbProgram.CTALPROD, "CTALPROD" },
        { CsdbContext.CsdbProgram.CH604PROD, "CH604PRD" },
        { CsdbContext.CsdbProgram.LJ4045PROD, "LJ4045P" },
        { CsdbContext.CsdbProgram.GXPROD, "GXPROD" }
    };

    private readonly string serviceName = ServiceNameTable[program];

    // DbSet for the ANCHOR table
    public DbSet<Anchor> Anchor { get; set; }

    // DbSet for the OBJECTATTRIBUTE table
    public DbSet<ObjectAttribute> ObjectAttribute { get; set; }

    // DbSet for the OBJECTNEW table
    public DbSet<ObjectNew> ObjectNew { get; set; }

    // DbSet for the DOCUMENT table
    public DbSet<Document> Document { get; set; }
    public DbSet<InnerSubQueryForKeys> InnerSubQueryForKeys { get; set; }

    public DbSet<SubQueryForKeys> SubQueryForKeys { get; set; }
    public DbSet<ManualsPerKey> ManualsPerKey { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            _ = optionsBuilder.UseOracle($"{baOraConnectionString}/{serviceName}");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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