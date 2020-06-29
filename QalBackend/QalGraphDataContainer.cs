using System;
using System.Collections.Generic;
using Columbo.DataModel.Utility;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class QalGraphDataContainer
  {
    #region Properties
    public string Unit { get; set; }

    public string SZero { get; set; }

    public string SSpan { get; set; }

    List<Trace<DateTime, decimal>> traces = new List<Trace<DateTime, decimal>>();
    public List<Trace<DateTime, decimal>> TraceCollection
    {
      get { return traces; }
      set { traces = value; }
    }
    #endregion Properties
  }
}