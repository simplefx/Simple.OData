namespace Simple.OData.ProductService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Category = c.String(),
                    })
                .PrimaryKey(t => t.ID);

            CreateTable(
                "dbo.WorkTaskModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        State = c.Guid(nullable: false),
                        Location_Latitude = c.Double(nullable: false),
                        Location_Longitude = c.Double(nullable: false),
                        WorkerId = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.WorkTaskAttachmentModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.Guid(nullable: false),
                        FileName = c.String(),
                        WorkTaskId = c.Guid(nullable: false),
                        WorkTaskModel_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkTaskModels", t => t.WorkTaskModel_Id)
                .Index(t => t.WorkTaskModel_Id);

            CreateTable(
                "dbo.WorkActivityReportModels",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Code = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Description = c.String(),
                        Type = c.Guid(nullable: false),
                        Location_Latitude = c.Double(nullable: false),
                        Location_Longitude = c.Double(nullable: false),
                        WorkTaskId = c.Guid(nullable: false),
                        WorkerId = c.Guid(nullable: false),
                        WorkTaskModel_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkTaskModels", t => t.WorkTaskModel_Id)
                .Index(t => t.WorkTaskModel_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkActivityReportModels", "WorkTaskModel_Id", "dbo.WorkTaskModels");
            DropForeignKey("dbo.WorkTaskAttachmentModels", "WorkTaskModel_Id", "dbo.WorkTaskModels");
            DropIndex("dbo.WorkActivityReportModels", new[] { "WorkTaskModel_Id" });
            DropIndex("dbo.WorkTaskAttachmentModels", new[] { "WorkTaskModel_Id" });
            DropTable("dbo.WorkActivityReportModels");
            DropTable("dbo.WorkTaskAttachmentModels");
            DropTable("dbo.WorkTaskModels");
            DropTable("dbo.Products");
        }
    }
}
