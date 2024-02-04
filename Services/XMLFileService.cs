using B1_Test_Task.Data;
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

namespace B1_Test_Task.Services
{
    class XMLFileService
    {
        private const int IMPORT_BLOCK_SIZE = 10000;

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

        public async Task <List<Row>> ReadDataFromFileAsync(string filePath)
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
                Console.WriteLine("Error writing to XML: " + ex.Message);
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
            foreach(string filePath in filePaths)
            {
                
                List<Row> oldRows;
                List<Row> newRows=new List<Row>();
                XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    oldRows = await Task.Run(()=>(List<Row>)serializer.Deserialize(reader));
                }


                foreach (Row row in oldRows)
                {
                    
                    if (substring!="" && (row.RanLatin.Contains(substring) || row.RanCyrillic.Contains(substring)))
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
            /*

            XmlDocument concatenatedDocument = new XmlDocument();

            // Create the root element for the concatenated document
            XmlElement rootElement = concatenatedDocument.CreateElement("ArrayOfRow");
            concatenatedDocument.AppendChild(rootElement);

            // Iterate through the list of document paths
            foreach (string documentPath in fileNames)
            {
                // Load each XML document asynchronously
                XmlDocument doc = new XmlDocument();
                await Task.Run(() => doc.Load(documentPath));

                // Get the root element of the loaded document
                XmlElement documentRootElement = await Task.Run(() => doc.DocumentElement);

                // Import the root element of the loaded document into the concatenated document
                XmlNode importedNode = await Task.Run(() => concatenatedDocument.ImportNode(documentRootElement, true));
                await Task.Run(() => rootElement.AppendChild(importedNode));
            }

            // Save the concatenated document to the output filename
            await Task.Run(() => concatenatedDocument.Save(outputFileName));
            */

            XmlDocument concatenatedDocument = new XmlDocument();

            // Create the root element for the concatenated document
            XmlElement rootElement = concatenatedDocument.CreateElement("ArrayOfRow");
            concatenatedDocument.AppendChild(rootElement);

            // Iterate through list of document paths
            foreach (string documentPath in fileNames)
            {
                // Load each XML document asynchronously
                XmlDocument doc = new XmlDocument();
                await Task.Run(() => doc.Load(documentPath));

                // Get the child nodes of the loaded document and append them to the root element of the concatenated document
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    XmlNode importedNode = await Task.Run(() => concatenatedDocument.ImportNode(node, true));
                    await Task.Run(() => rootElement.AppendChild(importedNode));
                }
            }

            // Save the concatenated document to the output filename
            concatenatedDocument.Save(outputFileName);

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

        public async Task ImportDataToDB(Task1Context context, string filePath)
        {
            List<Row> rows = await ReadDataFromFileAsync(filePath);

            List<Row> block = new List<Row>();
            await Task.Run(() =>
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    block.Add(rows[i]);

                    if (i % IMPORT_BLOCK_SIZE == 0)
                    {
                        RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                        context.Rows.AddRange(block);
                        context.SaveChanges();
                        block.Clear();
                    }
                }
            });
                /*
            await Task.Run(() =>
            {
                {
                    foreach (Row row in rows)
                    {
                        context.Rows.Add(row);
                        RowImportedToDB?.Invoke();
                        context.SaveChanges();
                    }
                }
            }
                );
                */

            


        }

        
    }

    
}
