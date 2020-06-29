using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Inspection
  {
    public Guid InspectionGUID { get; set; }

    public int ObjectID { get; set; }

    public DateTime InspectionDateTime { get; set; }

    public DateTime LastModifiedDateTime { get; set; }

    public int JobId { get; set; }

    public string InternalComment { get; set; }

    public int InspectionTypeID { get; set; }
  }
}
