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

        const int FILES_AMOUNT = 10;
        const int ROWS_IN_FILE_AMOUNT = 100000;

        private Random random = new Random();
        private XMLFileService fileService = new XMLFileService();

        private int filesCreatedAmount=0;
        public int FilesCreatedAmount { get => filesCreatedAmount; set => Set(ref filesCreatedAmount, value); }

        private int rowsDeletedAmount;
        public int RowsDeletedAmount { get => rowsDeletedAmount; set => Set(ref rowsDeletedAmount, value); }

        private int rowsImportedToDBCount;
        public int RowsImportedToDBCount { get => rowsImportedToDBCount; set => Set(ref rowsImportedToDBCount, value); }



        public bool FilesAreCreating { get => filesAreCreating; set => Set(ref filesAreCreating, value); }
        private bool filesAreCreating;

        private List<string> fileNames = new List<string>();

        private string concatenateProcessState;
        public string ConcatenateProcessState { get=>concatenateProcessState; set=>Set(ref concatenateProcessState,value); }

        public string DeleteSubstring { get; set; }

        private Task1Context context;

        //private Task1Context context;


        #endregion

        #region COMMANDS

        #region  GenerateFilesCommand


        public BaseCommand GenerateFilesCommand { get; }

        private async void OnGenerateFilesCommandExecuted(object c)
        {
            FilesAreCreating = true;
            for(int i=0; i< FILES_AMOUNT; i++)
            {
                string fileName = $"data_{i+1}.xml";
                fileNames.Add(fileName);
                await GenerateFileData(fileName);
                FilesCreatedAmount++;
            }
            FilesAreCreating = false;
            GenerateFilesCommand.RaiseCanExecuteChanged();

        }

        private bool CanGenerateFilesCommandExecute(object c)
        {
            
            return !FilesAreCreating;

        }
        #endregion

        #region  ConcatenateFilesCommand


        public BaseCommand ConcatenateFilesCommand { get; }

        private async void OnConcatenateFilesCommandExecuted(object c)
        {
            //FilesAreCreating = true;
            
            Task t = fileService.DeleteRowsAsync(fileNames, DeleteSubstring);
            await t;
            ConcatenateProcessState = "concatenating files...";
            await fileService.ConcatenateXmlFilesAsync(fileNames, "data_common.xml");
            ConcatenateProcessState = "finished concatenating files!";

        }

        

        private bool CanConcatenateFilesCommandExecute(object c)
        {

            return true;

        }
        #endregion

        #region  ImportDataToDBCommand


        public BaseCommand ImportDataToDBCommand { get; }

        private async void OnImportDataToDBCommandExecuted(object c)
        {
            await fileService.ImportDataToDB(context,"data_common.xml");

        }



        private bool CanImportDataToDBCommandExecute(object c)
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

            context = new Task1Context();


            fileService.RowDeleted += UpdateRowsDeletedCount;
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
            row.RanUint = GenerateRandomEvenInteger();
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

        public uint GenerateRandomEvenInteger()
        {
            return (uint)random.Next(1, 50000000) * 2;
        }

        private double GenerateRandomDecimal()
        {
            double randomValue = random.NextDouble() * 19 + 1; // generates a random value between 1 and 20
            return Math.Round(randomValue, 8); // rounds the value to 8 decimal places
        }

        private void UpdateRowsDeletedCount()
        {
            RowsDeletedAmount++;
        }

        private void UpdateRowsImportedToDBCount(int amount)
        {
            RowsImportedToDBCount+=amount;
        }

        #endregion
    }
}
