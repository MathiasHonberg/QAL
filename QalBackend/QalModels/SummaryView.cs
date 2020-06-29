using System;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class SummaryView
  {
    #region Properties
    public string ParentName { get; set; }

    public string Komponent { get; set; }

    /// <summary>Gets/Sets the KKS number.</summary>
    /// <remarks>
    /// KKS = KraftwerkKennzeichenSystem.
    /// A identification System for Power Plants.
    /// </remarks>
    /// <see href="https://www.vgb.org/en/aufbau_anwendung_kraftwerk_kennzeichensystem.html"/>
    /// <seealso href="https://richardspowerpage.com/wp-content/uploads/2018/11/KKS-guide-book.pdf"/>
    public string KKS { get; set; }

    public string Enhed { get; set; }

    public double? Maaleomraade { get; set; }

    public float? SVaerdi { get; set; }

    public int? GraenseVaerdi { get; set; }

    public DateTime? SenesteQAL3 { get; set; }

    public string Test { get; set; }

    public float? SenesteNAfv { get; set; }

    public float? SenesteSAfv { get; set; }

    public float? SkaeringMedY { get; set; }

    public float? Slope { get; set; }

    public int? Order { get; set; }
    #endregion Properties
  }
}