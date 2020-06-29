using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForceInspectOnline.WebService.ColumboModules.Qal.QalGenericModels
{
  public class GenericValueRow
  {
    public Guid GenericValueRowKey { get; set; }

    public Guid GenericConfigTableKey { get; set; }

    public int ItemId { get; set; }

    public Guid ItemKey { get; set; }

    public int VersionId { get; set; }

    public DateTimeOffset LastModifiedDate { get; set; }

    public Guid LastModifiedBy { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
  }
}