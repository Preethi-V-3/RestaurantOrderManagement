using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ResaturantOrderManagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ObservableCollection<Category> categories;
        public static ObservableCollection<String> tables;
        public static ObservableCollection<Order> ordersCompletedList;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            categories = MyStorage.GetEmbeddedXML<ObservableCollection<Category>>("Data.foods.xml");
            ordersCompletedList = MyStorage.ReadXML<ObservableCollection<Order>>("ordersCompleted.xml");
            
            if (ordersCompletedList == null)
                ordersCompletedList = new ObservableCollection<Order>();
            
            tables = new ObservableCollection<string> { "Table01", "Table02", "Table03", "Table04", "Table05" };

            TakeOrdersWindow takeOrdersWindow = new TakeOrdersWindow();
            takeOrdersWindow.Show();

            OrdersInKitchenWindow ordersInKitchenWindow = new OrdersInKitchenWindow();
            ordersInKitchenWindow.Show();
        }
    }
}
