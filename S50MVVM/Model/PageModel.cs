using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S50MVVM.Model
{
    public class PageModel
    {

            public int CustomerCount { get; set; }
            public string ProductStatus { get; set; }
            public string WebID { get; set; }
            public string PoblacioID { get; set; }
            public string OrderStatus { get; set; }
            public decimal TransactionValue { get; set; }
            public DateTime ShipmentDelivery { get; set; }
            public bool LocationStatus { get; set; }

        
    }
}
