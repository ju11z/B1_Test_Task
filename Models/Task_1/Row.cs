using System;
using System.Collections.Generic;
using System.Text;

namespace B1_Test_Task.Models.Task_1
{
    [Serializable]
    public class Row
    {
        public int Id { get; set; }
        public DateTime RanDate { get; set; }
        public string RanLatin { get; set; }
        public string RanCyrillic { get; set; }
        public int RanInt { get; set; }
        public float RanDecimal { get; set; }
    }
}
