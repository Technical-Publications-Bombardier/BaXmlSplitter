using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TechPubsDatabase.Models;

namespace TechPubsDatabase.Data;

/// <summary>
///     Flight and maintenance manuals' db context.
/// </summary>
/// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
public partial class ManualContext(
    DbContextOptions<ManualContext> options,
    CsdbContext.CsdbProgram program,
    string baOraConnectionString) : DbContext(options)
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

    /// <summary>
    ///     The service name
    /// </summary>
    private readonly string serviceName = ServiceNameTable[program];

    public ManualContext(CsdbContext.CsdbProgram program, string baOraConnectionString) : this(
        new DbContextOptions<ManualContext>(), program, baOraConnectionString)
    {
    }

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
    ///     Gets or sets the column from the ObjectAttribute table.
    /// </summary>
    /// <value>
    ///     The object attribute.
    /// </value>
    public DbSet<ObjectAttribute>? ObjectAttribute { get; set; }

    // DbSet for the OBJECTNEW table
    /// <summary>
    ///     Gets or sets the column from ObjectNew table.
    /// </summary>
    /// <value>
    ///     The object new.
    /// </value>
    public DbSet<ObjectNew>? ObjectNew { get; set; }

    // DbSet for the DOCUMENT table
    /// <summary>
    ///     Gets or sets the column from the Document table.
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
    ///     Gets or sets the sub-query for keys.
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

    public DbSet<ManualMetadata>? ManualsMetadata { get; set; }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            _ = optionsBuilder.UseOracle($"{baOraConnectionString}/{serviceName}");
        optionsBuilder.LogTo(Console.WriteLine); // TODO: remove in production
        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    ///     Revision date regular expression
    /// </summary>
    /// <returns><see cref="Regex" /> for the revision date.</returns>
    [GeneratedRegex(@"(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})")]
    private static partial Regex RevDateRe();

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

        modelBuilder.Entity<Anchor>().ToTable("ANCHOR").HasKey(a=>a.AnchorRef);
        modelBuilder.Entity<State>().ToTable("FINITESTATEMACHINESTATE").HasKey(s => s.StateValue);
        modelBuilder.Entity<ObjectAttribute>().ToTable("OBJECTATTRIBUTE").HasKey(oa=>oa.ObjectRef);
        modelBuilder.Entity<ObjectNew>().ToTable("OBJECTNEW");
        modelBuilder.Entity<Document>().ToTable("DOCUMENT").HasKey(d=>d.DocumentRef);
        modelBuilder.Entity<InnerSubQueryForKeys>().HasNoKey().ToView(null);
        modelBuilder.Entity<SubQueryForKeys>().HasNoKey().ToView(null);
        modelBuilder.Entity<ManualsPerKey>().HasNoKey().ToView(null);
        modelBuilder.Entity<ManualMetadata>().HasNoKey().ToView(null);
        modelBuilder.Entity<ObjectNew>()
            .HasMany(on => on.ObjectAttributes)
            .WithOne(oa => oa.ObjectNew)
            .HasForeignKey(oa => oa.ObjectRef);
        modelBuilder.Entity<ObjectNew>()
            .HasOne(on => on.State)
            .WithMany()
            .HasForeignKey(on => on.CurrentState).IsRequired(false);
        modelBuilder.Entity<ManualMetadata>().Property(m => m.RevDate).HasConversion(
            v => v.ToString("yyyyMMdd"), v => DateOnly.Parse(RevDateRe().Replace(v, "${year}-${month}-${day}"))
        );
        modelBuilder.Entity<ManualMetadata>().Property(m => m.ValidTime).HasConversion(
            v => v.ToString("yyyy-MM-dd HH:mm:ss"),
            v => DateTimeOffset.Parse(v)
        );
    }

    public async Task<IEnumerable<ManualMetadata?>> GetManualMetaDataAsync(IEnumerable<string> docnbrs)
    {
        var query = $"""
                     with ranked_subq as (
                     	select subq.name,
                     	       subq.manual,
                     	       subq.docnbr,
                     	       subq.cus,
                     	       subq.tsn,
                     	       fsmn.finitestatemachinename as state,
                     	       subq.revdate,
                     	       subq.validtime,
                     	       subq.objectref,
                     	       subq.objectid,
                     	       subq.parentobjectid,
                     	       p.objectref as objp,
                     	       row_number()
                     	       over(partition by subq.name,subq.manual,subq.docnbr,subq.cus,subq.tsn,fsmn.finitestatemachinename,subq.revdate,subq.validtime
                     	       ,subq.objectref,subq.objectid,subq.parentobjectid
                     	            order by p.objectref desc
                     	       ) as rn
                     	  from (
                     		select o.objectname as name,
                     		       manual.attributevalue as manual,
                     		       docnbr.attributevalue as docnbr,
                     		       cus.attributevalue as cus,
                     		       tsn.attributevalue as tsn,
                     		       revdate.attributevalue as revdate,
                     		       to_char(
                     			       o.validtime,'YYYY-MM-DD HH24:MI:SS'
                     		       ) as validtime,
                     		       o.objectref,
                     		       o.objectid,
                     		       o.parentobjectid
                     		  from objectnew o
                     		  left join objectattribute manual
                     		on o.objectref = manual.objectref
                     		   and manual.attributename = 'MANUAL',
                     		       objectattribute docnbr,
                     		       objectattribute cus,
                     		       objectattribute revdate,
                     		       objectattribute tsn
                     		 where docnbr.attributevalue in ({string.Join(", ", docnbrs.Select(d => $"'{d}'"))})
                     		   and o.objectref = docnbr.objectref
                     		   and tsn.objectref = o.objectref
                     		   and cus.objectref = o.objectref
                     		   and revdate.objectref = o.objectref
                     		   and docnbr.attributename = 'DOCNBR'
                     		   and cus.attributename = 'CUS'
                     		   and tsn.attributename = 'TSN'
                     		   and revdate.attributename = 'REVDATE'
                     		   and o.obsoletetime > sysdate
                     		 order by o.validtime desc
                     	) subq,
                     	       objectnew p,
                     	       finitestatemachinename fsmn
                     	 where p.finitestatemachineref = fsmn.finitestatemachineref
                     	   and p.objectid = subq.parentobjectid
                     )
                     select name,
                            manual,
                            docnbr,
                            cus,
                            tsn,
                            state,
                            revdate,
                            validtime,
                            objectref,
                            objectid,
                            parentobjectid,
                            objp
                       from ranked_subq
                      where rn = 1;
                     """.Replace("'MANUAL'",
            program != CsdbContext.CsdbProgram.B_IFM
                ? "'MANUALTYPE'" /* In maintenance manual DB's, we use 'MANUALTYPE' just to mix it up for no good reason */
                : "'MANUAL'", StringComparison.Ordinal);
        Debug.Assert(ManualsMetadata != null, nameof(ManualsMetadata) + " != null");
        return await ManualsMetadata.FromSqlRaw(query).ToListAsync();
    }
}