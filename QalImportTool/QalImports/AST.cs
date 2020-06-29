using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class AST
  {
    public int AST_ID { get; set; }

    public int AST_MID { get; set; }

    public DateTime AST_Dato { get; set; }

    public bool AST_Accept { get; set; }

    public string AST_INI { get; set; }

    public int AST_UID { get; set; }

    public string AST_DocName { get; set; }

    public int AST_DocLength { get; set; }

    public string AST_DocContentType { get; set; }
  }
}
