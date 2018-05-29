namespace K9.DataAccessLayer.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DropIbogaColumns : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Donation", "NumberOfIbogas");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Donation", "NumberOfIbogas", c => c.Int(nullable: false));
        }
    }
}
