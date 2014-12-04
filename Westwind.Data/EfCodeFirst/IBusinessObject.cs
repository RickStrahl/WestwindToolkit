using System;

namespace Westwind.Data.EfCodeFirst
{
    /// <summary>
    /// Marker interface for business objects and so we have access to
    /// DbContext instance.
    /// </summary>
    public interface IBusinessObject<TContext>
    {
        TContext Context { get; set; }
    }
}
