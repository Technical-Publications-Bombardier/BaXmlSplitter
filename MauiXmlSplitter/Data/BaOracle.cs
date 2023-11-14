using MauiXmlSplitter.Models;
using Microsoft.EntityFrameworkCore;

namespace BaXmlSplitter;

internal class ManualContext(CsdbContext.CsdbProgram program, string baOraConnectionString) : DbContext
{
    private readonly string serviceName = ServiceNameTable[program];

    private static readonly Dictionary<CsdbContext.CsdbProgram, string> ServiceNameTable = new()
        {
            { CsdbContext.CsdbProgram.B_IFM, "BIFM" },
            { CsdbContext.CsdbProgram.CTALPROD, "CTALPROD" },
            { CsdbContext.CsdbProgram.CH604PROD, "CH604PRD" },
            { CsdbContext.CsdbProgram.LJ4045PROD, "LJ4045P" },
            { CsdbContext.CsdbProgram.GXPROD, "GXPROD" }
        };

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseOracle($"{baOraConnectionString}/{serviceName}");
        base.OnConfiguring(optionsBuilder);
    }
}