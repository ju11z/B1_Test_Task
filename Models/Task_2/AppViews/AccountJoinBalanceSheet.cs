using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Models.Task_2.DBViews
{
    public class AccountJoinBalanceSheet
    {
        public int AccCode { get; set; }
        public double IncomingBalanceAsset { get; set; }

        public double IncomingBalanceLiability { get; set; }
        public double TurnoverDebet { get; set; }
        public double TurnoverCredit { get; set; }

        public double OutgoingBalanceAsset { get; set; }
        public double OutgoingBalanceLiability { get; set; }

        public bool IsCorrect { get; set; }
    }
}
