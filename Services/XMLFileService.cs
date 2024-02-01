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
        public void WriteDataToFile(List<Row> instances, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, instances);
            }
        }

        // Method to read data from a file using XML deserialization
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
            /*
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, classInstance);
            }
            */

            try
            {
                // Create a FileStream to write the XML file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(List<Row>));

                    // Use the XmlWriter to handle the asynchronous writing process
                    using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true }))
                    {
                        // Serialize the list of class instances and write to the XML file asynchronously
                        await Task.Run(() => serializer.Serialize(writer, instances));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions 
                Console.WriteLine("Error writing to XML: " + ex.Message);
            }

            //await Task.Delay(200);
        }



        // Method to read data from a file using XML deserialization
        public List<Row> ReadDataFromFileAsync(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (List<Row>)serializer.Deserialize(reader);
            }
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
                // Load the first XML file into an XDocument
                XDocument combinedXml = XDocument.Load(fileNames[0]);

                // Concatenate the remaining XML files
                for (int i = 1; i < fileNames.Count; i++)
                {
                    XDocument nextXml = XDocument.Load(fileNames[i]);
                    combinedXml.Root.Add(nextXml.Root.Descendants());
                }

                // Save the combined XML to the specified output file
                await Task.Run(() =>
                {
                    combinedXml.Save(outputFileName);
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions, e.g., log or notify the user
                Console.WriteLine("Error concatenating XML files: " + ex.Message);
                throw;
            }


            //await Task.Delay(5000);
        }

        
    }

    
}
