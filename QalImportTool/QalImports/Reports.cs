using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Reports
  {
    public int Q_ID { get; set; }

    public string DocName { get; set; }

    public int DocLength { get; set; }

    public string DocContentType { get; set; }

    public byte[] DocContent { get; set; }
  }
}
