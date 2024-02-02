namespace B1_Test_Task.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(nullable: false),
                        StatementId = c.Int(nullable: false),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Statements", t => t.StatementId, cascadeDelete: true)
                .Index(t => t.StatementId);
            
            CreateTable(
                "dbo.Statements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodStart = c.DateTime(nullable: false),
                        PeriodEnd = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AccountDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        IncomingBalanceAsset = c.Double(nullable: false),
                        IncomingBalanceLiability = c.Double(nullable: false),
                        TurnoverDebet = c.Double(nullable: false),
                        TurnoverCredit = c.Double(nullable: false),
                        OutgoingBalanceAsset = c.Double(nullable: false),
                        OutgoingBalanceLiability = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccountDatas", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Accounts", "StatementId", "dbo.Statements");
            DropIndex("dbo.AccountDatas", new[] { "AccountId" });
            DropIndex("dbo.Accounts", new[] { "StatementId" });
            DropTable("dbo.AccountDatas");
            DropTable("dbo.Statements");
            DropTable("dbo.Accounts");
        }
    }
}
