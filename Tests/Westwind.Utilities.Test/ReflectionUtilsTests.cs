using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Westwind.Utilities.Test
{
    [TestClass]
    public class ReflectionUtilsTests
    {
        [TestMethod]
        public void TypedValueToStringTest()
        {
            // Guid
            object val = Guid.NewGuid();            
            string res = ReflectionUtils.TypedValueToString(val);

            Assert.IsTrue(res.Contains("-"));
            Console.WriteLine(res);

            object val2 = ReflectionUtils.StringToTypedValue<Guid>(res);
            Assert.AreEqual(val, val2);

            // Single 
            val = (Single) 10.342F;            
            res = ReflectionUtils.TypedValueToString(val);
            Console.WriteLine(res);

            Assert.AreEqual(res, val.ToString());

            val2 = ReflectionUtils.StringToTypedValue<Single>(res);
            Assert.AreEqual(val, val2);

            // Single 
            val = (Single)10.342F;
            res = ReflectionUtils.TypedValueToString(val);
            Console.WriteLine(res);

            Assert.AreEqual(res, val.ToString());

            val2 = ReflectionUtils.StringToTypedValue<Single>(res);
            Assert.AreEqual(val, val2);
        }
    }
}
