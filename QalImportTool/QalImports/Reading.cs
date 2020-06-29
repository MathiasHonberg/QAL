using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Reading
  {
    public int R_ID { get; set; }

    public int R_QAL3_ID { get; set; }

    public DateTime R_Dato { get; set; }

    public decimal R_Zero { get; set; }

    public decimal R_ZeroGas { get; set; }

    public decimal R_Span { get; set; }

    public decimal R_SpanGas { get; set; }

    public string R_INI { get; set; }

    public int R_UID { get; set; }

    public DateTime R_SidstRettet { get; set; }

    public string R_Remarks { get; set; }

  }
}
