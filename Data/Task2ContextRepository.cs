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
        public IQueryable<AccountJoinBalanceSheet>  GetAccountInnerJoinBalanceSheet(int statementId)
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
            /*
             * var studentViewModel = from s in student
                                join st in studentAdditionalInfo on s.Id equals st.StudentId into st2
                                from st in st2.DefaultIfEmpty()
                                select new StudentViewModel { studentVm = s, studentAdditionalInfoVm = st };
             */

            /*
            var data = context.BalanceSheets.Where(b => b.StatementId == statementId)
                .Join(context.Accounts,
                b => b.AccountId,
                a => a.Id,
                (b, a) => new
                {
                    AccountCode = a.Code,
                    C1 = b.IncomingBalanceAsset
                });
            */

            /*
            foreach(var d in data)
            {
                MessageBox.Show($"{d.code}  {d.IncomingBalanceAsset}");
            }
            */

            

            return data;

        }

     
        
    }
}
