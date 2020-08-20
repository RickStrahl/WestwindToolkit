using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing.Design;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Threading;

using Westwind.Utilities;
using Westwind.Web.WebForms.Properties;

namespace Westwind.Web.Controls
{
   /// <summary>
   /// An individual binding item. A BindingItem maps a source object - 
   /// a property/field or database field - to a property of a Control object.
   ///
   /// The object is a child for the DataBinder object which acts as a master
   /// object that performs the actual binding of individual BingingItems.
   /// 
   /// Binding Items can be attached to controls and if the control implements the
   /// IDataBinder.
   /// 
   /// Note: This class inherits from Component so the designer can properly render
   ///       the item as an extender control.
   /// </summary>
   //[TypeConverter(typeof(DataItemTypeConverter))]
   [ToolboxData("<{0}:DataBindingItem runat=\"server\" />")]
   [ToolboxItem(false)]
   [Category("DataBinding")]
   [DefaultEvent("Validate")]
   [Description("An individual databinding item that allows you to bind a source binding source - a database field or Object property typically - to a target control property")]
   [Serializable]
   public class DataBindingItem : Control
   {
      /// <summary>
      /// Explicitly set designmode flag - stock doesn't work on Collection child items
      /// </summary>
      protected new bool DesignMode = (HttpContext.Current == null);

       /// <summary>
      /// Default Constructor
      /// </summary>
      public DataBindingItem()
      {
      }

      /// <summary>
      /// Overridden constructor to allow DataBinder to be passed
      /// as a reference. Unfortunately ASP.NET doesn't fire this when
      /// creating the DataBinder child items.
      /// </summary>
      /// <param name="Parent"></param>
      public DataBindingItem(DataBinder Parent)
      {
         _Binder = Parent;
      }

      /// <summary>
      /// Reference to the DataBinder parent object.
      /// </summary>
      [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public DataBinder Binder
      {
         get { return _Binder; }
         set { _Binder = value; }
      }
      private DataBinder _Binder = null;

      /// <summary>
      /// The ID of the control to that is bound.
      /// </summary>
      [Browsable(true)]
      [NotifyParentProperty(true)]
      [Description("The ID of the control to that is bound.")]
      [DefaultValue("")]
      [TypeConverter(typeof(ControlIDConverter))]
      public string ControlId
      {
         get
         {
            return _ControlId;
         }
         set
         {
            _ControlId = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private string _ControlId = "";

      /// <summary>
      /// An optional instance of the control that can be assigned. Used internally
      /// by the DataBindiner to assign the control whenever possible as the instance
      /// is more efficient and reliable than the string name.
      /// </summary>
      [NotifyParentProperty(false)]
      [Description("An instance value for the controls")]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public Control ControlInstance
      {
         get
         {
            return _ControlInstance;
         }
         set
         {
            _ControlInstance = value;
            if (_ControlInstance != null)
                _ControlId = ControlInstance.ID;
         }
      }
      private Control _ControlInstance = null;

      
       /// <summary>
       /// The value that is to be assigned to the control after unbinding.
       /// </summary>
      public object UnboundValue
      {
          get { return _UnboundValue; }
          set { _UnboundValue = value; }
      }
      private object _UnboundValue = null;


      /// <summary>
      /// The binding source object that is the source for databinding.
      /// This is an object of some sort and can be either a real object
      /// or a DataRow/DataTable/DataSet. If a DataTable is used the first row 
      /// is assumed. If a DataSet is used the first table and first row are assumed.
      ///
      /// The object reference is always Page relative, so binding doesn't work
      /// against local variables, only against properties of the form. Form
      /// properties that are bound should be marked public or protected, but
      /// not private as Reflection is used to get the values. 
      /// 
      /// This or me is implicit, but can be specified so
      ///  "Customer" or "this.Customer" is equivalent. 
      /// </summary>
      /// <example>
      /// // Bind a DataRow Item
      /// bi.BindingSource = "Customer.DataRow";
      /// bi.BindingSourceMember = "LastName";
      ///
      /// // Bind a DataRow within a DataSet  - not recommended though
      /// bi.BindingSource = "this.Customer.Tables["TCustomers"].Rows[0]";
      /// bi.BindingSourceMember = "LastName";
      ///
      /// // Bind an Object
      /// bi.BindingSource = "InventoryItem.Entity";
      /// bi.BindingSourceMember = "ItemPrice";
      /// 
      /// // Bind a form property
      /// bi.BindingSource = "this";   // also "me" 
      /// bi.BindingSourceMember = "CustomerPk";
      /// </example>
      [NotifyParentProperty(true)]
      [Description("The name of the object or DataSet/Table/Row to bind to. Page relative. Example: Customer.DataRow = this.Customer.DataRow"), DefaultValue("")]
      public string BindingSource
      {
         get { return _BindingSource; }
         set
         {
            _BindingSource = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private string _BindingSource = "";


      /// <summary>
      /// An instance of the object that the control is bound to
      /// Optional - can be passed instead of a BindingSource string. Using
      /// a reference is more efficient. Declarative use in the designer
      /// always uses strings, code base assignments should use instances
      /// with BindingSourceObject.
      /// </summary>
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public object BindingSourceObject
      {
         get { return _BindingSourceObject; }
         set
         {
            _BindingSourceObject = value;
         }
      }
      private object _BindingSourceObject = null;

      /// <summary>
      /// The property or field on the Binding Source that is
      /// bound. Example: BindingSource: Customer.Entity BindingSourceMember: Company
      /// </summary>
      [NotifyParentProperty(true)]
      [Description("The name of the property or field to bind to. Example: So you can bind to a BindingSource of Customer.DataRow and the BindingSourceMember is Company."), DefaultValue("")]
      public string BindingSourceMember
      {
         get { return _BindingSourceMember; }
         set
         {
            _BindingSourceMember = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private string _BindingSourceMember = "";

      /// <summary>
      /// Property that is bound on the target controlId
      /// </summary>
      [NotifyParentProperty(true)]
      [Description("Property that is bound on the target control"), DefaultValue("Text")]
      public string BindingProperty
      {
         get { return _BindingProperty; }
         set
         {
            _BindingProperty = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private string _BindingProperty = "Text";

      /// <summary>
      /// Format Expression ( {0:c) ) applied to the binding source when it's displayed.
      /// Watch out for two way conversion issues when formatting this way. If you create
      /// expressions and you are also saving make sure the format used can be saved back.
      /// </summary>
      [NotifyParentProperty(true)]
      [Description("Format Expression ( {0:c) ) applied to the binding source when it's displayed."), DefaultValue("")]
      public string DisplayFormat
      {
         get { return _DisplayFormat; }
         set
         {
            _DisplayFormat = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }      
      private string _DisplayFormat = "";

      /// <summary>
      /// If set requires that the control contains a value, otherwise a validation error is thrown
      /// Useful mostly on TextBox controls only.
      /// </summary>
      [NotifyParentProperty(true)]
      [Description("If set requires that the control contains a value, otherwise a validation error is thrown - recommended only on TextBox controls."), DefaultValue(false)]
      public bool IsRequired
      {
         get { return _IsRequired; }
         set
         {
            _IsRequired = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private bool _IsRequired = false;


      /// <summary>
      /// Determines whether the content displayed is Html encoded to prevent script injection
      /// </summary>
      [NotifyParentProperty(true)]
      [Description("Determines whether the content displayed is Html encoded to prevent script injection")]
      [DefaultValue(false)]
      public bool HtmlEncode
      {
          get { return _HtmlEncode; }
          set { _HtmlEncode = value; }
      }
      private bool _HtmlEncode = false;


      /// <summary>
      /// A descriptive name for the field used for error display
      /// </summary>
      [Description("A descriptive name for the field used for error display"), DefaultValue("")]
      [NotifyParentProperty(true)]
      public string UserFieldName
      {
         get { return _UserFieldName; }
         set
         {
            _UserFieldName = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private string _UserFieldName = "";

      /// <summary>
      /// Determines how binding and validation errors display on the control
      /// </summary>
      [Description("Determines how binding and validation errors display on the control"),
       DefaultValue(BindingErrorMessageLocations.WarningIconRight)]
      [NotifyParentProperty(true)]
      public BindingErrorMessageLocations ErrorMessageLocation
      {
         get { return _ErrorMessageLocation; }
         set
         {
            _ErrorMessageLocation = value;
            if (DesignMode && Binder != null)
               Binder.NotifyDesigner();
         }
      }
      private BindingErrorMessageLocations _ErrorMessageLocation = BindingErrorMessageLocations.WarningIconRight;

      /// <summary>
      /// Internal property that lets you know if there was binding error
      /// on this control after binding occurred
      /// </summary>
      [NotifyParentProperty(true)]
      [Browsable(false)]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public bool IsBindingError
      {
         get { return _IsBindingError; }
         set { _IsBindingError = value; }
      }
      private bool _IsBindingError = false;

      /// <summary>
      /// An error message that gets set if there is a binding error
      /// on the control. If this value is pre-set this value is used
      /// instead of an auto-generated message
      /// </summary>
      [NotifyParentProperty(true)]
      //[Browsable(false)]
      //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public string BindingErrorMessage
      {
         get { return _BindingErrorMessage; }
         set { _BindingErrorMessage = value; }
      }
      private string _BindingErrorMessage = "";

      /// <summary>
      /// Determines how databinding and unbinding is done on the target control. 
      /// One way only fires DataBind() and ignores Unbind() calls. 
      /// Two-way does both. None effectively turns off binding.
      /// </summary>
      [Description("Determines how databinding and unbinding is done on the target control. One way only fires DataBind() and ignores Unbind() calls. Two-way does both"),
      Browsable(true), DefaultValue(BindingModes.TwoWay)]
      public BindingModes BindingMode
      {
         get { return _BindingMode; }
         set { _BindingMode = value; }
      }
      private BindingModes _BindingMode = BindingModes.TwoWay;

      /// <summary>
      /// Use this event to hook up special validation logic. Called after binding completes. Return false to indicate validation failed
      /// </summary>
      [Browsable(true), Description("Use this event to hook up special validation logic. Called after binding completes. Return false to indicate validation failed")]
      public event delDataBindingItemValidate Validate;

      /// <summary>
      /// Fires the Validation Event
      /// </summary>
      /// <returns></returns>
      public bool OnValidate()
      {
         if (Validate != null)
         {
            DataBindingValidationEventArgs Args = new DataBindingValidationEventArgs();
            Args.DataBindingItem = this;

            Validate(this, Args);

            if (!Args.IsValid)
               return false;
         }

         return true;
      }

    

      /// <summary>
      /// Binds a source object and property to a control's property. For example
      /// you can bind a business object to a the text property of a text box, or 
      /// a DataRow field to a text box field. You specify a binding source object 
      /// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
      /// and bind it to the control and the property specified (Text).
      ///
      /// This method defaults `this` to the Parent Container of the DataBinder
      /// </summary>
      public new void DataBind()
      {
         if (BindingMode == BindingModes.None)
            return;

         DataBind(Binder?.Parent ?? Page);
      }

      /// <summary>
      /// Binds a source object and property to a control's property. For example
      /// you can bind a business object to a the text property of a text box, or 
      /// a DataRow field to a text box field. You specify a binding source object 
      /// (Customer.Entity or Customer.DataRow) and property or field(Company, FirstName)
      /// and bind it to the control and the property specified (Text).
      /// </summary>
      /// <param name="container">The Base control that binding source objects are attached to. It's recommended you define the databinder as a child of a Page or UserControl object so you have clear reference point of the `this` pointer for bindings.</param>
      public void DataBind(Control container)
      {
         if (BindingMode == BindingModes.None || BindingMode == BindingModes.UnbindOnly)
            return;
         
         // if binding source is empty try to load it from default binding source
         if (string.IsNullOrEmpty(BindingSource) &&
             Binder != null &&
             !string.IsNullOrEmpty(Binder.DefaultBindingSource))
         {
             BindingSource = Binder.DefaultBindingSource;
         }

         // Empty BindingSource - simply skip
         if (BindingSourceObject == null &&
             string.IsNullOrEmpty(BindingSource) ||
             string.IsNullOrEmpty(BindingSourceMember))
            return;

         // Retrieve the binding source either by object reference or by name
         string bindingSource = BindingSource;
         object bindingSourceObject = BindingSourceObject;

         string bindingSourceMember = BindingSourceMember;
         string bindingProperty = BindingProperty;

         Control activeControl = null;
         if (ControlInstance != null)
            activeControl = ControlInstance;
         else
            activeControl = WebUtils.FindControlRecursive(container, ControlId);

         try
         {
            if (activeControl == null)
               throw new ApplicationException("Control not found for binding to: " + ControlId);

            // Assign so error handler can get a clean control reference
            ControlInstance = activeControl;

            // Retrieve the bindingsource by name - otherwise we use the 
            if (bindingSourceObject == null)
            {
               // Get a reference to the actual control source object
               // Allow this or me to be bound to the page
               if (bindingSource == "this" || bindingSource.ToLower() == "me")
                  bindingSourceObject = container;
               else
                   bindingSourceObject = ReflectionUtils.GetPropertyEx(container, bindingSource);
            }

            if (bindingSourceObject == null)
               throw new BindingErrorException("Invalid BindingSource: " +
                                               BindingSource + "." + BindingSourceMember);

            // Retrieve the control source value
            object Value;

            if (bindingSourceObject is DataSet)
            {
               string Tablename = bindingSourceMember.Substring(0, bindingSourceMember.IndexOf("."));
               string Columnname = bindingSourceMember.Substring(bindingSourceMember.IndexOf(".") + 1);
               DataSet Ds = (DataSet)bindingSourceObject;
               Value = Ds.Tables[Tablename].Rows[0][Columnname];
            }
            else if (bindingSourceObject is DataRow)
            {
               DataRow Dr = (DataRow)bindingSourceObject;
               Value = Dr[bindingSourceMember];
            }
            else if (bindingSourceObject is DataTable)
            {
               DataTable Dt = (DataTable)bindingSourceObject;
               Value = Dt.Rows[0][bindingSourceMember];
            }
            else if (bindingSourceObject is DataView)
            {
               DataView Dv = (DataView)bindingSourceObject;
               Value = Dv.Table.Rows[0][bindingSourceMember];
            }
            else
            {
               Value = ReflectionUtils.GetPropertyEx(bindingSourceObject, bindingSourceMember);
            }

            /// Figure out the type of the control we're binding to
            object BindingValue = ReflectionUtils.GetProperty(activeControl, bindingProperty);
            Type BindingType = null;
            if (BindingValue != null)
                BindingType = BindingValue.GetType();

            // Most binding types are to textboxes which have string values
            // so we have to convert from type to string and optionally provide formatting
            if (BindingType != null && BindingType == typeof(string))
            {                
                if (Value == null)
                    ReflectionUtils.SetProperty(activeControl, bindingProperty, "");
                else
                {
                    
                    // Handle format string
                    if (!string.IsNullOrEmpty(DisplayFormat))
                        ReflectionUtils.SetProperty(activeControl, bindingProperty, String.Format(DisplayFormat, Value));
                    else
                    {
                        if (HtmlEncode)
                            Value = HttpUtility.HtmlEncode((string)Value);
                     
                        ReflectionUtils.SetProperty(activeControl, bindingProperty, Value.ToString());
                    }
                }
            }
            else
                // Otherwise we're just retrieving a property value and are reassigning it to the control
                // Just assign the value without any translation
                ReflectionUtils.SetProperty(activeControl, bindingProperty, Value);            
         }
         catch (Exception ex)
         {
            string lcException = ex.Message;
            throw (new BindingErrorException("Unable to bind " +
                bindingSource + "." +
                bindingSourceMember));
         }
      }

      /// <summary>
      /// Unbinds control properties back into the control source.
      /// 
      /// This method uses reflection to lift the data out of the control, then 
      /// parses the string value back into the type of the data source. If an error 
      /// occurs the exception is not caught internally, but generally the 
      /// FormUnbindData method captures the error and assigns an error message to 
      /// the BindingErrorMessage property of the control.
      /// </summary>
      public void Unbind()
      {
         if (BindingMode != BindingModes.TwoWay)
            return;

         if (Binder != null)
            Unbind(Binder.Page);

         Unbind(Page);
      }

      /// <summary>
      /// Unbinds control properties back into the control source.
      /// 
      /// This method uses reflection to lift the data out of the control, then 
      /// parses the string value back into the type of the data source. If an error 
      /// occurs the exception is not caught internally, but generally the 
      /// FormUnbindData method captures the error and assigns an error message to 
      /// the BindingErrorMessage property of the control.
      /// <seealso>Class wwWebDataHelper</seealso>
      /// </summary>
      /// <param name="WebPage">
      /// The base control that binding sources are based on.
      /// </param>
      public void Unbind(Control BindingContainerControl)
      {

         // Get the Control Instance first so we ALWAYS have a ControlId
         // instance reference available
         Control ActiveControl = null;
         if (ControlInstance != null)
            ActiveControl = ControlInstance;
         else
            ActiveControl = WebUtils.FindControlRecursive(BindingContainerControl, ControlId);

         if (ActiveControl == null)
            throw new ApplicationException(Resources.InvalidControlId);

         ControlInstance = ActiveControl;

         // Don't unbind this item unless we're in TwoWay mode
         if (BindingMode != BindingModes.TwoWay &&
             BindingMode != BindingModes.UnbindOnly)
            return;

         // if binding source is empty try to load it from default binding source
         if (string.IsNullOrEmpty(this.BindingSource) &&
             Binder != null &&
             !string.IsNullOrEmpty(Binder.DefaultBindingSource))
         {
             this.BindingSource = Binder.DefaultBindingSource;
         }


         // Empty BindingSource - simply skip
         if (this.BindingSourceObject == null &&
             string.IsNullOrEmpty(this.BindingSource) ||
             string.IsNullOrEmpty(this.BindingSourceMember))
            return;

         // Retrieve the binding source either by object reference or by name
         string BindingSource = this.BindingSource;
         object BindingSourceObject = this.BindingSourceObject;

         string BindingSourceMember = this.BindingSourceMember;
         string BindingProperty = this.BindingProperty;

         if (BindingSourceObject == null)
         {
            if ( string.IsNullOrEmpty(BindingSource) ||
                 string.IsNullOrEmpty(BindingSourceMember) )
               return;

            if (BindingSource == "this" || BindingSource.ToLower() == "me")
               BindingSourceObject = BindingContainerControl;
            else
               BindingSourceObject = ReflectionUtils.GetPropertyEx(BindingContainerControl, BindingSource);
         }

         if (BindingSourceObject == null)
            throw new ApplicationException(Resources.InvalidBindingSource);


         // Retrieve the new value from the control
         object ControlValue = ReflectionUtils.GetPropertyEx(ActiveControl, BindingProperty);

         // Check for Required values not being blank
         if (IsRequired && (string)ControlValue == "")
            throw new RequiredFieldException();

         // Try to retrieve the type of the BindingSourceMember
         Type typBindingSource = null;
         //string BindingSourceType;
         string DataColumn = null;
         string DataTable = null;

          // copy properties for local modification
          object bindingSourceObject = BindingSourceObject;
          string bindingSourceMember = BindingSourceMember;

         if (bindingSourceObject is DataSet)
         {
            // Split out the datatable and column names
            int At = bindingSourceMember.IndexOf(".");
            DataTable = bindingSourceMember.Substring(0, At);
            DataColumn = bindingSourceMember.Substring(At + 1);
            DataSet Ds = (DataSet)bindingSourceObject;
            typBindingSource = Ds.Tables[DataTable].Columns[DataColumn].DataType;
         }
         else if (bindingSourceObject is DataRow)
         {
            DataRow Dr = (DataRow)bindingSourceObject;
            typBindingSource = Dr.Table.Columns[bindingSourceMember].DataType;
         }
         else if (bindingSourceObject is DataTable)
         {
            DataTable dt = (DataTable)bindingSourceObject;
            typBindingSource = dt.Columns[bindingSourceMember].DataType;
         }
         else
         {
             Type t = bindingSourceObject.GetType();

             if (t.IsCOMObject)
             {
                 object val = t.InvokeMember(bindingSourceMember, ReflectionUtils.MemberAccess |  BindingFlags.GetProperty, null, BindingSourceObject, null);
                 if (val == null)
                     typBindingSource = typeof(string);
                 else
                 typBindingSource = val.GetType();
             }
             else
             {                 
                 string member = bindingSourceMember;
                 
                 // if we have . in the binding source we need to find the 
                 // base instance and final member to bind to
                 if (member.Contains("."))
                 {
                     string objName = member.Substring(0,member.LastIndexOf("."));                       
                     bindingSourceMember = member.Substring(member.LastIndexOf(".")+1);
                     bindingSourceObject = ReflectionUtils.GetPropertyEx(bindingSourceObject, objName);                    
                 }
             
                 //PropertyInfo[] pi = BindingSourceObject.GetType().GetProperties();

                 t = bindingSourceObject.GetType();

                 // It's an object property or field - get it
                 MemberInfo[] MInfo = t.GetMember(bindingSourceMember,
                                                  ReflectionUtils.MemberAccess);
                 if (MInfo[0].MemberType == MemberTypes.Field)
                 {
                     FieldInfo Field = (FieldInfo)MInfo[0];
                     typBindingSource = Field.FieldType;
                 }
                 else
                 {
                     PropertyInfo loField = (PropertyInfo)MInfo[0];
                     typBindingSource = loField.PropertyType;
                 }
             }
         }

         //  Retrieve the value
         

         // If it's a string we have to convert most likely 
         //     unless the target type is also a string
         if (ControlValue is string && typBindingSource != typeof(string))
             UnboundValue = ReflectionUtils.StringToTypedValue((string)ControlValue, 
                                                         typBindingSource, 
                                                         Thread.CurrentThread.CurrentCulture);
         else
             // Just assign type directly. 
             UnboundValue = ControlValue;
       
         /// Write the value back to the underlying object/data item
         if (bindingSourceObject is DataSet)
         {
            DataSet Ds = (DataSet)bindingSourceObject;
            Ds.Tables[DataTable].Rows[0][DataColumn] = UnboundValue;
         }
         else if (bindingSourceObject is DataRow)
         {
            DataRow Dr = (DataRow)bindingSourceObject;
            Dr[bindingSourceMember] = UnboundValue;
         }
         else if (bindingSourceObject is DataTable)
         {
            DataTable dt = (DataTable)bindingSourceObject;
            dt.Rows[0][bindingSourceMember] = UnboundValue;
         }
         else if (bindingSourceObject is DataView)
         {
            DataView dv = (DataView)bindingSourceObject;
            dv[0][bindingSourceMember] = UnboundValue;
         }
         else             
            // It's a property
            ReflectionUtils.SetPropertyEx(bindingSourceObject, bindingSourceMember, UnboundValue);

         // Clear the error message - no error
         BindingErrorMessage = "";
      }

      /// <summary>
      /// Returns a the control bindingsource and binding source member
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         if (string.IsNullOrEmpty(BindingSource))
            return base.ToString();

         return BindingSource + "." + BindingSourceMember;
      }


      #region Hide Properties for the Designer
      [Browsable(false)]
      public override string ID
      {
          get
          {
              return base.ID;
          }
          set
          {
              base.ID = value;
          }
      }

      [Browsable(false)]
      public override bool Visible
      {
          get
          {
              return base.Visible;
          }
          set
          {
              base.Visible = value;
          }
      }

      [Browsable(false)]
      public override bool EnableViewState
      {
          get
          {
              return base.EnableViewState;
          }
          set
          {
              base.EnableViewState = value;
          }
      }
      #endregion

   }

   /// <summary>
   /// Enumeration for the various binding error message locations possible
   /// that determine where the error messages are rendered in relation to the
   /// control.
   /// </summary>
   public enum BindingErrorMessageLocations
   {
      /// <summary>
      /// Displays an image icon to the right of the control
      /// </summary>
      WarningIconRight,
      /// <summary>
      /// Displays a text ! next to the control 
      /// </summary>
      TextExclamationRight,
      /// <summary>
      /// Displays the error message as text below the control
      /// </summary>
      RedTextBelow,
      /// <summary>
      /// Displays an icon and the text of the message below the control.
      /// </summary>
      RedTextAndIconBelow,
      /// <summary>
      /// No binding
      /// </summary>
      None
   }

   /// <summary>
   /// Determines how databinding is performed for the target control. Note that 
   /// if a DataBindingItem is  marked for None or OneWay, the control will not 
   /// be unbound or in the case of None bound even when an explicit call to 
   /// DataBind() or Unbind() is made.
   /// </summary>
   public enum BindingModes
   {
      /// <summary>
      /// Databinding occurs for DataBind() and Unbind()
      /// </summary>
      TwoWay,
      /// <summary>
      /// DataBinding occurs for DataBind() only
      /// </summary>
      OneWay,
      /// <summary>
      /// DataBinding occurs for Unbind() Only 
      /// </summary>
      UnbindOnly,
      /// <summary>
      /// No binding occurs (useful for conditional) enabling of binding via code
      /// </summary>
      None
   }


   /// <summary>
   /// Event Args passed to a Validate event of a DataBindingItem control.
   /// </summary>
   public class DataBindingValidationEventArgs : EventArgs
   {
      /// <summary>
      /// Instance of the DataBinding Control that fired this Validation event.
      /// </summary>
      public DataBindingItem DataBindingItem
      {
         get { return _DataBindingItem; }
         set { _DataBindingItem = value; }
      }
      private DataBindingItem _DataBindingItem = null;

      /// <summary>
      /// Out flag that determines whether this control value is valid.
      /// </summary>
      public bool IsValid
      {
         get { return _IsValid; }
         set { _IsValid = value; }
      }
      private bool _IsValid = true;
   }

}
