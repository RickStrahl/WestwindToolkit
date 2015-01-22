using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Data.Test.Models;
using Westwind.Data.EfCodeFirst;
using System.Data.Entity;
using System.Linq;
using Westwind.Utilities;

namespace Westwind.Data.Test
{
    public class busCustomer : EfCodeFirstBusinessBase<Customer, WebStoreContext>
    {
        public busCustomer()
        { }

        public busCustomer(string connectionString) : base(connectionString)
        { }

        public busCustomer(IBusinessObject<WebStoreContext> parentBusinessObject)
            : base(parentBusinessObject)
        { }

        /// <summary>
        /// You typically have a number of query operations that are 
        /// stored in the business object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Customer> GetActiveCustomers()
        {
            DateTime dt = DateTime.UtcNow.AddYears(2);
            return Context.Customers.Where(cust => cust.Entered > DateTime.UtcNow.AddYears(-2));
        }

        public IEnumerable<Customer> GetCustomerWithoutOrders()
        {
            return Context.Customers
                .Where( cust=> !Context.Orders.Any(ord=> ord.CustomerPk == cust.Id));
        }

        /// <summary>
        /// You often implement utility methods that perform actions on 
        /// that are associated with the current bus object.
        /// 
        /// Not the best example - this is probably a better candidate
        /// for a app level utility function but used here to demonstrate
        /// </summary>
        /// <param name="plainPasswordText"></param>
        /// <returns></returns>
        public string EncodePassword(string plainPasswordText)
        {
            return Encryption.EncryptString(plainPasswordText, "seeekret1092") + "~~";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override bool OnBeforeSave(Customer entity)
        {
            // encode password if it isn't already
            if (!string.IsNullOrEmpty(entity.Password) && !entity.Password.EndsWith("~~"))
                entity.Password = EncodePassword(entity.Password);

            entity.Updated = DateTime.UtcNow;
            
            // true means save is allowed
            // return false to fail
            return true;
        }

        protected override void OnValidate(Customer entity)
        {
            // check if entity exists
            if (IsNewEntity(entity))
            {
                if (Context.Customers
                    .Any(c => c.LastName == entity.LastName &&
                                c.FirstName == entity.FirstName))
                {
                    ValidationErrors.Add("Customer already exists");
                    return;
                }
            }
            
            if (string.IsNullOrEmpty(entity.LastName))
                ValidationErrors.Add("Last name can't be empty");
            if (string.IsNullOrEmpty(entity.FirstName))
                ValidationErrors.Add("First name can't be empty");      
        }

    }
}
