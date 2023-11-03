using System.Diagnostics.CodeAnalysis;

namespace MauiXmlSplitter.Models
{
    public static class CsdbContext
    {

        /// <summary>The CSDB programs.</summary>
        internal static readonly string[] Programs = Enum.GetNames<CsdbProgram>();
        /// <summary>
        /// The CSDB CsdbProgram.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum CsdbProgram
        {
            /// <summary>
            /// The <c>B_IFM</c> program for instrument flight manuals
            /// </summary>
            B_IFM,
            /// <summary>
            /// The <c>CH604PROD</c> program for Challenger 6XX maintenance manuals
            /// </summary>
            CH604PROD,
            /// <summary>
            /// The <c>CTALPROD</c> program for Challenger 3XX maintenance manuals
            /// </summary>
            CTALPROD,
            /// <summary>
            /// The <c>GXPROD</c> program for Global and Global Express maintenance manuals
            /// </summary>
            GXPROD,
            /// <summary>
            /// The <c>LJ4045PROD</c> program for Learjet 40/45 maintenance manuals
            /// </summary>
            LJ4045PROD
        };
    }
}