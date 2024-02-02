using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_2;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace B1_Test_Task.Services
{
    
    public class ExcelFileService
    {

        public Action<int> RowImportedToDB;
        private const int IMPORT_BLOCK_SIZE = 10;

        private const int ACCOUNT_DATA_START_ROW = 9;
        private const int ACCOUNT_START_ROW = 8;

        private const int ACCOUNT_COLUMN = 0;
        private const int INCOMING_BALANCE_ASSET_COLUMN = 1;
        private const int INCOMING_BALANCE_LIABILITY_COLUMN = 2;
        private const int TURNOVER_DEBET_COLUMN = 3;
        private const int TURNOVER_CREDIT_COLUMN = 4;
        private const int OUTGOING_BALANCE_ASSET_COLUMN = 5;
        private const int OUTGOING_BALANCE_LIABILITY_COLUMN = 6;

        public async Task ImportAccountsToDB(Task2Context context, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workBook = new HSSFWorkbook(fileStream); // For .xls files, use HSSFWorkbook

                ISheet sheet = workBook.GetSheetAt(0); // Assuming you want to read the first sheet in the Excel file

                await ImportFirstLevelAccountToDB(sheet, context);

                await ImportSecondLevelAccountToDB(sheet, context);

                await ImportThirdLevelAccountToDB(sheet, context);

            }
        }

        

        private async Task ImportFirstLevelAccountToDB(ISheet sheet, Task2Context context)
        {
            await Task.Run(async () =>
            {

                for (int i = ACCOUNT_START_ROW; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);

                    if (row != null)
                    {
                        ICell cell = sheet.GetRow(i).GetCell(0);
                        DataFormatter dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);
                        string value = dataFormatter.FormatCellValue(cell);
                        //string value = row.GetCell(ACCOUNT_COLUMN).NumericCellValue.ToString();
                        if (value.Contains("КЛАСС") && !value.Contains("КЛАССУ"))
                        {
                            Account entity = new Account
                            {
                                ParentId = 0,
                                Value = value
                            };

                            context.Accounts.Add(entity);
                        }
                    }
                }

                await context.SaveChangesAsync();
            });
            

        }

        private int FindFirstLevelParentId(string childValue, List<Account> parents)
        {
            int parentId = 0;

            Account account = parents.Find(p => p.Value.Contains(childValue[0]));

            if (account != null)
                return account.Id;

            return 0;

        }

        private int FindSecondLevelParentId(string childValue, List<Account> parents)
        {
            int parentId = 0;

            Account account = parents.Find(p => p.Value.Substring(0,2)==childValue.Substring(0, 2));

            if (account != null)
                return account.Id;

            return 0;

        }

        private async Task ImportSecondLevelAccountToDB(ISheet sheet, Task2Context context)
        {
            
            string pattern = @"\b\d{2}\b";
            Regex regex = new Regex(pattern);

            List<Account> parents = context.Accounts.ToList();

            await Task.Run(async () =>
            {
                for (int i = ACCOUNT_START_ROW; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);

                    if (row != null)
                    {
                        ICell cell = sheet.GetRow(i).GetCell(0);
                        DataFormatter dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);
                        string value = dataFormatter.FormatCellValue(cell);
                        //string value = row.GetCell(ACCOUNT_COLUMN).NumericCellValue.ToString();
                        MatchCollection matches = regex.Matches(value);
                        if (matches.Count==1)
                        {
                            
                            int parentId = FindFirstLevelParentId(value, parents);
                            Account entity = new Account
                            {
                                ParentId = parentId,
                                Value = value
                            };

                            context.Accounts.Add(entity);
                        }
                    }
                }

                
            });
            
            await context.SaveChangesAsync();
        }

        private async Task ImportThirdLevelAccountToDB(ISheet sheet, Task2Context context)
        {

            string pattern = @"\b\d{4}\b";
            Regex regex = new Regex(pattern);

            List<Account> parents = context.Accounts.ToList();

            await Task.Run(async () =>
            {
                for (int i = ACCOUNT_START_ROW; i <= sheet.LastRowNum; i++)
                {
                    IRow row = sheet.GetRow(i);

                    if (row != null)
                    {
                        ICell cell = sheet.GetRow(i).GetCell(0);
                        DataFormatter dataFormatter = new DataFormatter(CultureInfo.CurrentCulture);
                        string value = dataFormatter.FormatCellValue(cell);
                        //string value = row.GetCell(ACCOUNT_COLUMN).NumericCellValue.ToString();
                        MatchCollection matches = regex.Matches(value);
                        if (matches.Count == 1)
                        {

                            int parentId = FindSecondLevelParentId(value, parents);
                            Account entity = new Account
                            {
                                ParentId = parentId,
                                Value = value
                            };

                            context.Accounts.Add(entity);
                        }
                    }
                }


            });

            await context.SaveChangesAsync();
        }



        public async Task ImportAccountDataToDB(Task2Context context ,string filePath)
        {
            //List<AccountData> entities = new List<AccountData>();
            List<AccountData> block = new List<AccountData>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workBook = new HSSFWorkbook(fileStream); // For .xls files, use HSSFWorkbook

                ISheet sheet = workBook.GetSheetAt(0); // Assuming you want to read the first sheet in the Excel file


                await Task.Run(async() =>
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
                            await context.SaveChangesAsync();
                            block.Clear();
                        }
                    }
                });
            }
        }
    }
}
