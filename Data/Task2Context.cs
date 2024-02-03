using B1_Test_Task.Models.Task_2;
using B1_Test_Task.Models.Task_2.DBViews;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace B1_Test_Task.Data
{
    public class Task2Context : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<BalanceSheet> BalanceSheets { get; set; }
        public DbSet<Statement> Statements { get; set; }

        
        // Your context has been configured to use a 'Task2Context' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'B1_Test_Task.Data.Task2Context' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'Task2Context' 
        // connection string in the application configuration file.
        public Task2Context()
            : base("name=Task2Context")
        {
            Database.SetInitializer(new Initializer());
        }
       

        public class Initializer : IDatabaseInitializer<Task2Context>
        {

            
            public void InitializeDatabase(Task2Context context)
            {
                if (context.Database.Exists() && !context.Database.CompatibleWithModel(false))
                    context.Database.Delete();

                if (!context.Database.Exists())
                {
                    context.Database.Create();
                    context.Database.ExecuteSqlCommand("CREATE VIEW [AccountBalance] AS SELECT a.Code, b.IncomingBalanceAsset, b.IncomingBalanceLiability, b.TurnoverDebet, b.TurnoverCredit, b.OutgoingBalanceAsset, b.OutgoingBalanceLiability FROM Accounts a inner join BalanceSheets b on a.Id = b.AccountId");
                    //context.Database.ExecuteSqlCommand("alter table Accounts add constraint AccountCodeUnique unique (Code)");
                }
            }
            
        }
        

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}