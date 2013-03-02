#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 2011
 *          http://www.west-wind.com/
 * 
 * Created: 09/04/2008
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Web.JsonSerializers
{
    public interface IJSONSerializer
    {
        string Serialize(object value);
        object Deserialize(string jsonString, Type type);
        JsonDateEncodingModes DateSerializationMode { get; set; }
        bool FormatJsonOutput { get; set; }        
    }

    public class JSONSerializerBase
    {
       /// <summary>
        /// Master instance of the JSONSerializer that the user interacts with
        /// Used to read option properties
        /// </summary>
        protected JSONSerializer masterSerializer = null;


        /// <summary>
        /// Encodes Dates as a JSON string value that is compatible
        /// with MS AJAX and is safe for JSON validators. If false
        /// serializes dates as new Date() expression instead.
        /// 
        /// The default is true.
        /// </summary>
        public JsonDateEncodingModes DateSerializationMode
        {
            get { return masterSerializer.DateSerializationMode; }
            set { masterSerializer.DateSerializationMode = value; }
        }


        /// <summary>
        /// Determines if there are line breaks inserted into the 
        /// JSON to make it more easily human readable.
        /// </summary>
        public bool FormatJsonOutput
        {
            get { return masterSerializer.FormatJsonOutput; }
            set { masterSerializer.FormatJsonOutput = value; }
        }

        /// <summary>
        ///  Force a master Serializer to be passed for settings
        /// </summary>
        /// <param name="serializer"></param>
        public JSONSerializerBase(JSONSerializer serializer)
        {
            masterSerializer = serializer;
        }

    }

}
