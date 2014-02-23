using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using Westwind.Data.EfCodeFirst;
using System.Data.Entity;
using Westwind.Utilities;

namespace Westwind.Data.Test
{
    public class busOrder : EfCodeFirstBusinessBase<Order, WebStoreContext>
    {
        public busOrder()
        { }

        public busOrder(IBusinessObject<WebStoreContext> parentBusinessObject)
            : base(parentBusinessObject)
        { }
    }
}
