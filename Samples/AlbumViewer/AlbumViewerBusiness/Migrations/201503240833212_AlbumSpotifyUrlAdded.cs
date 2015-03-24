namespace AlbumViewerBusiness.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlbumSpotifyUrlAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Albums", "SpotifyUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Albums", "SpotifyUrl");
        }
    }
}
