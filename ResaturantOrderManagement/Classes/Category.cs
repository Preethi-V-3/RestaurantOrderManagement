using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResaturantOrderManagement
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public ObservableCollection<Food> Foods { get; set; }

        public Category()
        {
            Foods = new ObservableCollection<Food>();
        }
    }
}
