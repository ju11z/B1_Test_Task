using B1_Test_Task.Models.Task_1;
using System;
using System.Data.Entity;
using System.Linq;

namespace B1_Test_Task.Data
{
    public class Task1Context : DbContext
    {
        public DbSet<Row> Rows { get; set; }
        // Your context has been configured to use a 'Task1Context' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'B1_Test_Task.Data.Task1Context' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'Task1Context' 
        // connection string in the application configuration file.
        public Task1Context()
            : base("name=Task1Context")
        {
            //Database.SetInitializer(new DropCreateDatabaseAlways<Task1Context>());
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