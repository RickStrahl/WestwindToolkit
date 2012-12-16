using System;

namespace Westwind.Data
{
    /// <summary>
    /// Determines how LINQ Change Tracking is applied
    /// </summary>
    public enum TrackingModes
    {
        /// <summary>
        /// Uses a LINQ connected data context for change management
        /// whenever possible. Save and SubmitChanges operation is used
        /// to persist changes. In general this provides better performance
        /// for change tracking.
        /// </summary>
        Connected,

        /// <summary>
        /// Creates a new DataContext for each operation and performs .Save 
        /// operations by reconnecting to the DataContext.
        /// </summary>
        Disconnected
    }
}
