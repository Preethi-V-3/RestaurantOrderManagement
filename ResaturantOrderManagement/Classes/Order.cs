using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResaturantOrderManagement
{
    public class Order
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string TableNo { get; set; }
        public ObservableCollection<OrderedFood> OrderedFoods { get; set; }

        public Order()
        {
            OrderedFoods = new ObservableCollection<OrderedFood>();
        }
    }
}
