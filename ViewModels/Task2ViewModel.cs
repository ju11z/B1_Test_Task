using B1_Test_Task.Commands;
using B1_Test_Task.Data;
using B1_Test_Task.Services;
using B1_Test_Task.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.ViewModels
{
    class Task2ViewModel: BaseViewModel
    {
        #region PROPERTIES

        private ObservableCollection<string> filePaths;
        public ObservableCollection<string> FilePaths { get => filePaths; set => Set(ref filePaths, value); }

        private int rowsImportedToDBCount;
        public int RowsImportedToDBCount { get => rowsImportedToDBCount; set => Set(ref rowsImportedToDBCount, value); }

        private ExcelFileService service;

        #endregion
        #region COMMANDS

        #region UploadExcelFileCommand


        public BaseCommand UploadExcelFileCommand { get; }

        private async void UploadExcelFileCommandExecuted(object c)
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
            

            foreach(string filePath in filePaths)
            {
                await service.ImportDataToDB(new Task2Context(),filePath);
            }


        }

        private bool CanImportFilesToDBCommandExecute(object c)
        {

            return true;

        }
        #endregion


        #endregion
        #region CONSTRUCTORS

        public Task2ViewModel()
        {
            service = new ExcelFileService();
            service.RowImportedToDB += UpdateRowsImportedToDBCount;  

            UploadExcelFileCommand = new BaseCommand(UploadExcelFileCommandExecuted, CanUploadExcelFileCommandExecute);
            ImportFilesToDBCommand = new BaseCommand(ImportFilesToDBCommandExecuted, CanImportFilesToDBCommandExecute);

            filePaths = new ObservableCollection<string>();
            FilePaths = new ObservableCollection<string>();
            
        }

        #endregion
        #region METHODS

        private void UpdateRowsImportedToDBCount(int amount)
        {
            RowsImportedToDBCount += amount;
        }

        #endregion
    }
}
