using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResaturantOrderManagement
{

    public class Food
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public double FoodPrice { get; set; }
    }


    public class OrderedFood
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public double FoodPrice { get; set; }
        public double TotalPrice { get; set; }
        public int Quantity { get; set; }
        public bool ServedStatus { get; set; }
    }
}
