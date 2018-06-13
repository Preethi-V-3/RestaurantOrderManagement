using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ResaturantOrderManagement
{
    /// <summary>
    /// Interaction logic for OrdersInKitchen.xaml
    /// </summary>
    public partial class OrdersInKitchenWindow : Window
    {

        Order orderinKitchen;
        ObservableCollection<Order> kitchenlist;
        public static OrdersInKitchenWindow Instance { get; private set; }

        public OrdersInKitchenWindow()
        {
            InitializeComponent();

            Instance = this;
            kitchenlist = MyStorage.ReadXML<ObservableCollection<Order>>("ordersInProcessing.xml");
            if (kitchenlist == null)
                kitchenlist = new ObservableCollection<Order>();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Lbx_OrderTableNo.ItemsSource = kitchenlist;
        }

        private void Lbx_OrderTableNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if ((sender as ListBox).SelectedItem != null)
            //{
               orderinKitchen = (sender as ListBox).SelectedItem as Order;
            //    Dg_ProcessingOrders.ItemsSource = orderinKitchen.OrderedFoods;
            //}

        }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)e.OriginalSource;
            var selectedItem = checkBox.DataContext as OrderedFood;
            Dg_ProcessingOrders.SelectedItem = selectedItem;

            foreach (var order in kitchenlist)
            {
                if (orderinKitchen.OrderId == order.OrderId)
                {
                    foreach (var food in order.OrderedFoods)
                    {
                        if (selectedItem.FoodId == food.FoodId && food.Quantity == selectedItem.Quantity)
                        {
                               if (selectedItem.ServedStatus)
                                {
                                    food.ServedStatus = true;
                                    var lst = order.OrderedFoods.Where(S => S.FoodId == food.FoodId)
                                            .Select(S => { S.ServedStatus = true; return S; }).ToList();

                                    TakeOrdersWindow.UpdateServedStatus(orderinKitchen.OrderId, food);

                                }
                            break;
                        }
                    }
                    break;
                }
            }
           
            Dg_ProcessingOrders.CommitEdit();
            Dg_ProcessingOrders.CommitEdit();

        }

        public void Button_FinishServing_Click(object sender, RoutedEventArgs e)
        {
            bool removeFromKitchenList = true;

            foreach (var food in Instance.orderinKitchen.OrderedFoods)
            {
                if (!food.ServedStatus)
                {
                    removeFromKitchenList = false;
                }
            }

            if (removeFromKitchenList)
            {
                Instance.kitchenlist.Remove(Instance.orderinKitchen);
                Instance.Lbx_OrderTableNo.ItemsSource = Instance.kitchenlist;
            }
            else
            {
                MessageBox.Show("There are still orders to be served");
            }
        }

        internal static void UpdateFoodsServed(Order order)
        {
            if (!App.ordersCompletedList.Contains(order))
            {
                bool orderExists = false;

                foreach (var ord in Instance.kitchenlist)
                {
                    if (ord.OrderId == order.OrderId)
                    {
                        orderExists = true;
                        var notServedFoods = order.OrderedFoods.Where(S => S.ServedStatus == false).ToList();
                        var notServedFoodsInKitchen = ord.OrderedFoods.Where(S => S.ServedStatus == false).ToList();

                        //for removing the food in the kitchen
                        foreach (var food in notServedFoodsInKitchen)
                        {
                            if (!notServedFoods.Contains(food))
                            {
                                ord.OrderedFoods.Remove(food);
                            }
                        }

                        //for adding or updating the food in the kitchen
                        foreach (var food in notServedFoods)
                        {
                            var foodExists = ord.OrderedFoods.Where(S => S.FoodId == food.FoodId);
                            var lst = foodExists.Where(S => S.Quantity != food.Quantity).Select(S => { S.Quantity = food.Quantity; S.TotalPrice = food.TotalPrice; return S; }).ToList();

                            if (foodExists.Count() <= 0)
                            {
                                ord.OrderedFoods.Add(food);
                            }
                        }
                        break;
                    }
                }

                if (!orderExists)
                {
                    Order ord = new Order();
                    ord.OrderId = order.OrderId;
                    ord.OrderDate = order.OrderDate;
                    ord.TableNo = order.TableNo;
                    ord.OrderedFoods = order.OrderedFoods;

                    Instance.kitchenlist.Add(ord);
                }

                var selectedItem = Instance.Lbx_OrderTableNo.SelectedItem;

                Instance.Lbx_OrderTableNo.ItemsSource = Instance.kitchenlist;
                Instance.Lbx_OrderTableNo.SelectedItem = selectedItem;
                Instance.Dg_ProcessingOrders.Items.Refresh();

            }
            else
            {
                foreach(var ord in Instance.kitchenlist)
                {
                    if(ord.OrderId == order.OrderId)
                    {
                        Instance.kitchenlist.Remove(ord);
                        Instance.orderinKitchen = null;

                        break;
                    }
                }

                Instance.Lbx_OrderTableNo.ItemsSource = Instance.kitchenlist;
            }
        }
    }
}
