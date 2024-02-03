using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Models.Task_2
{
    public class Statement
    {
        public int Id { get; set; }

        public string BankTitle { get; set; }

        public string StatementTitle { get; set; }
        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }
    }
}
