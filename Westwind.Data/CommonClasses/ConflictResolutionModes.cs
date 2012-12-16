using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Westwind.Data
{

    /// <summary>
    /// Determines how conflicts on SubmitChanges are handled.
    /// </summary>
    public enum ConflictResolutionModes
    {
        /// <summary>
        /// No Conflict resolution - nothing is done when conflicts
        /// occur. You can check Context.ChangeConflicts manually
        /// </summary>
        None,
        /// <summary>
        /// Forces all changes to get written. Last one wins strategy
        /// </summary>
        ForceChanges,
        /// <summary>
        /// Aborts all changes and updates the entities with the values
        /// from the database.
        /// </summary>
        AbortChanges,
        /// <summary>
        /// Writes all changes that are not in conflict. Updates entities
        /// with values from the data.
        /// </summary>
        //WriteNonConflictChanges
    }

}
