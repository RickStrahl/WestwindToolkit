namespace Westwind.Web
{
    public enum ErrorHandlingModes
    {
        /// <summary>
        /// Default error display mode uses ASP.NET default
        /// behavior: Yellow screen of death or ASP.NET 
        /// error display configured.
        /// </summary>
        Default,
        /// <summary>
        /// An application specific error message meant to 
        /// be seen by end users. Typically this mode display
        /// only a fixed error message and no or limited
        /// error details.
        /// </summary>
        ApplicationErrorMessage,
        /// <summary>
        /// Display the application specific error message
        /// but also adds additional developer information 
        /// that provides debugging information to a developer
        /// while the app is running in production.
        /// </summary>
        DeveloperErrorMessage
    }
}
