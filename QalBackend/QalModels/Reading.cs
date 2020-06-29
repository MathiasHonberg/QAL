using System;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class Reading
  {
    #region Properties
    public int R_ID { get; set; }

    public int R_QAL_3 { get; set; }

    public DateTime R_Dato { get; set; }

    public double R_Zero { get; set; }

    public double R_Span { get; set; }

    public double R_SpanGas { get; set; }

    public string R_INI { get; set; }

    public int R_UID { get; set; }

    public DateTime R_SidstRettet { get; set; }

    public string R_Remarks { get; set; }

    public double R_ZeroGas { get; set; }

    public int m_M_ID { get; set; }

    public string R_Unit { get; set; }

    public double? SpanAdjust { get; set; }

    public double? ZeroAdjust { get; set; }

    public Status Status { get; set; }

    #endregion Properties
  }

  public enum Status
  {
    /// <summary></summary>
    NoDrift = 0,
    /// <summary>Zero Precision.</summary>
    ZeroPrecision = 1,
    /// <summary>Zero +Drift</summary>
    ZeroPositiveDriftAutomaticInternalAdjustment = 2,
    /// <summary>Zero -Drift</summary>
    ZeroNegativeDriftAutomaticInternalAdjustment = 3,
    /// <summary>Zero +Drift adjusted</summary>
    ZeroPositiveDriftNonAutomaticInternalAdjustment = 4,
    /// <summary>Zero -Drift adjusted</summary>
    ZeroNegativeDriftNonAutomaticInternalAdjustment = 5,
    /// <summary>Zero +Drift</summary>
    ZeroPositiveDriftNonAutomaticInternalAdjustmentFirstReading = 6,
    /// <summary>Zero -Drift</summary>
    ZeroNegativeDriftNonAutomaticInternalAdjustmentFirstReading = 7,
    /// <summary>Zero ?</summary>
    ZeroUnknownDrift = 8,
    /// <summary>Span Precision</summary>
    SpanPrecision = 9,
    /// <summary>Span +Drift</summary>
    SpanPositiveDriftAutomaticInternalAdjustment = 10,
    /// <summary>Span -Drift</summary>
    SpanNegativeDriftAutomaticInternalAdjustment = 11,
    /// <summary>Span +Drift adjustment</summary>
    SpanPositiveDriftNonAutomaticInternalAdjustment = 12,
    /// <summary>Span -Drift adjustment</summary>
    SpanNegativeDriftNonAutomaticInternalAdjustment = 13,
    /// <summary>Span +Drift</summary>
    SpanPositiveDriftNonAutomaticInternalAdjustmentFirstReading = 14,
    /// <summary>Span -Drift</summary>
    SpanNegativeDriftNonAutomaticInternalAdjustmentFirstReading = 15,
    /// <summary>Span ?</summary>
    SpanUnknownDrift = 16
  }
}