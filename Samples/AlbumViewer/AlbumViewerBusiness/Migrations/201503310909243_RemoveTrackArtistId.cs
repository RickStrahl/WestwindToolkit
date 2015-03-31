namespace AlbumViewerBusiness.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTrackArtistId : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Tracks", "ArtistId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tracks", "ArtistId", c => c.Int(nullable: false));
        }
    }
}
