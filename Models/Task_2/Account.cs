using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Models.Task_2
{
    public class Account
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        //public int StatementId { get; set; }
        //public Statement Statement { get; set; }

        public string Value { get; set; }
    }
}
