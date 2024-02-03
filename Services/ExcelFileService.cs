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

        private const int STATEMENT_BANK_TITLE_ROW = 0;
        private const int STATEMENT_BANK_TITLE_COLUMN = 1;
        private const int STATEMENT_TITLE_ROW = 1;
        private const int STATEMENT_PERIOD_ROW = 2;

        private IWorkbook workBoook;

        public async Task ImportAccountsToDB(Task2Context context, string filePath)
        {
            ISheet sheet = workBoook.GetSheetAt(0);

            await ImportFirstLevelAccountToDB(sheet, context);

                await ImportSecondLevelAccountToDB(sheet, context);

                await ImportThirdLevelAccountToDB(sheet, context);
                
        }

        public async Task<string> ImportDataToDB(Task2Context context, string filePath)
        {
            InitWorkBook(filePath);

            ISheet sheet = workBoook.GetSheetAt(0); // Assuming you want to read the first sheet in the Excel file

            try
            {
                bool validate = ValidateSheet(sheet, filePath);
            }
            catch (Exception e)
            {
                ExceptionNotifier.NotifyAboutException(e.Message);
                return $" error occured while importing {filePath}";
            }

            ImportStatementToDB(sheet, context);
            var importaccountstodb = ImportAccountsToDB(context, filePath);
            await importaccountstodb;
            var importaccountdatatodb = ImportAccountDataToDB(context, filePath);
            await importaccountdatatodb;

            //string importResult = $"{filePath} imported successfully";

            return $"{filePath} imported successfully";
        }

        private void InitWorkBook(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {

                if (fileStream.Name.EndsWith(".xlsx"))
                {
                    workBoook = new XSSFWorkbook(fileStream);
                }
                else
                {
                    workBoook = new HSSFWorkbook(fileStream);
                }
            }
        }

        private void ImportStatementToDB(ISheet sheet, Task2Context context)
        {
            string bankTitle = sheet.GetRow(STATEMENT_BANK_TITLE_ROW).GetCell(STATEMENT_BANK_TITLE_COLUMN).StringCellValue;
            string statementTitle = sheet.GetRow(STATEMENT_TITLE_ROW).GetCell(0).StringCellValue;
            string dateRowValue = sheet.GetRow(STATEMENT_PERIOD_ROW).GetCell(0).StringCellValue;

            string pattern = @"\b\d{2}\.\d{2}\.\d{4}\b";

            MatchCollection dateMatches = Regex.Matches(dateRowValue, pattern);

            string firstDate="";
            string lastDate="";

            if (dateMatches.Count > 1)
            {
                firstDate = dateMatches[0].Value;
                lastDate = dateMatches[1].Value;
            }

            DateTime periodStart= DateTime.ParseExact(firstDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime periodEnd = DateTime.ParseExact(lastDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            Statement statement = new Statement
            {
                BankTitle=bankTitle,
                StatementTitle=statementTitle,
                PeriodStart=periodStart,
                PeriodEnd=periodEnd
            };

            context.Statements.Add(statement);
            context.SaveChanges();

            
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

        private bool ValidateSheet(ISheet sheet, string filePath)
        {
            ICell incoming_balance_asset_column = null;
            ICell incoming_balance_liability_column = null;
            ICell turnover_debet_column = null;
            ICell turnover_credit_column = null;
            ICell outgoing_balance_asset_column = null;
            ICell outgoing_balance_liability_column = null;

            bool result = true;

            try
            {
                IRow row = sheet.GetRow(ACCOUNT_DATA_START_ROW);

                if (row == null)
                    result = false;

                if (!result)
                    throw new Exception($"{filePath} has wrong table structure!");

                incoming_balance_asset_column = row.GetCell(INCOMING_BALANCE_ASSET_COLUMN);
                incoming_balance_liability_column = row.GetCell(INCOMING_BALANCE_LIABILITY_COLUMN);
                turnover_debet_column = row.GetCell(TURNOVER_DEBET_COLUMN);
                turnover_credit_column = row.GetCell(TURNOVER_CREDIT_COLUMN);
                outgoing_balance_asset_column = row.GetCell(OUTGOING_BALANCE_ASSET_COLUMN);
                outgoing_balance_liability_column = row.GetCell(OUTGOING_BALANCE_LIABILITY_COLUMN);



                if (incoming_balance_asset_column == null
                    || incoming_balance_liability_column == null
                    || turnover_debet_column == null
                    || turnover_credit_column == null
                    || outgoing_balance_asset_column == null
                    || outgoing_balance_liability_column == null
                    )
                    result = false;

                if (!result)
                    throw new Exception($"{filePath} has wrong table structure!");

                if (!(incoming_balance_asset_column.CellType == CellType.Numeric
                && incoming_balance_liability_column.CellType == CellType.Numeric
                && turnover_debet_column.CellType == CellType.Numeric
                && turnover_credit_column.CellType == CellType.Numeric
                && outgoing_balance_asset_column.CellType == CellType.Numeric
                && outgoing_balance_liability_column.CellType == CellType.Numeric))
                    result = false;

                if (!result)
                    throw new Exception($"{filePath} has wrong table structure!");

            }
            catch
            {
                throw;
            }

            return result;
            
        }

        public async Task ImportAccountDataToDB(Task2Context context ,string filePath)
        {
            try { 
                List<AccountData> block = new List<AccountData>();

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //IWorkbook workBook = new HSSFWorkbook(fileStream); // For .xls files, use HSSFWorkbook

                    ISheet sheet = workBoook.GetSheetAt(0); // Assuming you want to read the first sheet in the Excel file
                    /*
                    try
                    {
                        bool validate = ValidateSheet(sheet);

                        if(!validate)
                            throw new Exception("Wrong table format!");
                    }
                    catch(Exception e)
                    {
                        ExceptionNotifier.NotifyAboutException(e.Message);
                        return;
                    }
                    */




                    for (int i = ACCOUNT_DATA_START_ROW; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);

                        if (row != null) // null cell values may cause null reference exceptions
                        {
                            AccountData entity = new AccountData
                            {
                                IncomingBalanceAsset = row.GetCell(INCOMING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                IncomingBalanceLiability = row.GetCell(INCOMING_BALANCE_LIABILITY_COLUMN).NumericCellValue,
                                TurnoverDebet = row.GetCell(TURNOVER_DEBET_COLUMN).NumericCellValue,
                                TurnoverCredit = row.GetCell(TURNOVER_CREDIT_COLUMN).NumericCellValue,
                                OutgoingBalanceAsset = row.GetCell(OUTGOING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                OutgoingBalanceLiability = row.GetCell(OUTGOING_BALANCE_LIABILITY_COLUMN).NumericCellValue
                            };

                            block.Add(entity);
                        }

                        if (i % IMPORT_BLOCK_SIZE == 0)
                        {
                            context.AccountData.AddRange(block);
                            await context.SaveChangesAsync();
                            block.Clear();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                return;
            }
        }
    }
}
