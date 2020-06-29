using System;
using System.Collections.Generic;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class Resume
  {
    #region Properties
    public List<string> ParentName { get; set; }

    public List<string> Komponent { get; set; }

    /// <summary>Gets/Sets the KKS number.</summary>
    /// <remarks>
    /// KKS = KraftwerkKennzeichenSystem.
    /// A identification System for Power Plants.
    /// </remarks>
    /// <see href="https://www.vgb.org/en/aufbau_anwendung_kraftwerk_kennzeichensystem.html"/>
    /// <seealso href="https://richardspowerpage.com/wp-content/uploads/2018/11/KKS-guide-book.pdf"/>
    public List<string> KKS { get; set; }

    public List<string> Enhed { get; set; }

    public List<double?> Måleområde { get; set; }

    public List<float?> SVærdi { get; set; }

    public List<int?> GrænseVærdi { get; set; }

    public List<DateTime?> SenesteQAL3 { get; set; }

    public string Test { get; set; }

    public List<float?> SenesteNAfv { get; set; }

    public List<float?> SenesteSAfv { get; set; }

    public List<float?> SkæringMedY { get; set; }

    public List<float?> Slope { get; set; }

    public List<int?> Order { get; set; }
    #endregion Properties
  }
}