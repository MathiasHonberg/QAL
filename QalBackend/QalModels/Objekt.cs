using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForceInspectOnline.WebService.ColumboModules.Qal
{
  public class Objekt
  {
    #region Properties
    public int ObjectID { get; set; }

    public string ExternalNo { get; set; }

    public int ParentObjectID { get; set; }

    public string ExternalDescription { get; set; }

    public string InternalDescription { get; set; }
    #endregion Properties
  }
}