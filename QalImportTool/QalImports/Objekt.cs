using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Objekt
  {
    public int ObjectID { get; set; }

    public string ExternalNo { get; set; }

    public int ParentObjectID { get; set; }

    public string ExternalDescription { get; set; }

    public string InternalDescription { get; set; }
  }
}
