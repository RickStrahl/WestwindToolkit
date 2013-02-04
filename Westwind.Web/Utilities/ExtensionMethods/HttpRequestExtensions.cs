using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Westwind.Web
{
    /// <summary>
    /// HttpRequest Extension methods to facilitate various input retrieval tasks tasks
    /// </summary>
    public static class HttpRequestBaseExtensions
    {
        /// <summary>
        /// Determines whether a form variable exists
        /// </summary>
        /// <param name="request"></param>
        /// <param name="formVarName"></param>
        /// <returns></returns>
        public static bool IsFormVar(this HttpRequestBase request, string formVarName)
        {
            var formVar = request.Form[formVarName];
            if ( string.IsNullOrEmpty(formVar) )
                return false;
                    
            return true;
        }

        /// <summary>
        /// Returns a value via Params[] and attempts
        /// to convert it to an integer.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="FormVarName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ParamsInt(this HttpRequestBase request, string formVarName, int defaultValue = -1)
        {
            var formVar = request.Params[formVarName];
            if (string.IsNullOrEmpty(formVar))
                return defaultValue;

            int result = -1;
            if (!int.TryParse(formVar, out result))
                return defaultValue;

            return result;
        }

        /// <summary>
        /// Returns a value via Params[] and attempts
        /// to convert it to a decimal value.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="FormVarName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal ParamsDecimal(this HttpRequestBase request, string formVarName, decimal defaultValue = -1M)
        {
            var formVar = request.Params[formVarName];
            if (string.IsNullOrEmpty(formVar))
                return defaultValue;

            decimal result = -1M;
            if (!decimal.TryParse(formVar, out result))
                return defaultValue;

            return result;
        }

    }
}
