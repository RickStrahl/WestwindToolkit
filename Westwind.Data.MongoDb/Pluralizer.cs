using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Data.MongoDb
{

    /// <summary>
    /// Overly simplistic pluralizer that is used
    /// to pluralize words by appending an s to the
    /// name and add additional words via dictionary
    /// overrides
    /// </summary>
    public class Pluralizer
    {
        public static Dictionary<string, string> Exceptions =
        new Dictionary<string,string> {
            {"Quiz", "Quizzes"},            
            {"Mouse", "Mice"},
            {"Man", "Men"}            
        };


        public static string Pluralize(string noun)
        {
            if (Exceptions.Keys.Contains(noun))
                return Exceptions[noun];

            if (noun.EndsWith("y"))
                return noun.Substring(0, noun.Length - 1) + "ies";
            
            return noun + "s";
        }
    }
}
