﻿using B1_Test_Task.Data;
using B1_Test_Task.Models.Task_1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections;

namespace B1_Test_Task.Services
{
    class XMLFileService
    {
        private const int IMPORT_BLOCK_SIZE = 10000;

        private const string XML_ROOT_ELEMENT_OPEN = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ArrayOfRow xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
        private const string XML_ROOT_ELEMENT_CLOSE = "</ArrayOfRow>";

        public Action OneFileConcatenated;

        public Action RowDeleted;
        public Action<int> RowImportedToDB;
        public void WriteDataToFile(List<Row> instances, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, instances);
            }
        }

        public async Task<List<Row>> ReadDataFromFileAsync(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await Task.Run(() =>
                (List<Row>)serializer.Deserialize(reader)
                );
            }
        }

        public async Task WriteDataToFileAsync(List<Row> instances, string filePath)
        {

            try
            {

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(List<Row>));


                    using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true }))
                    {
                        await Task.Run(() => serializer.Serialize(writer, instances));
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

        }

        public List<Row> ReadDataFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (List<Row>)serializer.Deserialize(reader);
            }
        }

        public async Task DeleteRowsAsync(List<string> filePaths, string substring)
        {
            if (substring == "")
                return;

            foreach (string filePath in filePaths)
            {

                List<Row> oldRows;
                List<Row> newRows = new List<Row>();
                XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    oldRows = await Task.Run(() => (List<Row>)serializer.Deserialize(reader));
                }


                foreach (Row row in oldRows)
                {

                    if (row.RanLatin.Contains(substring) || row.RanCyrillic.Contains(substring))
                    {

                        RowDeleted.Invoke();
                        continue;
                    }
                    newRows.Add(row);
                }


                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await Task.Run(() => serializer.Serialize(writer, newRows));
                }

            }

            await Task.CompletedTask;
        }

        public async Task XmlFilesWrongAsync(List<string> fileNames, string outputFileName)
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            await Task.Run(() =>
            {
                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    writer.Write(XML_ROOT_ELEMENT_OPEN);

                    foreach (string file in fileNames)
                    {
                        string fileText = File.ReadAllText(file);

                        fileText = fileText.Replace(XML_ROOT_ELEMENT_OPEN, "");
                        fileText = fileText.Replace(XML_ROOT_ELEMENT_CLOSE, "");

                        writer.Write(fileText);

                        OneFileConcatenated.Invoke();
                    }

                    writer.Write(XML_ROOT_ELEMENT_CLOSE);
                }
            });


        }

        public async Task ConcatenateXmlFilesAsync(List<string> fileNames, string outputFileName)
        {

            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            XmlDocument concatenatedDocument = new XmlDocument();


            XmlElement rootArrayOfRow = concatenatedDocument.CreateElement("ArrayOfRow");
            concatenatedDocument.AppendChild(rootArrayOfRow);

            foreach (string documentPath in fileNames)
            {
                XmlDocument doc = new XmlDocument();
                await Task.Run(() => doc.Load(documentPath));

                XmlNodeList rowElements = await Task.Run(() => doc.GetElementsByTagName("Row"));

                foreach (XmlNode rowElement in rowElements)
                {
                    XmlNode importedNode = concatenatedDocument.ImportNode(rowElement, true);
                    await Task.Run(() => rootArrayOfRow.AppendChild(importedNode));
                }

                await Task.Run(() => concatenatedDocument.Save(outputFileName));

                OneFileConcatenated.Invoke();
            }
        }



        public async Task ImportDataToDB(Task1Context context, string filePath)
        {
            List<Row> rows = await ReadDataFromFileAsync(filePath);
            Row[] block = new Row[IMPORT_BLOCK_SIZE];
            int cutBlock;

            await Task.Run(async () =>
            {
                int i = 1;
                foreach (var row in rows)
                {

                    cutBlock = i % IMPORT_BLOCK_SIZE;
                    block[cutBlock] = row;

                    if (cutBlock == 0)
                    {
                        RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                        await Task.Run(() => context.Rows.AddRange(block));
                        await Task.Run(() => context.SaveChangesAsync());

                    }
                    i++;
                }
            });

        }


    }


}
