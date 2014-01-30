using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.Data.MongoDb
{
    public class Pluralizer
    {
        public static Dictionary<string, string> Exceptions =
        new Dictionary<string,string> {
            {"Quiz", "Quizzes"},            
            {"Mouse", "Mice"}
        };


        public static string Pluralize(string noun)
        {
            if (Exceptions.Keys.Contains(noun))
                return Exceptions[noun];

            return noun + "s";
        }
    }
}
