using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ForceInspectOnline.WebService.ColumboModules.Qal.QalGenericModels
{
  public class GenericValueField
  {
    public Guid GenericValueFieldKey { get; set; }

    public Guid GenericValueRowKey { get; set; }

    public Guid GenericConfigFieldKey { get; set; }

    public Guid GenericConfigLookupFieldKey { get; set; }

    public bool ValueBit { get; set; }

    public DateTime ValueDateTimeOffset { get; set; }

    public decimal ValueDecimal { get; set; }

    public int ValueInt { get; set; }

    public string ValueNvarchar { get; set; }

    public string OriginalValue { get; set; }

    public bool Readonly { get; set; }

    public DateTimeOffset LastModifiedDate { get; set; }

    public Guid LastModifiedBy { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
  }
}