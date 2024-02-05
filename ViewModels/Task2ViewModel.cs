using B1_Test_Task.Commands;
using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_2;
using B1_Test_Task.Models.Task_2.DBViews;
using B1_Test_Task.Services;
using B1_Test_Task.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static B1_Test_Task.Data.Task2ContextRepository;

namespace B1_Test_Task.ViewModels
{
    class Task2ViewModel: BaseViewModel
    {
        #region PROPERTIES

        private Action DataImportedToDb;

        private ObservableCollection<string> filePaths;
        public ObservableCollection<string> FilePaths { get => filePaths; set => Set(ref filePaths, value); }

        private int rowsImportedToDBCount;
        public int RowsImportedToDBCount { get => rowsImportedToDBCount; set => Set(ref rowsImportedToDBCount, value); }

        private string importState;
        public string ImportState { get => importState; set => Set(ref importState, value); }

        private ExcelFileService service;

        private Task2ContextRepository repository;
        /*

        private ObservableCollection<BalanceSheet> balanceSheets;

        public ObservableCollection<BalanceSheet> BalanceSheets { get => balanceSheets; set => Set(ref balanceSheets, value); }
        */
        private ObservableCollection<AccountJoinBalanceSheet> balanceSheets;

        public ObservableCollection<AccountJoinBalanceSheet> BalanceSheets { get => balanceSheets; set => Set(ref balanceSheets, value); }

        private ObservableCollection<Statement> statements;

        public ObservableCollection<Statement> Statements { get => statements; set => Set(ref statements, value); }


        private Task2Context context;

        private Statement selectedStatement;
        public Statement SelectedStatement
        { get => selectedStatement; set => 
                Set(ref selectedStatement, value);
            
        }

        private string task2status;
        public string Task2Status { get => task2status; set => Set(ref task2status, value); }

        #endregion
        #region COMMANDS

        #region UploadExcelFileCommand


        public BaseCommand UploadExcelFileCommand { get; }

        private void UploadExcelFileCommandExecuted(object c)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.Multiselect = true;
            dialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string[] selectedFileNames = dialog.FileNames;

                foreach(string name in selectedFileNames)
                {
                    if (!FilePaths.Contains(name))
                    {
                        FilePaths.Add(name);
                    }
                }
            }

            ImportFilesToDBCommand.RaiseCanExecuteChanged();


        }

        private bool CanUploadExcelFileCommandExecute(object c)
        {

            return true;

        }
        #endregion


        #region ImportFilesToDBCommand


        public BaseCommand ImportFilesToDBCommand { get; }

        private async void ImportFilesToDBCommandExecuted(object c)
        {

            Task2Status = "importing files...";
            

           
                foreach (string filePath in filePaths)
                {
                    
                    var importResult = await service.ImportDataToDBAsync(new Task2Context(), filePath);

                    Task2Status += $"\n{importResult}";
                    
                }
                 
            ImportState += "\nimporting files finished";

            FilePaths.Clear();

            DataImportedToDb.Invoke();


        }

        private bool CanImportFilesToDBCommandExecute(object c)
        {

            return FilePaths.Count>0;

        }
        #endregion

        #region LoadStatementsCommand


        public BaseCommand LoadStatementsCommand { get; }

        private void LoadStatementsCommandExecuted(object c)
        {
            
            BalanceSheets = new ObservableCollection<AccountJoinBalanceSheet>(repository.GetAccountInnerJoinBalanceSheet(SelectedStatement.Id));

        }

        private bool CanLoadStatementsCommandExecute(object c)
        {

            return true;

        }
        #endregion




        #endregion
        #region CONSTRUCTORS

        public Task2ViewModel()
        {
            service = new ExcelFileService();

            UploadExcelFileCommand = new BaseCommand(UploadExcelFileCommandExecuted, CanUploadExcelFileCommandExecute);
            ImportFilesToDBCommand = new BaseCommand(ImportFilesToDBCommandExecuted, CanImportFilesToDBCommandExecute);
            LoadStatementsCommand = new BaseCommand(LoadStatementsCommandExecuted, CanLoadStatementsCommandExecute);

            filePaths = new ObservableCollection<string>();
            FilePaths = new ObservableCollection<string>();

            repository = new Task2ContextRepository();
            repository.GetAccountInnerJoinBalanceSheet(1);

            context = new Task2Context();

            Statements = new ObservableCollection<Statement>(context.Statements.ToList());

            BalanceSheets = new ObservableCollection<AccountJoinBalanceSheet>(repository.GetAccountInnerJoinBalanceSheet(1).ToList());
            DataImportedToDb += UpdateDataDisplay;

            SelectedStatement = context.Statements.FirstOrDefault();
            if (SelectedStatement!=null)
            {
                BalanceSheets = new ObservableCollection<AccountJoinBalanceSheet>(repository.GetAccountInnerJoinBalanceSheet(SelectedStatement.Id).ToList());
            }
            

        }

        #endregion
        #region METHODS


        private void UpdateDataDisplay()
        {
            Statements = new ObservableCollection<Statement>(context.Statements.ToList());

            BalanceSheets = new ObservableCollection<AccountJoinBalanceSheet>(repository.GetAccountInnerJoinBalanceSheet(1).ToList());
        }

        private void HighlightBalanceSheet() {
            
        }

        #endregion
    }
}
