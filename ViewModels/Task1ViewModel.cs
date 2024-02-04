using B1_Test_Task.Commands;
using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_1;
using B1_Test_Task.Services;
using B1_Test_Task.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace B1_Test_Task.ViewModels
{
    public class Task1ViewModel: BaseViewModel
    {
        #region PROPERTIES

        const int FILES_AMOUNT = 20;
        const int ROWS_IN_FILE_AMOUNT = 100000;

        private Task1ContextRepository repository;

        private Random random = new Random();
        private XMLFileService fileService = new XMLFileService();

        private int filesCreatedAmount=0;
        public int FilesCreatedAmount { get => filesCreatedAmount; set => Set(ref filesCreatedAmount, value); }

        private int rowsDeletedAmount;
        public int RowsDeletedAmount { get => rowsDeletedAmount; set => Set(ref rowsDeletedAmount, value); }

        private int rowsImportedToDBCount;
        public int RowsImportedToDBCount { get => rowsImportedToDBCount; set => Set(ref rowsImportedToDBCount, value); }

        private bool operationIsRunning;

        private bool filesAreGenerated;
        private bool filesAreConcatenated;

        public bool FilesAreCreating { get => filesAreCreating; set => Set(ref filesAreCreating, value); }
        private bool filesAreCreating;

        private List<string> fileNames = new List<string>();

        private string concatenateProcessState;
        public string ConcatenateProcessState { get=>concatenateProcessState; set=>Set(ref concatenateProcessState,value); }

        public string DeleteSubstring { get; set; }

        public int FilesConcatenatedCount { get=>filesConcatenatedCount; set=>Set(ref filesConcatenatedCount, value); }
        private int filesConcatenatedCount;


        public long IntSumm { get => intSumm; set => Set(ref intSumm, value); }
        private long intSumm;

        public float DecimalMedian { get => decimalMedian; set => Set(ref decimalMedian, value); }
        private float decimalMedian;

        private Task1Context context;

        //private Task1Context context;


        #endregion

        #region COMMANDS

        #region  GenerateFilesCommand


        public BaseCommand GenerateFilesCommand { get; }

        private async void OnGenerateFilesCommandExecuted(object c)
        {
            FilesCreatedAmount = 0;
            operationIsRunning = true;

            for(int i=0; i< FILES_AMOUNT; i++)
            {
                string fileName = $"data_{i+1}.xml";
                fileNames.Add(fileName);
                await GenerateFileData(fileName);
                FilesCreatedAmount++;
            }
            operationIsRunning = false;
            filesAreGenerated = true;

            RaiseCommandsCanExecuteCnaged();

        }

        private bool CanGenerateFilesCommandExecute(object c)
        {
            
            return !operationIsRunning;

        }
        #endregion

        #region  ConcatenateFilesCommand


        public BaseCommand ConcatenateFilesCommand { get; }

        private async void OnConcatenateFilesCommandExecuted(object c)
        {
            //FilesAreCreating = true;
            operationIsRunning = true;
            
            Task t = fileService.DeleteRowsAsync(fileNames, DeleteSubstring);
            await t;
            ConcatenateProcessState = "concatenating files...";
            await fileService.ConcatenateXmlFilesAsync(fileNames, "data_common.xml");
            ConcatenateProcessState = "finished concatenating files!";

            operationIsRunning = false;
            filesAreConcatenated = true;

            RaiseCommandsCanExecuteCnaged();

        }

        

        private bool CanConcatenateFilesCommandExecute(object c)
        {

            return !operationIsRunning&&filesAreGenerated;

        }
        #endregion

        #region  ImportDataToDBCommand


        public BaseCommand ImportDataToDBCommand { get; }

        private async void OnImportDataToDBCommandExecuted(object c)
        {
            operationIsRunning = true;
            await fileService.ImportDataToDB(context,"data_common.xml");
            operationIsRunning = false;

            RaiseCommandsCanExecuteCnaged();

        }


        private bool CanImportDataToDBCommandExecute(object c)
        {

            return !operationIsRunning&&filesAreConcatenated;

        }
        #endregion

        #region  CalculateSummAndMedianCommand


        public BaseCommand CalculateSummAndMedianCommand { get; }

        private async void OnCalculateSummAndMedianCommandExecuted(object c)
        {
            IntSumm = repository.GetIntSumm();
            DecimalMedian = repository.GetFloatMedian();

        }


        private bool CanCalculateSummAndMedianCommandExecute(object c)
        {

            return true;

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
            repository = new Task1ContextRepository();

            context = new Task1Context();


            fileService.RowDeleted += UpdateRowsDeletedCount;
            fileService.OneFileConcatenated += UpdateFileConcatenatedCount;
            fileService.RowImportedToDB += UpdateRowsImportedToDBCount;
        }

        #endregion

        #region METHODS

        private async Task GenerateFileData(string fileName)
        {
            List<Row> rows = new List<Row>();
            int a=0;
            for (int i = 0; i < ROWS_IN_FILE_AMOUNT; i++)
            {
                Row row = GenerateRowData();
                rows.Add(row);
                a++;
            }
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
            
            // Calculate the range for the random date
            DateTime today = DateTime.Today;
            DateTime fiveYearsAgo = today.AddYears(-5);

            // Generate a random number of days within the range
            int range = (today - fiveYearsAgo).Days;
            int randomDays = random.Next(range);

            // Add the random number of days to the start date
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
            float randomValue = (float)random.NextDouble() * 19 + 1; // generates a random value between 1 and 20
            return (float)Math.Round(randomValue, 8); // rounds the value to 8 decimal places
        }

        private void UpdateRowsDeletedCount()
        {
            RowsDeletedAmount++;
        }

        private void UpdateRowsImportedToDBCount(int amount)
        {
            RowsImportedToDBCount+=amount;
        }

        private void UpdateFileConcatenatedCount()
        {
            FilesConcatenatedCount++;
        }

        private void RaiseCommandsCanExecuteCnaged()
        {
            GenerateFilesCommand.RaiseCanExecuteChanged();
            ConcatenateFilesCommand.RaiseCanExecuteChanged();
            ImportDataToDBCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
