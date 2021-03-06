﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QalImports
{
  public class GenericConfigLookupField
  {
    public Guid GenericConfigLookupFieldKey { get; set; }

    public Guid GenericConfigLookupTableKey { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public Guid LastModifiedBy { get; set; }
  }
}
