using B1_Test_Task.Models.Task_1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace B1_Test_Task.Services
{
    class FileService
    {
        public void WriteDataToFile(List<Row> classInstance, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Row>));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, classInstance);
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
    }
}
