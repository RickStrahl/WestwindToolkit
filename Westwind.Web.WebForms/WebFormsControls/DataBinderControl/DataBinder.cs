#define USE_WWBUSINESS

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI.Design;
using System.Text.RegularExpressions;

using Westwind.Utilities;
using System.Web;
using System.Collections;
using Westwind.Web.WebForms.Properties;
using System.Linq;

namespace Westwind.Web.Controls
{
    /// <summary>
    /// The DataBinder class provides two-way, simple databinding a single
    /// datasource value and single control property. It can bind object properties
    /// and fields and database values (DataRow fields) to a control property such
    /// as the Text, Checked or SelectedValue properties. In a nutshell the
    /// controls acts as a connector between a datasource and the control and
    /// provides explicit databinding for the control.
    /// 
    /// The control supports two-way binding. Control can be bound to the
    /// datasource values and can be unbound by taking control values and storing
    /// them back into the datasource. The process is performed explicitly by
    /// calling the DataBind() and Unbind() methods of the control. Controls
    /// attached to the databinder can also be bound individually.
    /// 
    /// The control also provides a BindErrors collection which captures any
    /// binding errors and allows friendly display of these binding errors using
    /// the ToHtml() method. BindingErrors can be manually added and so application
    /// level errors can be handled the same way as binding errors. It's also
    /// possible to pull in ASP.NET Validator control errors.
    /// 
    /// Simple validation is available with IsRequired for each DataBinding item.
    /// Additional validation can be performed server side by implementing the
    /// ValidateControl event which allows you to write application level
    /// validation code.
    /// 
    /// This control is implemented as an Extender control that extends any Control
    ///  based class. This means you can databind to ANY control class and its
    /// properties with this component.
    /// <seealso>Databinding with DataBinder</seealso>
    /// </summary>
    [NonVisualControl, Designer(typeof(DataBinderDesigner))]
    [ProvideProperty("DataBindingItem", typeof(Control))]
    [ParseChildren(true, "DataBindingItems")]
    [PersistChildren(false)]
    [DefaultProperty("DataBindingItems")]
    [DefaultEvent("ValidateControl")]
    public class DataBinder : Control, IExtenderProvider
    {
        public DataBinder()
        {
            _DataBindingItems = new DataBindingItemCollection(this);
        }

        public new bool DesignMode = (HttpContext.Current == null);

        /// <summary>
        /// A collection of all the DataBindingItems that are to be bound. Each 
        /// &lt;&lt;%= TopicLink([DataBindingItem],[_1UL03RIKQ]) %&gt;&gt; contains 
        /// the information needed to bind and unbind a DataSource to a Control 
        /// property.
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public DataBindingItemCollection DataBindingItems
        {
            get
            {
                return _DataBindingItems;
            }
        }
        DataBindingItemCollection _DataBindingItems = null;


        /////// <summary>
        /////// Collection of all the preserved properties that are to
        /////// be preserved/restored. Collection hold, ControlId, Property
        /////// </summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //[PersistenceMode(PersistenceMode.InnerProperty)]
        //public List<DataBindingItem> DataBindingItems
        //{
        //    get
        //    {
        //        return _DataBindingItems;
        //    }
        //}
        //List<DataBindingItem> _DataBindingItems = new List<DataBindingItem>();

        /// <summary>
        /// A collection of binding errors that is filled after binding or unbinding
        /// if errors occur during binding and unbinding.
        /// </summary>
        [Browsable(false)]
        public BindingErrors BindingErrors
        {
            get { return _BindingErrors; }
            //set { _BindingErrors = value; }
        }
        private BindingErrors _BindingErrors = new BindingErrors();

        /// <summary>
        /// Determines whether binding errors are display on controls.
        /// </summary>
        [Description("Determines whether binding errors are displayed on controls. The display mode is determined for each binding individually.")]
        [DefaultValue(true)]
        [Category("Error Display")]
        public bool ShowBindingErrorsOnControls
        {
            get { return _ShowBindingErrorsOnControls; }
            set { _ShowBindingErrorsOnControls = value; }
        }
        private bool _ShowBindingErrorsOnControls = true;


        
        /// <summary>
        /// Hides the Validator display and uses only the DataBinder's display for errors
        /// </summary>
        [Description("Hides the Validator display and uses only the DataBinder's display for errors")]
        [DefaultValue(true)]
        [Category("Error Display")]
        public bool HideValidators
        {
            get { return _HideValidators; }
            set { _HideValidators = value; }
        }
        private bool _HideValidators = true;


        /// <summary>
        /// A default binding source that is used if the binding source 
        /// on an individual item is not set.
        /// </summary>
        public string DefaultBindingSource
        {
            get { return _DefaultBindingSource; }
            set { _DefaultBindingSource = value; }
        }
        private string _DefaultBindingSource = "";


        /// <summary>
        /// Optional Url to the Warning and Info Icons.
        /// Note: Used only if the control uses images.
        /// </summary>
        [Description("Optional Image Url for the Error Icon. Used only if the control uses image icons."),
        Editor("System.Web.UI.Design.ImageUrlEditor", typeof(UITypeEditor)),
        DefaultValue("WebResource")]
        [Category("Error Display")]
        public string ErrorIconUrl
        {
            get { return _ErrorIconUrl; }
            set { _ErrorIconUrl = value; }
        }
        private string _ErrorIconUrl = "WebResource";


        /// <summary>
        /// Determines whether the control uses client script to inject error 
        /// notification icons/messages into the page. Setting this flag to true causes
        ///  JavaScript to be added to the page to create the messages. If false, the 
        /// DataBinder uses Controls.Add to add controls to the Page or other 
        /// Containers.
        /// 
        /// JavaScript injection is preferrable as it works reliable under all 
        /// environments except when JavaScript is off. Controls.Add() can have 
        /// problems if &lt;% %&gt; &lt;%= %&gt; script is used in a container that has
        ///  an error and needs to add a control.
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        [Description("Uses Client Script code to inject Validation Error messages into the document. More reliable than Controls.Add() due to markup restrictions"),
        DefaultValue(true)]
        public bool UseClientScriptHtmlInjection
        {
            get { return _UseClientScriptHtmlInjection; }
            set { _UseClientScriptHtmlInjection = value; }
        }
        private bool _UseClientScriptHtmlInjection = true;

        bool _ClientScriptInjectionScriptAdded = false;

        /// <summary>
        /// The Web Resource Url used to access retrieve the Error Icon.
        /// Used to minimize reloading this URL from the resource manager
        /// repeatedly.
        /// </summary>
        protected string ErrorIconWebResource
        {
            get
            {
                if (string.IsNullOrEmpty(_ErrorIconWebResource))
                    _ErrorIconWebResource = ClientScriptProxy.GetWebResourceUrl(this, GetType(), "Westwind.Web.WebForms.Resources.warning.gif");

                return _ErrorIconWebResource;
            }
        }
        private string _ErrorIconWebResource = "";


        /// <summary>
        /// Automatically imports all controls on the form that implement the IDataBinder interface and adds them to the DataBinder
        /// </summary>
        [Description("Automatically imports all controls on the form that implement the IDataBinder interface and adds them to the DataBinder"),
         Browsable(false), DefaultValue(false)]
        public bool AutoLoadDataBoundControls
        {
            get { return _AutoLoadDataBoundControls; }
            set { _AutoLoadDataBoundControls = value; }
        }
        private bool _AutoLoadDataBoundControls = false;

        /// <summary>
        /// Flag that determines whether controls where auto-loaded from the page.
        /// </summary>
        private bool _AutoLoadedDataBoundControls = false;

        /// <summary>
        /// Determines whether this control works as an Extender object to other controls on the form.
        /// In some situations it might be useful to disable the extender functionality such
        /// as when all databinding is driven through code or when using the IDataBinder
        /// interface with custom designed controls that have their own DataBinder objects.
        /// </summary>
        [Browsable(true), Description("Determines whether this control works as an Extender object to other controls on the form"), DefaultValue(true)]
        public bool IsExtender
        {
            get { return _IsExtender; }
            set { _IsExtender = value; }
        }
        private bool _IsExtender = true;


        /// <summary>
        /// Message displayed when IsRequired is blank. 
        /// 
        /// Format string where {0} is the derived field name or UserFieldName.
        /// </summary>
        [Description("Message displayed when IsRequired fails. Format string where {0} is the field name"),
         Category("Messages")]
        public string IsRequiredErrorMessage
        {
            get { return _EmptyErrorMessage; }
            set { _EmptyErrorMessage = value; }
        }
        private string _EmptyErrorMessage = "{0} can't be left empty";

        /// <summary>
        /// Error displayed when an unbinding error occurs. Typically
        /// this will be some sort of format conversion problem
        /// {0} denotes the derived field name.
        /// </summary>
        [Description("Error displayed when an unbinding error occurs - typically a format conversion error. {0} holds derived field name"), Category("Messages")]
        public string UnBindingErrorMessage
        {
            get { return _UnBindingErrorMessage; }
            set { _UnBindingErrorMessage = value; }
        }
        private string _UnBindingErrorMessage = "Invalid format for {0}";


        /// <summary>
        /// Binding Error message when a control fails to bind
        /// </summary>
        [Description("Binding Error message when a control fails to bind")]
        [DefaultValue("Binding Error"), Category("Messages")]
        public string BindingErrorMessage
        {
            get { return _BindingErrorMessage; }
            set { _BindingErrorMessage = value; }
        }
        private string _BindingErrorMessage = "Binding Error";




        /// <summary>
        /// Event that can be hooked to validate each control after it's been unbound. 
        /// Allows for doing application level validation of the data once it's been 
        /// returned.
        /// 
        /// This method receives a DataBindingItem parameter which includes a 
        /// reference to both the control and the DataSource object where you can check
        ///  values. Return false from the event method to indicate that validation 
        /// failed which causes a new BindingError to be created and added to the 
        /// BindingErrors collection.
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <example>
        /// &lt;&lt;code lang=&quot;C#&quot;&gt;&gt;protected bool 
        /// DataBinder_ValidateControl(Westwind.Web.Controls.DataBindingItem Item)
        /// {
        ///     if (Item.ControlInstance == txtCategoryId)
        ///     {
        ///         DropDownList List = Item.ControlInstance as DropDownList;
        ///         if (List.SelectedItem.Text == &quot;Dairy Products&quot;)
        ///         {
        ///             Item.BindingErrorMessage = &quot;Dairy Properties not allowed 
        /// (ValidateControl)&quot;;
        ///             return false;
        ///         }
        ///     }
        /// 
        ///     return true;
        /// }&lt;&lt;/code&gt;&gt;
        /// </example>
        [Description("Fired after a control has been unbound. Gets passed an instance of the DataBinding item. Event must check for the control to check.")]
        public event delItemResultNotification ValidateControl;

        /// <summary>
        /// Fired just before the control is bound. You can return false from the 
        /// handler to cause the control to not be bound
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        [Description("Fires immediately before a control is bound. Fires for all controls and is passed a DataBindingItem.")]
        public event delItemResultNotification BeforeBindControl;

        /// <summary>
        /// Fires immediately after the control has been bound. You can check for
        /// errors or perform additional validation.
        /// </summary>
        [Description("Fires immediately after a control has been bound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
        public event delItemNotification AfterBindControl;

        /// <summary>
        /// Fires immediately before unbinding of a control takes place.
        /// You can return false to abort DataUnbinding.
        /// </summary>
        [Description("Fires immediately before a control is unbound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
        public event delItemResultNotification BeforeUnbindControl;

        /// <summary>
        /// Fires immediately after binding is complete. You can check for errors 
        /// and take additional action. 
        /// </summary>
        [Description("Fires immediately after a control has been unbound. Gets passed a DataBinding Item instance. Fires for all bound controls.")]
        public event delItemNotification AfterUnbindControl;

        /// <summary>
        /// Make MS Ajax aware
        /// </summary>
        private ClientScriptProxy ClientScriptProxy = null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ClientScriptProxy = ClientScriptProxy.Current;
        }


        /// <summary>
        /// Performs data binding against each of the defined DataBindingItems defined 
        /// on the DataBinder control. This binds all BindingSources to the specified 
        /// control properties.
        /// 
        /// Typically DataBind is called in the Page_Load() of the page cycle and only 
        /// when the page originally loads - ie. (if !Page.IsPostPack). Subsequent page
        ///  hits post back values so you typically do not want to rebind values to 
        /// POST form variables on each hit.
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;
        /// Invoice.Load(id);   // load data to bind
        /// 
        /// // bind only on first load or if the product is changed
        /// if (!IsPostBack || IsProductChange)
        ///    DataBinder.DataBind();
        /// 
        /// // Manually bind this control always - it's ReadOnly and so doesn't post 
        /// back
        /// DataBinder.GetDataBindingItem(txtPk).DataBind();
        /// &lt;&lt;/code&gt;&gt;
        /// 
        /// Some controls - non-Postback, or read only controls for example - you will 
        /// want to rebind explicit each time so make sure those are bound explicitly 
        /// outside of the !IsPostBack block.
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <returns>true if there no errors. False otherwise.</returns>
        public new bool DataBind()
        {
            return DataBind(Page);
        }


        /// <summary>
        /// Performs data binding against each of the defined DataBindingItems defined 
        /// on the DataBinder control. This binds all BindingSources to the specified 
        /// control properties.
        /// 
        /// Typically DataBind is called in the Page_Load() of the page cycle and only 
        /// when the page originally loads - ie. (if !Page.IsPostPack). Subsequent page
        ///  hits post back values so you typically do not want to rebind values to 
        /// POST form variables on each hit.
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;
        /// Invoice.Load(id);   // load data to bind
        /// 
        /// // bind only on first load or if the product is changed
        /// if (!IsPostBack || IsProductChange)
        ///    DataBinder.DataBind(this);
        /// 
        /// // Manually bind this control always - it's ReadOnly and so doesn't post 
        /// back
        /// DataBinder.GetDataBindingItem(txtPk).DataBind();
        /// &lt;&lt;/code&gt;&gt;
        /// 
        /// Some controls - non-Postback, or read only controls for example - you will 
        /// want to rebind explicit each time so make sure those are bound explicitly 
        /// outside of the !IsPostBack block.
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <param name="Container">
        /// The top level container that is bound
        /// </param>
        public bool DataBind(Control Container)
        {
            if (AutoLoadDataBoundControls)
                LoadFromControls(Container);

            bool ResultFlag = true;

            // Run through each item and bind it
            foreach (DataBindingItem Item in DataBindingItems)
            {
                try
                {
                    if (BeforeBindControl != null)
                    {
                        if (!BeforeBindControl(Item))
                            continue;
                    }

                    // Here's where all the work happens
                    Item.DataBind(Container);
                }
                // Binding errors fire into here
                catch (Exception ex)
                {
                    HandleUnbindingError(Item, ex);
                }

                if (AfterBindControl != null)
                    AfterBindControl(Item);
            }

            return ResultFlag;

        }

        /// <summary>
        /// Unbinds value from controls back into their underlying binding sources for 
        /// the defined DataBinding items of this control. Returns true on success 
        /// false on failure.
        /// 
        /// Unbinding handles unbinding and checking for unbinding errors for invalid 
        /// data values that can't be converted back into their underlying data source.
        ///  On failure of .Unbind() the BindingErrors collection will be set. A 
        /// typical unbind operation occurs in  button click event or other 'save' 
        /// operation fired from the page:
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;
        /// protected void btnSave_Click(object sender, EventArgs e)
        /// {
        ///     // unbind back into the underlying data source: Product.Entity for most
        ///  fields
        ///     DataBinder.Unbind();
        /// 
        ///     // check for binding errors and display if there's a problem
        ///     if (DataBinder.BindingErrors.Count &gt; 0)
        ///     {
        ///         ErrorDisplay.Text = DataBinder.BindingErrors.ToHtml();
        ///         return;
        ///     }
        /// 
        ///     // validate the business object - check product entity for rule 
        /// violations
        ///     if (!Product.Validate())
        ///     {
        ///         // Automatically add binding errors from bus object 
        /// ValidationErrors
        ///         // requires IList that has ControlID and Message properties
        ///         
        /// DataBinder.AddValidationErrorsToBindingErrors(Product.ValidationE
        /// rrors);
        /// 
        ///         // You can also manually add binding error messages and assign to a
        ///  control
        ///         //DataBinder.AddBindingError("Invalid Country 
        /// Code",txtCountry);
        /// 
        ///         ErrorDisplay.Text = DataBinder.BindingErrors.ToHtml();
        ///         return;
        ///     }
        /// 
        ///     if (!Product.Save())
        ///     {
        ///         ErrorDisplay.ShowError("Couldn't save Product:&lt;br/&gt;" + 
        /// Product.ErrorMessage);
        ///         return;
        ///     }
        /// 
        ///     ErrorDisplay.ShowMessage("Product information has been saved.");
        /// }
        /// &lt;&lt;/code&gt;&gt;
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <returns>True if there are no errors. False if unbinding failed and BindingErrors Collection set with errors.</returns>
        public bool Unbind()
        {
            return Unbind(Page);
        }  

        /// <summary>
        /// Unbinds value from controls back into their underlying binding sources for 
        /// the defined DataBinding items of this control. Returns true on success 
        /// false on failure.
        /// 
        /// Unbinding handles unbinding and checking for unbinding errors for invalid 
        /// data values that can't be converted back into their underlying data source.
        ///  On failure of .Unbind() the BindingErrors collection will be set. A 
        /// typical unbind operation occurs in  button click event or other 'save' 
        /// operation fired from the page:
        /// 
        /// &lt;&lt;code lang="C#"&gt;&gt;
        /// protected void btnSave_Click(object sender, EventArgs e)
        /// {
        ///     // unbind back into the underlying data source: Product.Entity for most
        ///  fields
        ///     DataBinder.Unbind();
        /// 
        ///     // check for binding errors and display if there's a problem
        ///     if (DataBinder.BindingErrors.Count &gt; 0)
        ///     {
        ///         ErrorDisplay.Text = DataBinder.BindingErrors.ToHtml();
        ///         return;
        ///     }
        /// 
        ///     // validate the business object - check product entity for rule 
        /// violations
        ///     if (!Product.Validate())
        ///     {
        ///         // Automatically add binding errors from bus object 
        /// ValidationErrors
        ///         // requires IList that has ControlID and Message properties
        ///         
        /// DataBinder.AddValidationErrorsToBindingErrors(Product.ValidationE
        /// rrors);
        /// 
        ///         // You can also manually add binding error messages and assign to a
        ///  control
        ///         //DataBinder.AddBindingError("Invalid Country 
        /// Code",txtCountry);
        /// 
        ///         ErrorDisplay.Text = DataBinder.BindingErrors.ToHtml();
        ///         return;
        ///     }
        /// 
        ///     if (!Product.Save())
        ///     {
        ///         ErrorDisplay.ShowError("Couldn't save Product:&lt;br/&gt;" + 
        /// Product.ErrorMessage);
        ///         return;
        ///     }
        /// 
        ///     ErrorDisplay.ShowMessage("Product information has been saved.");
        /// }
        /// &lt;&lt;/code&gt;&gt;
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <param name="Container">
        /// The top level container Control that is bound.
        /// </param>
        /// <returns>True if there are no errors. False if unbinding failed. Check the BindingErrors for errors.</returns>
        public bool Unbind(Control Container)
        {
            if (AutoLoadDataBoundControls)
                LoadFromControls(Container);

            bool ResultFlag = true;

            // Loop through all bound items and unbind them
            foreach (DataBindingItem bindingItem in DataBindingItems)
            {
                try
                {
                    if (BeforeUnbindControl != null)
                    {
                        if (!BeforeUnbindControl(bindingItem))
                            continue;
                    }

                    // here's where all the work happens!
                    bindingItem.Unbind(Container);

                    // Run any validation logic - DataBinder Global first
                    if (!OnValidateControl(bindingItem))
                        HandleUnbindingError(bindingItem, new ValidationErrorException(bindingItem.BindingErrorMessage));

                    // Run control specific validation
                    if (!bindingItem.OnValidate())
                        HandleUnbindingError(bindingItem, new ValidationErrorException(bindingItem.BindingErrorMessage));
                }
                // Handles any unbinding errors
                catch (Exception ex)
                {
                    HandleUnbindingError(bindingItem, ex);
                    ResultFlag = false;
                }

                // Notify that we're done unbinding
                if (AfterUnbindControl != null)
                    AfterUnbindControl(bindingItem);
            }

            Page.Validate();

            // Add existing validators to the BindingErrors
            foreach (IValidator Validator in Page.Validators)
            {
                if (Validator.IsValid)
                    continue;

                string clientId = null;


                BaseValidator baseValidator = Validator as BaseValidator;

                // Find the DataBindingItem related to this control
                DataBindingItem item = null;
                foreach (DataBindingItem itm in DataBindingItems)
                {
                    if (itm.ControlId == baseValidator.ControlToValidate)
                    {
                        item = itm;
                        break;
                    }
                }
                if (item != null)
                {
                    item.BindingErrorMessage = baseValidator.ErrorMessage;
                    HandleUnbindingError(item,new ValidationErrorException(item.BindingErrorMessage));
                }
                else
                {
                    Control Ctl = WebUtils.FindControlRecursive(Page, baseValidator.ControlToValidate);
                    if (Ctl != null)
                    
                        clientId = Ctl.ClientID;                   

                    BindingErrors.Add(new BindingError(Validator.ErrorMessage, clientId));
                }
                
                // Set Validator to valid so the validator no longer displays
                if (this.HideValidators)
                {
                    baseValidator.EnableViewState = false;
                    baseValidator.Visible = false;
                }
            }

            return ResultFlag;
        }

        /// <summary>
        /// Manages errors that occur during unbinding. Sets BindingErrors collection and
        /// and writes out validation error display to the page if specified
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="ex"></param>
        private void HandleUnbindingError(DataBindingItem Item, Exception ex)
        {
            Item.IsBindingError = true;
            string DerivedUserFieldName = string.Empty;

            // Display Error info by setting BindingErrorMessage property
            try
            {
                string ErrorMessage = null;
                DerivedUserFieldName = string.Empty;    
                
                
                // Must check that the control exists - if invalid ID was
                // passed there may not be an instance!
                if (Item.ControlInstance == null)
                    ErrorMessage = Resources.InvalidControl + ": " + Item.ControlId;
                else
                {
                    DerivedUserFieldName = DeriveUserFieldName(Item);                    
                    if (ex is RequiredFieldException)
                    {
                        ErrorMessage = string.Format(this.IsRequiredErrorMessage, DerivedUserFieldName);
                    }
                    else if (ex is ValidationErrorException)
                    {
                        /// Binding Error Message will be set
                        ErrorMessage = ex.Message;
                    }
                    // Explicit error message returned
                    else if (ex is BindingErrorException)
                    {
                        ErrorMessage = ex.Message + " (" + DerivedUserFieldName + ")";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Item.BindingErrorMessage))
                            ErrorMessage = string.Format(this.UnBindingErrorMessage, DerivedUserFieldName);
                        else
                            // Control has a pre-assigned error message
                            ErrorMessage = Item.BindingErrorMessage;
                    }
                }
                AddBindingError(ErrorMessage, Item);
            }
            catch (Exception)
            {
                AddBindingError(string.Format(this.BindingErrorMessage,DerivedUserFieldName), Item);
            }
        }

        /// <summary>
        /// Adds a binding to the control. This method is a simple way to establish a 
        /// binding.
        /// 
        /// Returns the Item so you can customize properties further
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <param name="ControlToBind">
        /// An instance of the control to bind to
        /// </param>
        /// <param name="ControlPropertyToBind">
        /// Property on the control to bind to
        /// </param>
        /// <param name="SourceObjectToBindTo">
        /// An instance of the data item or object that is to be bound
        /// </param>
        /// <param name="SourceMemberToBindTo">
        /// The name of the property to bind on the data item or object
        /// </param>
        public DataBindingItem AddBinding(Control ControlToBind, string ControlPropertyToBind,
                          object SourceObjectToBindTo, string SourceMemberToBindTo)
        {
            DataBindingItem Item = new DataBindingItem(this);

            Item.ControlInstance = ControlToBind;
            Item.ControlId = ControlToBind.ID;
            Item.BindingSourceObject = SourceObjectToBindTo;
            Item.BindingSourceMember = SourceMemberToBindTo;

            DataBindingItems.Add(Item);

            return Item;
        }

        /// <summary>
        /// Adds a binding to the control. This method is a simple way to establish a 
        /// binding.
        /// 
        /// Returns the Item so you can customize properties further
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <param name="ControlToBind">
        /// An instance of a control that is to be bound
        /// </param>
        /// <param name="ControlPropertyToBind">
        /// The property on the control to bind to
        /// </param>
        /// <param name="SourceObjectNameToBindTo">
        /// The name of a data item or object to bind to.
        /// </param>
        /// <param name="SourceMemberToBindTo">
        /// The name of the property on the object to bind to
        /// </param>
        public DataBindingItem AddBinding(Control ControlToBind, string ControlPropertyToBind,
                          string SourceObjectNameToBindTo, string SourceMemberToBindTo)
        {
            DataBindingItem Item = new DataBindingItem(this);

            Item.ControlInstance = ControlToBind;
            Item.ControlId = ControlToBind.ID;
            Item.Page = Page;
            Item.BindingSource = SourceObjectNameToBindTo;
            Item.BindingSourceMember = SourceMemberToBindTo;

            DataBindingItems.Add(Item);

            return Item;
        }

        /// <summary>
        /// This method only adds a data binding item, but doesn't bind it to anything.
        ///  This can be useful for only displaying errors
        /// <seealso>Class DataBinder</seealso>
        /// </summary>
        /// <param name="ControlToBind">
        /// An instance of the control to bind to
        /// </param>
        public DataBindingItem AddBinding(Control ControlToBind)
        {
            DataBindingItem Item = new DataBindingItem(this);

            Item.ControlInstance = ControlToBind;
            Item.ControlId = ControlToBind.ID;
            Item.Page = Page;

            DataBindingItems.Add(Item);

            return Item;
        }

        /// <summary>
        /// Adds a binding error message to a specific control attached to this binder
        /// BindingErrors collection.
        /// </summary>
        /// <param name="ControlName">Form relative Name (ID) of the control to set the error on</param>
        /// <param name="ErrorMessage">The Error Message to set it to.</param>
        /// <returns>true if the control was found. False if not found, but message is still assigned</returns>
        public bool AddBindingError(string ErrorMessage, string ControlName)
        {
            DataBindingItem DataBindingItem = null;

            foreach (DataBindingItem Ctl in DataBindingItems)
            {
                if (Ctl.ControlId == ControlName)
                {
                    DataBindingItem = Ctl;
                    break;
                }
            }

            if (DataBindingItem == null)
            {
                BindingErrors.Add(new BindingError(ErrorMessage));
                return false;
            }

            return AddBindingError(ErrorMessage, DataBindingItem);
        }

        /// <summary>
        /// Adds a binding error to the collection of binding errors.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        /// <param name="control"></param>
        /// <returns>false if the control was not able to get a control reference to attach hotlinks and an icon. Error message always gets added</returns>
        public bool AddBindingError(string ErrorMessage, Control Control)
        {
            DataBindingItem DataBindingItem = null;

            if (Control == null)
            {
                BindingErrors.Add(new BindingError(ErrorMessage));
                return false;
            }

            foreach (DataBindingItem Ctl in DataBindingItems)
            {
                if (Ctl.ControlId == Control.ID)
                {
                    Ctl.ControlInstance = Control;
                    DataBindingItem = Ctl;
                    break;
                }
            }

            // No associated control found - just add the error message
            if (DataBindingItem == null)
            {
                BindingErrors.Add(new BindingError(ErrorMessage, Control.ClientID));
                return false;
            }

            return AddBindingError(ErrorMessage, DataBindingItem);
        }

        /// <summary>
        /// Adds a binding error for DataBindingItem control. This is the most efficient
        /// way to add a BindingError. The other overloads call into this method after
        /// looking up the Control in the DataBinder.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        /// <param name="BindingItem"></param>
        /// <returns></returns>
        public bool AddBindingError(string ErrorMessage, DataBindingItem BindingItem)
        {

            // Associated control found - add icon and link id
            if (BindingItem.ControlInstance != null)
                BindingErrors.Add(new BindingError(ErrorMessage, BindingItem.ControlInstance.ClientID));
            else
            {
                // Just set the error message
                BindingErrors.Add(new BindingError(ErrorMessage));
                return false;
            }

            BindingItem.BindingErrorMessage = ErrorMessage;

            // Insert the error text/icon as a literal
            if (ShowBindingErrorsOnControls && BindingItem.ControlInstance != null)
            {
                // Retrieve the Html Markup for the error
                // NOTE: If script code injection is enabled this is done with client
                //       script code to avoid Controls.Add() functionality which may not
                //       always work reliably if <%= %> tags are in document. Script HTML injection
                //       is the preferred behavior as it should work on any page. If script is used
                //       the message returned is blank and the startup script is embedded instead
                string HtmlMarkup = GetBindingErrorMessageHtml(BindingItem);

                if (!string.IsNullOrEmpty(HtmlMarkup))
                {
                    LiteralControl Literal = new LiteralControl();
                    Control Parent = BindingItem.ControlInstance.Parent;

                    int CtlIdx = Parent.Controls.IndexOf(BindingItem.ControlInstance);
                    try
                    {
                        // Can't add controls to the Control collection if <%= %> tags are on the page
                        Parent.Controls.AddAt(CtlIdx + 1, Literal);
                    }
                    catch { ; }
                }
            }

            return true;
        }


        /// <summary>
        /// Takes a collection of ValidationErrors and assigns it to the
        /// matching controls. The IList object should have Message and
        /// ControlID (optional) properties.
        /// 
        /// These controls must match in signature as follows:
        /// Must have the same name as the field and a 3 letter prefix. For example,
        /// 
        /// txtCompany - matches company field
        /// cmbCountry - matches the Country field
        /// 
        /// The input parameter is a generic IList value, but the type should be
        /// specifically Westwind.BusinessObjects.ValidationErrorCollection. The
        /// generic parameter is used here to avoid an assembly dependence.
        /// </summary>        
        /// <param name="errors">List of objects that have at least ControlID and Message properties</param>
        public void AddValidationErrorsToBindingErrors(IList errors)
        {            
            if (errors == null)
                return;

            foreach (object error in errors)
            {
                // use Reflection to retrieve the values to avoid dependency on ValidationErrors object                
                string message = ReflectionUtils.GetProperty(error, "Message") as string;
                if (message == null)
                    continue;
                
                string controlID = ReflectionUtils.GetProperty(error, "ControlID") as string;                
                
                Control ctl = WebUtils.FindControlRecursive(Page.Form, controlID);
                AddBindingError(message, ctl);
            }
        }

        /// <summary>
        /// Picks up all controls on the form that implement the IDataBinder interface
        /// and adds them to the DataBindingItems Collection
        /// </summary>
        /// <param name="Container"></param>
        /// <returns></returns>
        public void LoadFromControls(Control Container)
        {
            // Only allow loading of controls implicitly once
            if (_AutoLoadedDataBoundControls)
                return;
            _AutoLoadedDataBoundControls = true;

            LoadDataBoundControls(Container);
        }

        /// <summary>
        /// Loop through all of the contained controls of the form and
        /// check for all that implement IDataBinder. If found
        /// add the BindingItem to this Databinder
        /// </summary>
        /// <param name="Container"></param>
        private void LoadDataBoundControls(Control Container)
        {
            foreach (Control Ctl in Container.Controls)
            {
                // ** Recursively call down into any containers
                if (Ctl.Controls.Count > 0)
                    LoadDataBoundControls(Ctl);

                if (Ctl is IDataBinder)
                    DataBindingItems.Add(((IDataBinder)Ctl).BindingItem);
            }
        }

        /// <summary>
        /// Returns a UserField name. Returns UserFieldname if set, or if not
        /// attempts to derive the name based on the field.
        /// </summary>
        /// <param name="Control"></param>
        /// <returns></returns>
        protected string DeriveUserFieldName(DataBindingItem Item)
        {
            if (!string.IsNullOrEmpty(Item.UserFieldName))
                return Item.UserFieldName;

            string ControlID = Item.ControlInstance.ID;

            // Try to get a name by stripping of control prefixes
            string ControlName = Regex.Replace(Item.ControlInstance.ID, "^txt|^chk|^lst|^rad|", "", RegexOptions.IgnoreCase);
            if (ControlName != ControlID)
                return ControlName;

            // Nope - use the default ID
            return ControlID;
        }


        // <summary>
        /// Creates the text for binding error messages based on the 
        /// BindingErrorMessage property of a data bound control.
        /// 
        /// If set the control calls this method render the error message. Called by 
        /// the various controls to generate the error HTML based on the <see>Enum 
        /// ErrorMessageLocations</see>.
        /// 
        /// If UseClientScriptHtmlInjection is set the error message is injected
        /// purely through a client script JavaScript function which avoids problems
        /// with Controls.Add() when script tags are present in the container.
        /// <seealso>Class wwWebDataHelper</seealso>
        /// </summary>
        /// <param name="control">
        /// Instance of the control that has an error.
        /// </param>
        /// <returns>String</returns>
        internal string GetBindingErrorMessageHtml(DataBindingItem Item)
        {
            string Image = null;
            if (string.IsNullOrEmpty(ErrorIconUrl) || ErrorIconUrl == "WebResource")
                Image = ErrorIconWebResource;
            else
                Image = ResolveClientUrl(ErrorIconUrl);

            string Message = "";

            if (Item.ErrorMessageLocation == BindingErrorMessageLocations.WarningIconRight)
                Message = string.Format("&nbsp;<img src=\"{0}\" title=\"{1}\" />", Image, Item.BindingErrorMessage);
            else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.RedTextBelow)
                Message = "<br /><span style=\"color:red;\"><smaller>" + Item.BindingErrorMessage + "</smaller></span>";
            else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.RedTextAndIconBelow)
                Message = string.Format("<br /><img src=\"{0}\" title=\"{1}\"> <span style=\"color:red;\" /><smaller>{1}</smaller></span>", Image, Item.BindingErrorMessage);
            else if (Item.ErrorMessageLocation == BindingErrorMessageLocations.None)
                Message = "";
            else
                Message = "<span style='color:red;font-weight:bold;'> * </span>";

            
            // Use Client Side JavaScript to inject the message rather than adding a control
            if (UseClientScriptHtmlInjection && Item.ControlInstance != null)
            {
                // Fix up to a JSON string for straight embedding
                Message = WebUtils.EncodeJsString(Message);

                if (!_ClientScriptInjectionScriptAdded)
                    AddScriptForAddHtmlAfterControl();

                ClientScriptProxy.RegisterStartupScript(this,GetType(), Item.ControlId,
                    string.Format("AddHtmlAfterControl(\"{0}\",{1});\r\n", Item.ControlInstance.ClientID, Message), true);

                // Message is handled in script so nothing else to write
                Message = "";
            }


            // Message will be embedded with a Literal Control
            return Message;
        }

        /// <summary>
        /// This method adds the static script to handle injecting the warning icon/messages 
        /// into the page as literal strings.
        /// </summary>
        private void AddScriptForAddHtmlAfterControl()
        {
            ClientScriptProxy.RegisterClientScriptBlock(this,GetType(), "AddHtmlAfterControl",
         @"function AddHtmlAfterControl(ControlId,HtmlMarkup)
{
var Ctl = document.getElementById(ControlId);
if (Ctl == null)
 return;
 
var Insert = document.createElement('span');
Insert.innerHTML = HtmlMarkup;

var Sibling = Ctl.nextSibling;
if (Sibling != null)
 Ctl.parentNode.insertBefore(Insert,Sibling);
else
 Ctl.parentNode.appendChild(Insert);
}", true);

        }

        /// <summary>
        /// Fires the ValidateControlEvent
        /// </summary>
        /// <param name="Item"></param>
        /// <returns>false - Validation for control failed and a BindingError is added, true - Validation succeeded</returns>
        public bool OnValidateControl(DataBindingItem Item)
        {
            if (ValidateControl != null && !ValidateControl(Item))
                return false;

            return true;
        }

        /// <summary>
        /// Fires the BeforeUnbindControl event
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public bool OnBeforeUnbindControl(DataBindingItem Item)
        {            
            if (BeforeUnbindControl != null)
                return BeforeUnbindControl(Item);

            return true;
        }

        #region IExtenderProvider Members

        /// <summary>
        /// Determines whether a control can be extended. Basically
        /// we allow ANYTHING to be extended so all controls except
        /// the databinder itself are extendable.
        /// 
        /// Optionally the control can be set up to not act as 
        /// an extender in which case the IsExtender property 
        /// can be set to false
        /// </summary>
        /// <param name="extendee"></param>
        /// <returns></returns>
        public bool CanExtend(object extendee)
        {
            if (!IsExtender)
                return false;

            // Don't extend ourself <g>
            if (extendee is DataBinder)
                return false;

            if (extendee is Control)
                return true;

            return false;
        }

        /// <summary>
        /// Returns a specific DataBinding Item for a given control.
        /// Always returns an item even if the Control is not found.
        /// If you need to check whether this is a valid item check
        /// the BindingSource property for being blank.
        /// 
        /// Extender Property Get method
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public DataBindingItem GetDataBindingItem(Control control)
        {
            foreach (DataBindingItem Item in DataBindingItems)
            {
                if (Item.ControlId == control.ID)
                {
                    // Ensure the binder is set on the item
                    Item.Binder = this;
                    return Item;
                }
            }

            DataBindingItem NewItem = new DataBindingItem(this);
            NewItem.ControlId = control.ID;
            NewItem.ControlInstance = control;

            if (control is ListControl)
                NewItem.BindingProperty = "SelectedValue";
            else if (control is CheckBox)
                NewItem.BindingProperty = "Checked";

            DataBindingItems.Add(NewItem);

            return NewItem;
        }

        /// <summary>
        /// Return a specific databinding item for a give control id.
        /// Note unlike the ControlInstance version return null if the
        /// ControlId isn't found. 
        /// </summary>
        /// <param name="ControlId"></param>
        /// <returns></returns>
        public DataBindingItem GetDataBindingItem(string ControlId)
        {
            for (int i = 0; i < DataBindingItems.Count; i++)
            {
                if (DataBindingItems[i].ControlId == ControlId)
                    return DataBindingItems[i];
            }

            return null;
        }

        /// <summary>
        /// This is never fired in ASP.NET runtime code
        /// </summary>
        /// <param name="extendee"></param>
        /// <param name="Item"></param>
        //public void SetDataBindingItem(object extendee, object Item)
        //{
        //   DataBindingItem BindingItem = Item as DataBindingItem;


        //    Control Ctl = extendee as Control;

        //    HttpContext.Current.Response.Write("SetDataBindingItem fired " + BindingItem.ControlId);
        //}

        /// <summary>
        /// this method is used to ensure that designer is notified
        /// every time there is a change in the sub-ordinate validators
        /// </summary>
        internal void NotifyDesigner()
        {
            if (DesignMode)
            {
                IDesignerHost Host = Site.Container as IDesignerHost;
                ControlDesigner Designer = Host.GetDesigner(this) as ControlDesigner;
                PropertyDescriptor Descriptor = null;
                try
                {
                    Descriptor = TypeDescriptor.GetProperties(this)["DataBindingItems"];
                }
                catch
                {
                    return;
                }

                ComponentChangedEventArgs ccea = new ComponentChangedEventArgs(
                            this,
                            Descriptor,
                            null,
                            DataBindingItems);
                Designer.OnComponentChanged(this, ccea);
            }
        }


        #endregion
    }

    public delegate bool delItemResultNotification(DataBindingItem Item);

    public delegate void delItemNotification(DataBindingItem Item);    

    public delegate void delDataBindingItemValidate(object sender, DataBindingValidationEventArgs e);


    /// <summary>
    /// Control designer used so we get a grey button display instead of the 
    /// default label display for the control.
    /// </summary>
    internal class DataBinderDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml("Control Extender");
        }
    }
}
