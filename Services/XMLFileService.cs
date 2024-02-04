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
            if (substring == "")
                return;

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

        public async Task ConcatenateXmlFilesWrongAsync(List<string> fileNames, string outputFileName)
        {
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
                Console.WriteLine("XML file deleted successfully.");
            }

            await Task.Run(() => {
                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    writer.Write(XML_ROOT_ELEMENT_OPEN);

                    foreach (string file in fileNames)
                    {
                        string fileText = File.ReadAllText(file);

                        fileText = fileText.Replace(XML_ROOT_ELEMENT_OPEN, "");
                        fileText = fileText.Replace(XML_ROOT_ELEMENT_CLOSE, "");

                        writer.Write(fileText); // Concatenate the text from each file and write to the output file

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
                Console.WriteLine("XML file deleted successfully.");
            }

            XmlDocument concatenatedDocument = new XmlDocument();

            // Create the root element for the concatenated document
            XmlElement rootArrayOfRow = concatenatedDocument.CreateElement("ArrayOfRow");
            concatenatedDocument.AppendChild(rootArrayOfRow);

            // Iterate through the list of document paths
            foreach (string documentPath in fileNames)
            {
                // Load each XML document asynchronously
                XmlDocument doc = new XmlDocument();
                await Task.Run(() => doc.Load(documentPath));

                // Get all the "Row" elements from the current document
                XmlNodeList rowElements = doc.GetElementsByTagName("Row");

                // Append each "Row" element to the root element of the concatenated document
                foreach (XmlNode rowElement in rowElements)
                {
                    XmlNode importedNode = concatenatedDocument.ImportNode(rowElement, true);
                    rootArrayOfRow.AppendChild(importedNode);
                }

                await Task.Run(() => concatenatedDocument.Save(outputFileName));

                OneFileConcatenated.Invoke();
            }

            // Save the concatenated document to the output filename
            

            /*
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

                // Create a new node for each loaded document and append it to the root element of the concatenated document
                XmlNode importedNode = await Task.Run(() => concatenatedDocument.ImportNode(doc.DocumentElement, true));
                await Task.Run(() => rootElement.AppendChild(importedNode));

                await Task.Run(() => concatenatedDocument.Save(outputFileName));

                OneFileConcatenated.Invoke();

            }
            */

            // Save the concatenated document to the output filename
            //concatenatedDocument.Save(outputFileName);

            /*
            XmlDocument concatenatedDocument = new XmlDocument();

            
            XmlElement rootElement = concatenatedDocument.CreateElement("ArrayOfRow");
            concatenatedDocument.AppendChild(rootElement);

            foreach (string documentPath in fileNames)
            {
                XmlDocument doc = new XmlDocument();
                await Task.Run(() => doc.Load(documentPath));

                List<XmlNode> nodes = new List<XmlNode>();

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    XmlNode importedNode = await Task.Run(() => concatenatedDocument.ImportNode(node, true));
                    nodes.Add(importedNode);
                    //await Task.Run(() => rootElement.AppendChild(importedNode));

                }
                await Task.Run(() => rootElement.AppendChild(nodes));
                //await Task.Run(() => rootElement.);

            }

            concatenatedDocument.Save(outputFileName);
            */
        }
        
        

        public async Task ImportDataToDB(Task1Context context, string filePath)
        {
            List<Row> rows = await ReadDataFromFileAsync(filePath);


            //array realization


            /*Row[] block = new Row[IMPORT_BLOCK_SIZE];
            int cutBlock;

            await Task.Run(() =>
            {
                int i = 1;
                foreach (var row in rows)
                {
                    
                    cutBlock = i % IMPORT_BLOCK_SIZE;
                    block[cutBlock] = row;

                    if (cutBlock == 0)
                    {
                        RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                        context.Rows.AddRange(block);
                        context.SaveChanges();
                        
                    }
                    i++;
                }
            });*/

            //List realization
            int cutBlock;
            List<Row> block = new List<Row>();
            await Task.Run(() =>
            {
                int i = 1;
                foreach (var row in rows)
                {

                    cutBlock = i % IMPORT_BLOCK_SIZE;
                    block.Add(row);

                    if (cutBlock == 0)
                    {
                        RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                        context.Rows.AddRange(block);
                        context.SaveChanges();
                        block.Clear();
                    }
                    i++;
                }/*
                for (int i = 1; i < rows.Count + 1; i++)
                {
                    
                    block.Add(rows[i - 1]);



                    if (i % IMPORT_BLOCK_SIZE == 0)
                    {
                        RowImportedToDB?.Invoke(IMPORT_BLOCK_SIZE);
                        context.Rows.AddRange(block);
                        context.SaveChanges();
                        block.Clear();
                    }
                }*/
            });



        }


    }

    
}
