using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_2;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Services
{
    
    public class ExcelFileService
    {

        public Action<int> RowImportedToDB;
        private const int IMPORT_BLOCK_SIZE = 10;

        private const int ACCOUNT_DATA_START_ROW = 9;

        private const int INCOMING_BALANCE_ASSET_COLUMN = 1;
        private const int INCOMING_BALANCE_LIABILITY_COLUMN = 2;
        private const int TURNOVER_DEBET_COLUMN = 3;
        private const int TURNOVER_CREDIT_COLUMN = 4;
        private const int OUTGOING_BALANCE_ASSET_COLUMN = 5;
        private const int OUTGOING_BALANCE_LIABILITY_COLUMN = 6;

        public async Task ImportDataToDB(Task2Context context ,string filePath)
        {
            //List<AccountData> entities = new List<AccountData>();
            List<AccountData> block = new List<AccountData>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workBook = new HSSFWorkbook(fileStream); // For .xls files, use HSSFWorkbook

                ISheet sheet = workBook.GetSheetAt(0); // Assuming you want to read the first sheet in the Excel file


                await Task.Run(() =>
                {

                    for (int i = ACCOUNT_DATA_START_ROW; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);

                        if (row != null) // null cell values may cause null reference exceptions
                        {
                            AccountData entity = new AccountData
                            {
                                IncomingBalanceAsset = row.GetCell(INCOMING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                IncomingBalanceLiability = row.GetCell(INCOMING_BALANCE_LIABILITY_COLUMN).NumericCellValue,
                                TurnoverDebet= row.GetCell(TURNOVER_DEBET_COLUMN).NumericCellValue,
                                TurnoverCredit= row.GetCell(TURNOVER_CREDIT_COLUMN).NumericCellValue,
                                OutgoingBalanceAsset= row.GetCell(OUTGOING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                OutgoingBalanceLiability=row.GetCell(OUTGOING_BALANCE_LIABILITY_COLUMN).NumericCellValue
                            };

                            block.Add(entity);
                        }

                        if (i % IMPORT_BLOCK_SIZE == 0)
                        {
                            RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                            context.AccountData.AddRange(block);
                            context.SaveChanges();
                            block.Clear();
                        }
                    }
                });
            }
        }
    }
}
