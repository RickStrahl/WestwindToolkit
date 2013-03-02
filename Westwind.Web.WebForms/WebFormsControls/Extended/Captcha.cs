#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2008 2011
 *          http://www.west-wind.com/
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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections;
using Westwind.Utilities;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace Westwind.Web.Controls
{
    
    /// <summary>
    /// A Captcha control that uses simple math expressions for validation
    /// before accepting input. 
    /// 
    /// This control is fully self contained and carries the expected 'result'
    /// with it in ControlState data, so unlike image captcha there's no separate
    /// handler involved.
    /// </summary>
    public class Captcha : WebControl 
    {
        protected int ExpectedResult = 0;
        internal DisplayExpression Expression = null;
        protected string EnteredValue = string.Empty;
        
        /// <summary>
        /// Set during validation
        /// </summary>
        [Description("Validation Status of the control. Typically set automatically."),DefaultValue(false)]
        [Category("Validation")]
        public bool Validated
        {
            get { return _Validated; }
            set { _Validated = value; }
        }
        private bool _Validated = false;

        
        /// <summary>
        /// The error message that is displayed when the not validated
        /// </summary>
        [Description("The error message that is displayed when the not validated")]
        [Category("Validation")]
        [Localizable(true)]
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = "";

        
        /// <summary>
        /// The message that is displayed above the expression that is to be entered.
        /// </summary>
        [Description("The message that is displayed above the expression that is to be entered.")]
        [Category("Validation"), DefaultValue("Please validate the following expression:")]
        [Localizable(true)]
        public string DisplayMessage
        {
            get { return _DisplayMessage; }
            set { _DisplayMessage = value; }
        }
        private string _DisplayMessage = "Please validate the following expression:";


       /// <summary>
       /// The timeout for this message in minutes.
       /// </summary>
       [Description("Timeout for this captcha in minutes"),DefaultValue(10)]
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        private int _Timeout = 10;

        /// <summary>
        /// An optional page identifier that has to be matched by
        /// the validation and is written into ViewState. Ensures
        /// that spammers can't just capture the full post buffer
        /// and repost to another page.
        /// 
        /// This can be any unique value that the page uses like
        /// an ID or other page specific value that makes the
        /// particular request unique.
        /// 
        /// Highly recommended you set this.
        /// </summary>
        public string UniquePageId
        {
            get { return _UniquePageId; }
            set { _UniquePageId = value; }
        }
        private string _UniquePageId = string.Empty;

        

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            
            Page.RegisterRequiresControlState(this);
            
            /// Use PreLoad instead of IPostbackDataHandler
            ///     for loading control POST data as part of Validate()
            ///     cycle. Validate must fire prior to Load but
            ///     after ViewState's been restored.
            Page.PreLoad += OnPreLoad;
        }

        protected void OnPreLoad(object sender, EventArgs e)
        {            
            if (!Page.IsPostBack)
                GenerateExpression();
            else
                Validate();           
            
            // Fudge the page so the control Id is added to the numeric value entered
            Page.ClientScript.RegisterOnSubmitStatement(GetType(), Expression.Id,
                "var CpCtl = document.getElementById('" + Expression.Id + "');\r\n" +
                "if (CpCtl) CpCtl.value += '_" +  Expression.Id + "';");            
        }
        

        protected void Validate()
        {            
            EnteredValue = Page.Request.Form[Expression.Id] as string;
            if (EnteredValue == null)
                EnteredValue = "";

            // Check for the fudge value
            if (!EnteredValue.EndsWith("_" + Expression.Id))
            {
                GenerateExpression();
                Validated = false;
                EnteredValue = string.Empty;
                ErrorMessage = "Invalid Page validation - missing security code.";
                return;
            }
            
            // Strip out fudge value added with JavaScript
            EnteredValue = EnteredValue.Replace("_" + Expression.Id, "");

            if (Expression == null)
            {
                GenerateExpression();
                Validated = false;
                EnteredValue = string.Empty;
                ErrorMessage = "Page validation cannot be applied - please reload the page";
                return;
            }

            if (Expression.Entered < DateTime.UtcNow.AddMinutes(Timeout * -1))
            {
                GenerateExpression();
                Validated = false;
                EnteredValue = string.Empty;
                ErrorMessage = "Page validation code has expired";
                return;
            }

            if (Expression.UniquePageId != UniquePageId)
            {
                GenerateExpression();
                Validated = false;
                EnteredValue = string.Empty;
                ErrorMessage = "Page validation failed - postback to invalid page";
                return;
            }

            int val = -1;
            int.TryParse(EnteredValue,out val);

            if (val == -1)
            {
                Validated = false;
                ErrorMessage = "Invalid page validation input value.";
                GenerateExpression();
                return;
            }

            if (Expression.ExpectedValue == val)
                Validated = true;
            else
            {
                Validated = false;
                ErrorMessage = "Page validation failed.<br/><small>Note: Javascript must be turned on.</small>";
                EnteredValue = string.Empty;
                GenerateExpression();
            }
            
        }

        /// <summary>
        /// Method can be used to generate a new Expression object
        /// with new values to use. Use this method to update the
        /// wwCaptcha expression after you've saved an entry.        
        /// </summary>
        /// <returns></returns>
        public void GenerateExpression()
        {
            DisplayExpression exp = new DisplayExpression();
            Random rand = new Random();
            
            exp.Value1 = rand.Next(9) + 1;
            exp.Value2 = rand.Next(9) + 1;
            exp.Operation =  rand.Next(1) == 0 ? "+" : "*" ;

            if (exp.Operation == "+")
                exp.ExpectedValue = exp.Value1 + exp.Value2;
            
            if (exp.Operation == "*")
               exp.ExpectedValue = exp.Value1 * exp.Value2;

            exp.UniquePageId = UniquePageId;

            Expression = exp;            
        }

        
        protected override void Render(HtmlTextWriter writer)
        {
            if (CssClass == null)
                Style.Add(HtmlTextWriterStyle.Padding, "5px");
            else
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            
            if (!Width.IsEmpty)
                Style.Add(HtmlTextWriterStyle.Width, Width.ToString());

            writer.AddAttribute(HtmlTextWriterAttribute.Style,Style.Value);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (Expression == null)
                Expression = new DisplayExpression();

            if (!string.IsNullOrEmpty(DisplayMessage))
                writer.Write(DisplayMessage + "<br />" );

            // Write the Expression label
            writer.Write(Expression.Value1.ToString() + " " + 
                         Expression.Operation + " " + 
                         Expression.Value2.ToString() + " = " );
            
            writer.Write(" <input type='text' value='" + EnteredValue +
                         "' id='" + Expression.Id + "' name='" + Expression.Id + "' style='width: 30px;' />");


            writer.RenderEndTag(); // main div
        }

        protected override void LoadControlState(object savedState)
        {            
            Expression = savedState as DisplayExpression;

            if (Expression == null)
                Expression = new DisplayExpression();
        }

        protected override object SaveControlState()
        {
            return Expression;
        }
    }


    [Serializable]    
    internal class DisplayExpression
    {
        public int ExpectedValue = 0;
        public int Value1 = 0;
        public int Value2 = 0;
        public string Operation = "+";
        public string Id= Guid.NewGuid().GetHashCode().ToString("x");
        public string UniquePageId = string.Empty;
        public DateTime Entered = DateTime.UtcNow;
    }



}
