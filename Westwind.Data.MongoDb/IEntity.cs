
namespace Westwind.Data.MongoDb
{
    /// <summary>
    /// Marker interface for Mongo Entities that ensures that
    /// each entity has an Id property    
    /// </summary>
    public interface IEntity
    {
        string Id { get; set; }
    }
}
