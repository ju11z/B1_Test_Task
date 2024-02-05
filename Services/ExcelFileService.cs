using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_2;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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
        private const int IMPORT_BLOCK_SIZE = 1;

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

        private async Task ImportAccountsToDBAsync(Task2Context context, string filePath)
        {
            ISheet sheet = workBoook.GetSheetAt(0);

            try
            {

                await ImportFirstLevelAccountToDBAsync(sheet, context);

                await ImportSecondLevelAccountToDBAsync(sheet, context);

                await ImportThirdLevelAccountToDBAsync(sheet, context);

            }
            catch (Exception e)
            {
                return;
            }
        }

        public async Task<string> ImportDataToDBAsync(Task2Context context, string filePath)
        {
            try
            {
                InitWorkBook(filePath);
            }
            catch (Exception e)
            {
                return $" error occured while importing {filePath}";
            }


            ISheet sheet = workBoook.GetSheetAt(0);

            try
            {
                bool validate = ValidateSheet(sheet, filePath);
            }
            catch (Exception e)
            {
                return $" error occured while importing {filePath}";
            }

            int statementId = ImportStatementToDB(sheet, context);
            var importaccountstodb = ImportAccountsToDBAsync(context, filePath);
            await importaccountstodb;
            var importaccountdatatodb = ImportBalanceSheetToDBAsync(statementId, context, filePath);
            await importaccountdatatodb;

            return $"{filePath} imported successfully";
        }

        private void InitWorkBook(string filePath)
        {
            try
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
            catch (IOException ex)
            {
                throw;
            }

        }

        private int ImportStatementToDB(ISheet sheet, Task2Context context)
        {
            string bankTitle = sheet.GetRow(STATEMENT_BANK_TITLE_ROW).GetCell(STATEMENT_BANK_TITLE_COLUMN).StringCellValue;
            string statementTitle = sheet.GetRow(STATEMENT_TITLE_ROW).GetCell(0).StringCellValue;
            string dateRowValue = sheet.GetRow(STATEMENT_PERIOD_ROW).GetCell(0).StringCellValue;

            string pattern = @"\b\d{2}\.\d{2}\.\d{4}\b";

            MatchCollection dateMatches = Regex.Matches(dateRowValue, pattern);

            string firstDate = "";
            string lastDate = "";

            if (dateMatches.Count > 1)
            {
                firstDate = dateMatches[0].Value;
                lastDate = dateMatches[1].Value;
            }

            DateTime periodStart = DateTime.ParseExact(firstDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            DateTime periodEnd = DateTime.ParseExact(lastDate, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

            Statement statement = new Statement
            {
                BankTitle = bankTitle,
                StatementTitle = statementTitle,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };

            context.Statements.Add(statement);
            context.SaveChanges();

            return statement.Id;


        }

        private bool AccountCodeIsUnique(int code, Task2Context context)
        {
            return context.Accounts.Where(a => a.Code == code).ToList().Count < 1;
        }


        private async Task ImportFirstLevelAccountToDBAsync(ISheet sheet, Task2Context context)
        {
            string pattern = @"\d+";
            int code = 0;

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
                        if (value.Contains("КЛАСС") && !value.Contains("КЛАССУ"))
                        {
                            Match match = Regex.Match(value, pattern);

                            if (match.Success)
                            {
                                code = Int32.Parse(match.Value);
                            }

                            if (!AccountCodeIsUnique(code, context))
                                continue;

                            Account entity = new Account
                            {
                                ParentId = 0,
                                Code = code,
                                Title = value
                            };

                            context.Accounts.Add(entity);
                            await context.SaveChangesAsync();


                        }
                    }
                }


            });
        }

        private int FindFirstLevelParentId(string childValue, List<Account> parents)
        {
            Account account = parents.Find(p => p.Code.ToString().Contains(childValue[0]));

            if (account != null)
                return account.Id;

            return 0;

        }

        private int FindSecondLevelParentId(string childValue, List<Account> parents)
        {
            List<Account> secondLevelParents = parents.Where(p => p.Code.ToString().Length > 1).ToList();

            Account account = secondLevelParents.Find(p => p.Code.ToString().Substring(0, 2) == childValue.Substring(0, 2));

            if (account != null)
                return account.Id;

            return 0;

        }

        private async Task ImportSecondLevelAccountToDBAsync(ISheet sheet, Task2Context context)
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
                        MatchCollection matches = regex.Matches(value);
                        if (matches.Count == 1)
                        {

                            int parentId = FindFirstLevelParentId(value, parents);
                            int code = Int32.Parse(value);

                            if (!AccountCodeIsUnique(code, context))
                                continue;

                            Account entity = new Account
                            {
                                ParentId = parentId,
                                Code = code
                            };

                            context.Accounts.Add(entity);
                            await context.SaveChangesAsync();

                        }
                    }
                }


            });


        }

        private async Task ImportThirdLevelAccountToDBAsync(ISheet sheet, Task2Context context)
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
                        MatchCollection matches = regex.Matches(value);
                        if (matches.Count == 1)
                        {
                            int code = Int32.Parse(value);

                            if (!AccountCodeIsUnique(code, context))
                                continue;

                            int parentId = FindSecondLevelParentId(value, parents);
                            Account entity = new Account
                            {
                                ParentId = parentId,
                                Code = code
                            };


                            context.Accounts.Add(entity);
                            await context.SaveChangesAsync();


                        }
                    }
                }


            });

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

        private async Task ImportBalanceSheetToDBAsync(int statementId, Task2Context context, string filePath)
        {

            try
            {
                List<BalanceSheet> block = new List<BalanceSheet>();

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {

                    ISheet sheet = workBoook.GetSheetAt(0);

                    for (int i = ACCOUNT_DATA_START_ROW; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);

                        if (row.GetCell(0).ToString().Contains("КЛАСС") || row.GetCell(0).ToString().Contains("БАЛАНС"))
                            continue;

                        if (row != null)
                        {
                            BalanceSheet entity = new BalanceSheet
                            {
                                IncomingBalanceAsset = row.GetCell(INCOMING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                IncomingBalanceLiability = row.GetCell(INCOMING_BALANCE_LIABILITY_COLUMN).NumericCellValue,
                                TurnoverDebet = row.GetCell(TURNOVER_DEBET_COLUMN).NumericCellValue,
                                TurnoverCredit = row.GetCell(TURNOVER_CREDIT_COLUMN).NumericCellValue,
                                OutgoingBalanceAsset = row.GetCell(OUTGOING_BALANCE_ASSET_COLUMN).NumericCellValue,
                                OutgoingBalanceLiability = row.GetCell(OUTGOING_BALANCE_LIABILITY_COLUMN).NumericCellValue,
                                StatementId = statementId,
                                AccountId = await FindAccountIdByValueAsync(Int32.Parse(row.GetCell(ACCOUNT_COLUMN).ToString()), context)
                            };

                            block.Add(entity);
                        }

                        if (i % IMPORT_BLOCK_SIZE == 0)
                        {
                            context.BalanceSheets.AddRange(block);
                            await context.SaveChangesAsync();
                            block.Clear();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                
                return;
            }


        }

        private async Task<int> FindAccountIdByValueAsync(int code, Task2Context context)
        {
            List<Account> accounts = context.Accounts.ToList();
            int id = await Task.Run(() => accounts.Find(a => a.Code == code).Id);
            if (id != null)
                return id;
            return 0;
        }
    }
}
