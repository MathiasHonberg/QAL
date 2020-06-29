using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Position
  {
    public int ID { get; set; }

    public string Pos { get; set; }

    public int ParentID { get; set; }

    public int WPID { get; set; }

    public string Description { get; set; }

    public int ListIndex { get; set; }

    public string Remarks { get; set; }

    public string WPSubId { get; set; }

    //public Guid WorkItemKey { get; set; }

  }
}
