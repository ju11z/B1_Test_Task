using B1_Test_Task.Models.Task_2.DBViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace B1_Test_Task.Data
{
    public class Task2ContextRepository
    {
        
        private Task2Context context;
        public Task2ContextRepository()
        {
            context = new Task2Context();
        }

        private bool BalanceSheetIsCorrect(double IncomingBalanceAsset, double IncomingBalanceLiability, double TurnoverDebet, double TurnoverCredit, double OutgoingBalanceAsset, double OutgoingBalanceLiability)
        {
           
            if(IncomingBalanceAsset!=0 && IncomingBalanceLiability == 0
                && Math.Round(IncomingBalanceAsset + TurnoverDebet - TurnoverCredit, 3)==Math.Round(OutgoingBalanceAsset, 3))
                return true;
            
            if (IncomingBalanceAsset == 0 && IncomingBalanceLiability != 0
                && Math.Round(IncomingBalanceLiability - TurnoverDebet + TurnoverCredit, 3) == Math.Round(OutgoingBalanceLiability, 3))
                return true;
            if (IncomingBalanceAsset != 0 && IncomingBalanceLiability != 0
                && Math.Round(IncomingBalanceAsset + TurnoverDebet - TurnoverCredit, 3) == Math.Round(OutgoingBalanceAsset, 3)
                && Math.Round(IncomingBalanceLiability - TurnoverDebet + TurnoverCredit, 3) == Math.Round(OutgoingBalanceLiability, 3))
                return true;

                return false;
            
        }

        public List<AccountJoinBalanceSheet>  GetAccountInnerJoinBalanceSheet(int statementId)
        {
            var data = from a in context.Accounts
                              join b in context.BalanceSheets
                              on a.Id equals b.AccountId
                              where b.StatementId==statementId
                              select new AccountJoinBalanceSheet
                              {
                                  AccCode = a.Code,
                                  IncomingBalanceAsset = b.IncomingBalanceAsset,
                                  IncomingBalanceLiability=b.IncomingBalanceLiability,
                                  TurnoverDebet=b.TurnoverDebet,
                                  TurnoverCredit=b.TurnoverCredit,
                                  OutgoingBalanceAsset=b.OutgoingBalanceAsset,
                                  OutgoingBalanceLiability=b.OutgoingBalanceLiability
                                  
                              };

            List<AccountJoinBalanceSheet> res = new List<AccountJoinBalanceSheet>(data.ToList());
            
            foreach(var entry in res)
            {
                entry.IsCorrect = BalanceSheetIsCorrect(entry.IncomingBalanceAsset, entry.IncomingBalanceLiability, entry.TurnoverDebet, entry.TurnoverCredit, entry.OutgoingBalanceAsset, entry.OutgoingBalanceLiability);
            }

            return res;

        }
    }
}
