using System;

namespace Westwind.Data
{
    /// <summary>
    /// Contains public options that can be set to affect how
    /// the business object operates
    /// </summary>
    public class BusinessObjectOptions
    {
        /// <summary>
        /// Determines whether exceptions are thrown on errors
        /// or whether error messages are merely set.
        /// </summary>
        public bool ThrowExceptions = false;

        /// <summary>
        /// Determines how LINQ is used for object tracking. 
        /// 
        /// In connected mode all changes are tracked until SubmitChanges or Save
        /// is called. Save() reverts to calling SubmitChanges.
        /// 
        /// In disconnected mode a new context is created for each data operation
        /// and save uses Attach to reattach to a context.
        /// 
        /// Use Connected for much better performance use disconnected if you
        /// prefer atomic operations in the database with individual entities.
        /// </summary>
        public TrackingModes TrackingMode = TrackingModes.Connected;

        /// <summary>
        /// Optional Connection string that is used with the data context
        /// 
        /// Note: This property should be set in the constructor/Initialize of the
        /// business object. 
        /// 
        /// If blank the default context connection string is used.
        /// </summary>
        public string ConnectionString = "";


        /// <summary>
        /// Determines the default Conflict Resolution mode for changes submitted
        /// to the context.
        /// </summary>
        public ConflictResolutionModes ConflictResolutionMode = ConflictResolutionModes.ForceChanges;
    }
}
