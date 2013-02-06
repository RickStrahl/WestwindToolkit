using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using Westwind.Data.EfCodeFirst;
using System.Data.Entity;
using Westwind.Utilities;

namespace Westwind.Data.Test
{
    public class busLineItem : EfCodeFirstBusinessBase<Order, WebStoreContext>
    {
        public busLineItem()
        { }

        public busLineItem(IBusinessObject<WebStoreContext> parentBusinessObject)
            : base(parentBusinessObject)
        { }
    }
}
