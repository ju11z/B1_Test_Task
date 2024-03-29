﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Models.Task_2
{
    public class Account
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        [Index("CodeUniqueIndex", 2, IsUnique = true)]
        public int Code { get; set; }
        public string Title { get; set; }
    }
}
