using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Models.Task_2
{
    public class BalanceSheet
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int StatementId { get; set; }
        public Statement Statement { get; set; }

        public double IncomingBalanceAsset { get; set; }
        public double IncomingBalanceLiability { get; set; }
        public double TurnoverDebet { get; set; }
        public double TurnoverCredit { get; set; }

        public double OutgoingBalanceAsset { get; set; }
        public double OutgoingBalanceLiability { get; set; }
    }
}
