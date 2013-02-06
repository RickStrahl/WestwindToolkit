using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using Westwind.Data.EfCodeFirst;
using System.Data.Entity;
using Westwind.Utilities;

namespace Westwind.Data.Test
{
    public class busCustomer : EfCodeFirstBusinessBase<Customer, WebStoreContext>
    {
        public busCustomer()
        { }

        public busCustomer(IBusinessObject<WebStoreContext> parentBusinessObject)
            : base(parentBusinessObject)
        { }
    }
}
