namespace AlbumViewerBusiness.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNullableFks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Albums", "ArtistId", "dbo.Artists");
            DropForeignKey("dbo.Tracks", "AlbumId", "dbo.Albums");
            DropIndex("dbo.Albums", new[] { "ArtistId" });
            DropIndex("dbo.Tracks", new[] { "AlbumId" });
            AlterColumn("dbo.Albums", "ArtistId", c => c.Int());
            AlterColumn("dbo.Tracks", "AlbumId", c => c.Int());
            CreateIndex("dbo.Albums", "ArtistId");
            CreateIndex("dbo.Tracks", "AlbumId");
            AddForeignKey("dbo.Albums", "ArtistId", "dbo.Artists", "Id");
            AddForeignKey("dbo.Tracks", "AlbumId", "dbo.Albums", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tracks", "AlbumId", "dbo.Albums");
            DropForeignKey("dbo.Albums", "ArtistId", "dbo.Artists");
            DropIndex("dbo.Tracks", new[] { "AlbumId" });
            DropIndex("dbo.Albums", new[] { "ArtistId" });
            AlterColumn("dbo.Tracks", "AlbumId", c => c.Int(nullable: false));
            AlterColumn("dbo.Albums", "ArtistId", c => c.Int(nullable: false));
            CreateIndex("dbo.Tracks", "AlbumId");
            CreateIndex("dbo.Albums", "ArtistId");
            AddForeignKey("dbo.Tracks", "AlbumId", "dbo.Albums", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Albums", "ArtistId", "dbo.Artists", "Id", cascadeDelete: true);
        }
    }
}
