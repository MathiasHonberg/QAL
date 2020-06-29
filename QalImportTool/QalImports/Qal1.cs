using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class Qal1
  {
    public int Q1_ID { get; set; }

    public int Q1_MID { get; set; }

    public DateTime Q1_Dato { get; set; }

    public int Q1_IID { get; set; }

    public string Q1_Description { get; set; }

    public string Q1_CID { get; set; }

    public string Q1_IVersion { get; set; }

    public bool Q1_AIA { get; set; }
    
    public decimal Q1_SV { get; set; }

    public decimal Q1_MV { get; set; }

    public string Q1_Unit { get; set; }

    public decimal Q1_SZero { get; set; }

    public decimal Q1_SSpan { get; set; }

    public string Q1_INI { get; set; }

    public int Q1_UID { get; set; }

    public bool Q1_Accept { get; set; }

    //Property in 'Reports' aswell
    public string Q1_DocName { get; set; }

    //Property in 'Reports' aswell
    public int Q1_DocLength { get; set; }

    //Property in 'Reports' aswell
    public string Q1_DocContentType { get; set; }
  }
}
