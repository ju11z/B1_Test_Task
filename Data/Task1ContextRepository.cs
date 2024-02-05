using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_Test_Task.Data
{
    public class Task1ContextRepository
    {
        private Task1Context context;
        public Task1ContextRepository()
        {
            context = new Task1Context();
        }



        public long GetIntSumm()
        {
            var rez=context.Database.SqlQuery<long>("select sum(CAST(RanInt AS bigint)) from Rows");
            if (rez != null)
                return rez.First();
            else return 0;
        }

        public float GetFloatMedian()
        {
            var rez = context.Database.SqlQuery<float>("select ( (select max(RanDecimal) from(select top 50 percent RanDecimal from Rows order by RanDecimal) as bottomhalf) + (select min(RanDecimal) from(select top 50 percent RanDecimal from Rows order by Randecimal desc) as tophalf) ) / 2 as median");
            if (rez != null)
                return rez.First();
            else return 0;
        }
    }
}
