﻿using B1_Test_Task.Models.Task_1;
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

namespace B1_Test_Task.Services
{
    class XMLFileService
    {
        public Action RowDeleted;
        public void WriteDataToFile(List<Row> instances, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, instances);
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
                Console.WriteLine("Error writing to XML: " + ex.Message);
            }

        }



        public List<Row> ReadDataFromFileAsync(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (List<Row>)serializer.Deserialize(reader);
            }
        }

        public async Task DeleteRowsAsync(List<string> filePaths, string substring)
        {
            foreach(string filePath in filePaths)
            {
                
                List<Row> oldRows;
                List<Row> newRows=new List<Row>();
                XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    oldRows = await Task.Run(()=>(List<Row>)serializer.Deserialize(reader));
                }

                //substring = "jj";

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

        public async Task ConcatenateXmlFilesAsync(List<string> fileNames, string outputFileName)
        {

            List<XDocument> xmlDocuments = new List<XDocument>();

            foreach (string filePath in fileNames)
            {
                if (File.Exists(filePath))
                {
                    XDocument doc = await Task.Run(() => XDocument.Load(filePath));

                    xmlDocuments.Add(doc);
                }
                else
                {
                    throw new FileNotFoundException($"File not found at path {filePath}");
                }
            }

            /*
            XDocument concatenatedDocument = await Task.Run(() => new XDocument(
                new XElement("Root",
     xmlDocuments.SelectMany(doc => doc.Root.Elements())
                    )
                ));
            */

            XDocument concatenatedDocument = await Task.Run( async () => new XDocument(
               new XElement("Root", await Task.Run(async () =>
    xmlDocuments.SelectMany(doc => doc.Root.Elements())
                   ))
               ));



            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                await writer.WriteAsync(concatenatedDocument.ToString());
            }

        }
        
        public async Task ConcatenateXmlFilesAsyncOld(List<string> fileNames, string outputFileName)
        {

            try
            {
                XDocument combinedXml = XDocument.Load(fileNames[0]);

                for (int i = 1; i < fileNames.Count; i++)
                {
                    XDocument nextXml = XDocument.Load(fileNames[i]);
                    combinedXml.Root.Add(nextXml.Root.Descendants());
                }

                await Task.Run(() =>
                {
                    combinedXml.Save(outputFileName);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error concatenating XML files: " + ex.Message);
                throw;
            }


            //await Task.Delay(5000);
        }

        
    }

    
}