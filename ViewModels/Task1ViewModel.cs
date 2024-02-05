using B1_Test_Task.Commands;
using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_1;
using B1_Test_Task.Services;
using B1_Test_Task.ViewModels.Base;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace B1_Test_Task.ViewModels
{
    public class Task1ViewModel : BaseViewModel
    {
        #region PROPERTIES

        const int ROWS_IN_FILE_AMOUNT = 100000;

        private Task1ContextRepository repository;

        private Random random = new Random();
        private XMLFileService fileService = new XMLFileService();

        private int filesCreatedAmount = 0;
        public int FilesCreatedAmount { get => filesCreatedAmount; set => Set(ref filesCreatedAmount, value); }

        private int rowsDeletedAmount;
        public int RowsDeletedAmount { get => rowsDeletedAmount; set => Set(ref rowsDeletedAmount, value); }

        private int rowsImportedToDBCount;
        public int RowsImportedToDBCount { get => rowsImportedToDBCount; set => Set(ref rowsImportedToDBCount, value); }

        private bool operationIsRunning;

        private bool filesAreGenerated;
        private bool filesAreConcatenated;
        private bool filesAreImported;

        public bool FilesAreCreating { get => filesAreCreating; set => Set(ref filesAreCreating, value); }
        private bool filesAreCreating;

        private List<string> fileNames = new List<string>();

        private string concatenateProcessState;
        public string ConcatenateProcessState { get => concatenateProcessState; set => Set(ref concatenateProcessState, value); }

        public string DeleteSubstring { get; set; }

        public string Task1Status { get => task1Status; set => Set(ref task1Status, value); }
        private string task1Status;

        private int progressBarMin;
        public int ProgressBarMin { get => progressBarMin; set => Set(ref progressBarMin, value); }

        private int progressBarMax;
        public int ProgressBarMax { get => progressBarMax; set => Set(ref progressBarMax, value); }

        private int progressBarValue;
        public int ProgressBarValue { get => progressBarValue; set => Set(ref progressBarValue, value); }

        public int FilesConcatenatedCount { get => filesConcatenatedCount; set => Set(ref filesConcatenatedCount, value); }
        private int filesConcatenatedCount;


        public long IntSumm { get => intSumm; set => Set(ref intSumm, value); }
        private long intSumm;

        public float DecimalMedian { get => decimalMedian; set => Set(ref decimalMedian, value); }
        private float decimalMedian;

        private Task1Context context;

        private bool canChangeFilesAmount;
        public bool CanChangeFilesAmount { get => canChangeFilesAmount; set => Set(ref canChangeFilesAmount, value); }

        private int filesAmountMax;
        public int FilesAmountMax { get => filesAmountMax; set => Set(ref filesAmountMax, value); }

        private int filesAmountMin;
        public int FilesAmountMin { get => filesAmountMin; set => Set(ref filesAmountMin, value); }

        private int filesAmountCurrent;
        public int FilesAmountCurrent { get => filesAmountCurrent; set => Set(ref filesAmountCurrent, value); }

        private string filesOutputFolder;
        public string FilesOutputFolder { get => filesOutputFolder; set => Set(ref filesOutputFolder, value); }


        #endregion

        #region COMMANDS

        #region  GenerateFilesCommand


        public BaseCommand GenerateFilesCommand { get; }

        private async void OnGenerateFilesCommandExecuted(object c)
        {
            FilesCreatedAmount = 0;
            ProgressBarMin = 0;
            ProgressBarMax = FilesAmountCurrent;
            CanChangeFilesAmount = false;

            FilesCreatedAmount = 0;
            operationIsRunning = true;

            for (int i = 0; i < FilesAmountCurrent; i++)
            {
                string fileName = FilesOutputFolder + "/" + $"data_{i + 1}.xml";
                fileNames.Add(fileName);
                await GenerateFileData(fileName);
                FilesCreatedAmount++;

                ProgressBarValue = FilesCreatedAmount;
                Task1Status = $"generated {FilesCreatedAmount}/{FilesAmountCurrent} files";

            }
            operationIsRunning = false;
            filesAreGenerated = true;

            RaiseCommandsCanExecuteCnaged();

        }

        private bool CanGenerateFilesCommandExecute(object c)
        {

            return !operationIsRunning && !filesAreGenerated;

        }
        #endregion

        #region  ConcatenateFilesCommand


        public BaseCommand ConcatenateFilesCommand { get; }

        private async void OnConcatenateFilesCommandExecuted(object c)
        {
            FilesConcatenatedCount = 0;
            ProgressBarMin = 0;
            ProgressBarMax = FilesAmountCurrent;
            Task1Status = "start concatenate files";
            operationIsRunning = true;

            Task t = fileService.DeleteRowsAsync(fileNames, DeleteSubstring);
            await t;
            ConcatenateProcessState = "concatenating files...";
            await fileService.ConcatenateXmlFilesAsync(fileNames, FilesOutputFolder + "/" + "data_common.xml");
            ConcatenateProcessState = "finished concatenating files!";

            operationIsRunning = false;
            filesAreConcatenated = true;

            Task1Status = "finish concatenate files";

            RaiseCommandsCanExecuteCnaged();

        }

        private bool CanConcatenateFilesCommandExecute(object c)
        {
            return !operationIsRunning && !filesAreConcatenated && filesAreGenerated;
        }
        #endregion

        #region  ImportDataToDBCommand

        public BaseCommand ImportDataToDBCommand { get; }

        private async void OnImportDataToDBCommandExecuted(object c)
        {
            ProgressBarMin = 0;
            ProgressBarMax = FilesAmountCurrent * ROWS_IN_FILE_AMOUNT;
            Task1Status = "start importing data to database";

            operationIsRunning = true;
            await fileService.ImportDataToDB(context, FilesOutputFolder + "/" + "data_common.xml");
            operationIsRunning = false;
            filesAreImported = true;

            RaiseCommandsCanExecuteCnaged();

            Task1Status = "succesfully imported data to database";

        }

        private bool CanImportDataToDBCommandExecute(object c)
        {

            return !operationIsRunning && !filesAreImported && filesAreConcatenated;

        }
        #endregion

        #region  SetFilesFolderCommand

        public BaseCommand SetFilesFolderCommand { get; }

        private async void OnSetFilesFolderCommandExecuted(object c)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dialog.FileName;
                FilesOutputFolder = folder;
            }

        }

        private bool CanSetFilesFolderCommandExecute(object c)
        {

            return !filesAreGenerated && !operationIsRunning;

        }
        #endregion

        #region  CalculateSummAndMedianCommand


        public BaseCommand CalculateSummAndMedianCommand { get; }

        private async void OnCalculateSummAndMedianCommandExecuted(object c)
        {
            IntSumm = await Task.Run(() => repository.GetIntSumm());
            DecimalMedian = await Task.Run(() => repository.GetFloatMedian());

            Task1Status = $"summ of integer : {intSumm}; median of decimals: {DecimalMedian}";

        }

        private bool CanCalculateSummAndMedianCommandExecute(object c)
        {

            return !operationIsRunning && filesAreImported;

        }
        #endregion

        #endregion
        #region CONSTRUCTOR

        public Task1ViewModel()
        {
            GenerateFilesCommand = new BaseCommand(OnGenerateFilesCommandExecuted, CanGenerateFilesCommandExecute);
            ConcatenateFilesCommand = new BaseCommand(OnConcatenateFilesCommandExecuted, CanConcatenateFilesCommandExecute);
            ImportDataToDBCommand = new BaseCommand(OnImportDataToDBCommandExecuted, CanImportDataToDBCommandExecute);
            CalculateSummAndMedianCommand = new BaseCommand(OnCalculateSummAndMedianCommandExecuted, CanCalculateSummAndMedianCommandExecute);
            SetFilesFolderCommand = new BaseCommand(OnSetFilesFolderCommandExecuted, CanSetFilesFolderCommandExecute);
            repository = new Task1ContextRepository();

            context = new Task1Context();

            fileService.RowDeleted += UpdateRowsDeletedCount;
            fileService.OneFileConcatenated += UpdateFileConcatenatedCount;
            fileService.RowImportedToDB += UpdateRowsImportedToDBCount;

            FilesAmountMax = 100;
            FilesAmountMin = 10;
            FilesAmountCurrent = FilesAmountMin;
            CanChangeFilesAmount = true;

            ProgressBarValue = 0;
        }

        #endregion

        #region METHODS

        private async Task GenerateFileData(string fileName)
        {
            List<Row> rows = new List<Row>();
            int a = 0;

            await Task.Run(() =>
            {
                for (int i = 0; i < ROWS_IN_FILE_AMOUNT; i++)
                {
                    Row row = GenerateRowData();
                    rows.Add(row);
                    a++;
                }
            });

            await fileService.WriteDataToFileAsync(rows, fileName);
        }

        private Row GenerateRowData()
        {
            Row row = new Row();

            row.RanDate = GenerateDateTime();
            row.RanLatin = GenerateLatinString();
            row.RanCyrillic = GenerateCyrillicString();
            row.RanInt = GenerateRandomEvenInteger();
            row.RanDecimal = GenerateRandomDecimal();

            return row;
        }

        private DateTime GenerateDateTime()
        {
            DateTime today = DateTime.Today;
            DateTime fiveYearsAgo = today.AddYears(-5);

            int range = (today - fiveYearsAgo).Days;
            int randomDays = random.Next(range);

            DateTime randomDate = fiveYearsAgo.AddDays(randomDays);

            return randomDate;
        }

        private string GenerateLatinString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            char[] randomString = new char[10];

            for (int i = 0; i < 10; i++)
            {
                randomString[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomString);

        }

        private string GenerateCyrillicString()
        {
            const string chars = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюя";

            char[] randomString = new char[10];

            for (int i = 0; i < 10; i++)
            {
                randomString[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomString);
        }

        public int GenerateRandomEvenInteger()
        {
            return (int)random.Next(1, 50000000) * 2;
        }

        private float GenerateRandomDecimal()
        {
            float randomValue = (float)random.NextDouble() * 19 + 1;
            return (float)Math.Round(randomValue, 8);
        }

        private void UpdateRowsDeletedCount()
        {
            RowsDeletedAmount++;
            Task1Status = $"deleted {RowsDeletedAmount} rows";
        }

        private void UpdateRowsImportedToDBCount(int amount)
        {
            RowsImportedToDBCount += amount;
            Task1Status = $"imported {RowsImportedToDBCount} rows to database";
            ProgressBarValue = rowsImportedToDBCount;
        }

        private void UpdateFileConcatenatedCount()
        {
            FilesConcatenatedCount++;
            Task1Status = $"concatenated {FilesConcatenatedCount} files";
            ProgressBarValue = FilesConcatenatedCount;
        }


        private void RaiseCommandsCanExecuteCnaged()
        {
            GenerateFilesCommand.RaiseCanExecuteChanged();
            ConcatenateFilesCommand.RaiseCanExecuteChanged();
            ImportDataToDBCommand.RaiseCanExecuteChanged();
            SetFilesFolderCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
