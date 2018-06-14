using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ResaturantOrderManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TakeOrdersWindow : Window
    {
        Order order;
        Double totalPrice;

        public static TakeOrdersWindow Instance { get; private set; }
        public ObservableCollection<Order> ordersInProcessingList;

        public TakeOrdersWindow()
        {
            InitializeComponent();

            Instance = this;

            ordersInProcessingList = MyStorage.ReadXML<ObservableCollection<Order>>("ordersInProcessing.xml");

            if(ordersInProcessingList == null)
                ordersInProcessingList = new ObservableCollection<Order>();

            //extrafoods = new ObservableCollection<OrderedFood>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            CoBx_Category.ItemsSource = App.categories;
            CoBx_TableNo.ItemsSource = App.tables;
            Dg_CompletedOrders.ItemsSource = App.ordersCompletedList;
            Dg_ProcessingOrders.ItemsSource = ordersInProcessingList;


            if (App.ordersCompletedList.Count() <= 0)
            {
                Sp_ViewOrderDetails.Visibility = Visibility.Hidden;
                TBl_NoOrders.Visibility = Visibility.Visible;
                TBl_ViewOrders.Visibility = Visibility.Hidden;
            }
            else
            {
                Sp_ViewOrderDetails.Visibility = Visibility.Hidden;
                TBl_NoOrders.Visibility = Visibility.Hidden;
                TBl_ViewOrders.Visibility = Visibility.Visible;
            }

            if (ordersInProcessingList.Count() <= 0)
            {
                Sp_EditOrderDetails.Visibility = Visibility.Hidden;
                TBl_EditNoOrders.Visibility = Visibility.Visible;
                TBl_EditViewOrders.Visibility = Visibility.Hidden;
            }
            else
            {
                Sp_EditOrderDetails.Visibility = Visibility.Hidden;
                TBl_EditNoOrders.Visibility = Visibility.Hidden;
                TBl_EditViewOrders.Visibility = Visibility.Visible;
            }

        }

        internal static void UpdateServedStatus(string orderId, OrderedFood orderedFood)
        {
            foreach ( var ord in Instance.ordersInProcessingList)
            {
                if(ord.OrderId == orderId)
                {
                    foreach (var food in ord.OrderedFoods)
                    {
                        if(food.FoodId == orderedFood.FoodId)
                        {
                            if (food.Quantity == orderedFood.Quantity && food.ServedStatus == false)
                            {
                                food.ServedStatus = true;

                                if (Instance.order !=null && Instance.order.OrderId == ord.OrderId)
                                {
                                    Instance.Sp_CurrentOrderDetails.DataContext = ord;
                                    Instance.Dg_OrderedFoods.Items.Refresh();
                                }
                            }
                            break;
                        }
                    }
                    break;

                }
            }
            MyStorage.WriteXML<ObservableCollection<Order>>("ordersInProcessing.xml", Instance.ordersInProcessingList);
        }

        private void CoBx_TableNo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoBx_TableNo.SelectedItem != null)
            {
                order.TableNo = CoBx_TableNo.SelectedItem.ToString();
                LBx_Food.SelectedItem = null;
            }
        }

        private void Dg_CompletedOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dg = sender as DataGrid;

            if (App.ordersCompletedList.Count <= 0)
            {
                Sp_ViewOrderDetails.Visibility = Visibility.Hidden;
                TBl_NoOrders.Visibility = Visibility.Visible;
                TBl_ViewOrders.Visibility = Visibility.Hidden;
            }
            else if (dg.SelectedItem == null)
            {
                Sp_ViewOrderDetails.Visibility = Visibility.Hidden;
                TBl_NoOrders.Visibility = Visibility.Hidden;
                TBl_ViewOrders.Visibility = Visibility.Visible;
            }
            else
            {
                Sp_ViewOrderDetails.Visibility = Visibility.Visible;
                TBl_NoOrders.Visibility = Visibility.Hidden;
                TBl_ViewOrders.Visibility = Visibility.Hidden;

                double price = 0.0;
                foreach (var food in (dg.SelectedItem as Order).OrderedFoods)
                {
                    price += food.TotalPrice;
                }
                TBx_VTotalPrice.Text = string.Format("{0:C} EUR", price); ;
            }
        }

        private void LBx_Food_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var food = LBx_Food.SelectedItem as Food;
            bool updated = false;

            if (CoBx_TableNo.SelectedItem == null)
            {
                MessageBox.Show("Select the tableNo and proceed");
            }
            else if (order != null && food != null)
            {
                var orderedFood = new OrderedFood { FoodId = food.FoodId, FoodName = food.FoodName, FoodPrice = food.FoodPrice, Quantity = 1, ServedStatus = false };
                orderedFood.TotalPrice = orderedFood.FoodPrice * orderedFood.Quantity;

                foreach (var item in order.OrderedFoods)
                {
                    if (item.FoodId == orderedFood.FoodId)
                    {
                        MessageBox.Show("Food is already added to Orders");

                        updated = true;
                        break;
                    }
                }

                if (!updated)
                {
                    order.OrderedFoods.Add(orderedFood);
                    totalPrice = totalPrice + food.FoodPrice;
                }

                Sp_CurrentOrderDetails.DataContext = order;
                Dg_OrderedFoods.ItemsSource = order.OrderedFoods;
                SetTotalPrice();
            }

        }


        //Buttons OnClick events
        private void Btn_Click_NewOrder(object sender, RoutedEventArgs e)
        {

            EnableDisableComponents();

            var orderId = 0;
            var count = App.ordersCompletedList.Count() + ordersInProcessingList.Count();

            if (count <= 0)
            {
                orderId = 01;
            }
            else
            {
                orderId = count + 1;
            }

            var date = DateTime.Now;

            order = new Order { OrderId = "Order" + orderId, OrderDate = date };

            totalPrice = 0.00;

            Sp_CurrentOrderDetails.DataContext = order;
            Dg_OrderedFoods.ItemsSource = null;
            SetTotalPrice();
        }

        private void Btn_Click_SendOrderKitchen(object sender, RoutedEventArgs e)
        {
            bool orderInKitchen = false;

            if (order.OrderedFoods.Count() > 0)
            {
                foreach (var item in ordersInProcessingList)
                {
                    if (item.OrderId == order.OrderId)
                    {
                        foreach (var food in order.OrderedFoods)
                        {
                            var lst = item.OrderedFoods.Where(S => S.FoodId == food.FoodId)
                                    .Select(S => { S.Quantity = food.Quantity; S.TotalPrice = food.TotalPrice; return S; }).ToList();
                            if (lst == null)
                            {
                                item.OrderedFoods.Add(food);
                            }
                        }

                        orderInKitchen = true;
                        break;
                    }
                }

                if (!orderInKitchen)
                {
                    ordersInProcessingList.Add(order);
                }

                MyStorage.WriteXML<ObservableCollection<Order>>("ordersInProcessing.xml", ordersInProcessingList);
                OrdersInKitchenWindow.UpdateFoodsServed(order);

                order = null;
                Sp_CurrentOrderDetails.DataContext = null;
                Dg_OrderedFoods.ItemsSource = null;
                Dg_ProcessingOrders.ItemsSource = ordersInProcessingList;
                CoBx_TableNo.SelectedItem = null;

                Sp_EditOrderDetails.Visibility = Visibility.Hidden;
                TBl_EditNoOrders.Visibility = Visibility.Hidden;
                TBl_EditViewOrders.Visibility = Visibility.Visible;

                totalPrice = 0.0;
                SetTotalPrice();

                MessageBox.Show("Order is sent to Kitchen");

                EnableDisableComponents();
            }
            else
            {
                MessageBox.Show("No Food is selected to place order!!");
            }

        }

        private void Button_Click_RemoveItem(object sender, RoutedEventArgs e)
        {
            if (Dg_OrderedFoods.SelectedItem != null)
            {
                var selectedItem = Dg_OrderedFoods.SelectedItem as OrderedFood;

                if (selectedItem.ServedStatus)
                {
                    MessageBox.Show("Cannot remove " + selectedItem.FoodName + ". It is already served!");
                }
                else
                {
                    order.OrderedFoods.Remove(selectedItem);
                    totalPrice = totalPrice - selectedItem.TotalPrice;
                    SetTotalPrice();

                    //Dg_OrderedFoods.ItemsSource = order.OrderedFoods;
                     Dg_OrderedFoods.Items.Refresh();


                }

            }
            else
            {
                MessageBox.Show("Select a food to delete");
            }
        }


        private void Button_IncQty(object sender, RoutedEventArgs e)
        {
            if (Dg_OrderedFoods.SelectedItem != null)
            {
                var selectedFood = Dg_OrderedFoods.SelectedItem as OrderedFood;
                
                if (selectedFood.ServedStatus)
                {
                    var food = new OrderedFood { FoodId = selectedFood.FoodId, FoodName = selectedFood.FoodName, FoodPrice = selectedFood.FoodPrice, Quantity = 1, ServedStatus = false };
                    food.TotalPrice = food.Quantity * food.FoodPrice;

                    order.OrderedFoods.Add(food);
                }
                else
                {
                    order.OrderedFoods.Where(S => S.FoodId == selectedFood.FoodId && S.ServedStatus == false)
                                        .Select(S => { S.Quantity += 1; S.TotalPrice = selectedFood.FoodPrice * S.Quantity; return S; }).ToList();
                }

                Dg_OrderedFoods.Items.Refresh();

                totalPrice = totalPrice + selectedFood.FoodPrice;
                SetTotalPrice();
            }
            else
            {
                MessageBox.Show("Select a food to increment");
            }
        }

        private void Button_DecQty(object sender, RoutedEventArgs e)
        {
            if (Dg_OrderedFoods.SelectedItem != null)
            {
                var selectedFood = Dg_OrderedFoods.SelectedItem as OrderedFood;

                if (selectedFood.ServedStatus)
                {
                    MessageBox.Show("Cannot decrease quantity of " + selectedFood.FoodName + ". It is already served!");
                }
                else
                {
                    if (selectedFood.Quantity == 1)
                    {
                        order.OrderedFoods.Remove(selectedFood);
                    }
                    else
                    {
                        order.OrderedFoods.Where(S => S.FoodId == selectedFood.FoodId)
                                        .Select(S => { S.Quantity -= 1; S.TotalPrice = selectedFood.FoodPrice * S.Quantity; return S; }).ToList();
                    }

                    //Dg_OrderedFoods.ItemsSource = order.OrderedFoods;
                    Dg_OrderedFoods.Items.Refresh();

                    totalPrice = totalPrice - selectedFood.FoodPrice;
                    SetTotalPrice();
                }

            }
            else
            {
                MessageBox.Show("Select a food to decrement");
            }
        }

        private void Btn_Click_EditOrder(object sender, RoutedEventArgs e)
        {
            if (order == null)
            {
                EnableDisableComponents();
            }
            order = Dg_ProcessingOrders.SelectedItem as Order;

            tabOrders.SelectedIndex = 0;
            totalPrice = 0.0;

            Sp_CurrentOrderDetails.DataContext = order;
            Dg_OrderedFoods.ItemsSource = order.OrderedFoods;

            CoBx_TableNo.SelectedItem = order.TableNo;
            Dg_ProcessingOrders.SelectedItem = null;

            foreach (var item in order.OrderedFoods)
            {
                totalPrice += item.TotalPrice;
            }
            SetTotalPrice();
        }

        private void EnableDisableComponents()
        {
            if (Btn_NewOrder.IsEnabled)
                Btn_NewOrder.IsEnabled = false;
            else
                Btn_NewOrder.IsEnabled = true;

            if (Grid_TakeOrders.IsEnabled)
                Grid_TakeOrders.IsEnabled = false;
            else
                Grid_TakeOrders.IsEnabled = true;

            if (Sp_CurrentOrderDetails.IsEnabled)
                Sp_CurrentOrderDetails.IsEnabled = false;
            else
                Sp_CurrentOrderDetails.IsEnabled = true;
        }

        private void SetTotalPrice()
        {
            TBx_TotalPrice.Text = string.Format("{0:C} EUR", totalPrice);
        }

        private void Dg_ProcessingOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dg = sender as DataGrid;

            if(ordersInProcessingList.Count <= 0)
            {
                Sp_EditOrderDetails.Visibility = Visibility.Hidden;
                TBl_EditNoOrders.Visibility = Visibility.Visible;
                TBl_EditViewOrders.Visibility = Visibility.Hidden;
            }
            else if (dg.SelectedItem == null)
            {
                Sp_EditOrderDetails.Visibility = Visibility.Hidden;
                TBl_EditNoOrders.Visibility = Visibility.Hidden;
                TBl_EditViewOrders.Visibility = Visibility.Visible;
            }
            else
            {
                Sp_EditOrderDetails.Visibility = Visibility.Visible;
                TBl_EditNoOrders.Visibility = Visibility.Hidden;
                TBl_EditViewOrders.Visibility = Visibility.Hidden;

                double price = 0.0;
                foreach (var food in (dg.SelectedItem as Order).OrderedFoods)
                {
                    price += food.TotalPrice;
                }
                TBx_ETotalPrice.Text = string.Format("{0:C} EUR", price);

            }
        }

        private void Btn_ViewOrder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Thanks for the order! Receipt will be generated");

        }

        private void TBx_FilterFood_TextChanged(object sender, TextChangedEventArgs e)
        {
            var lst = from s in (CoBx_Category.SelectedItem as Category).Foods where s.FoodName.ToLower().Contains(TBx_FilterFood.Text.ToLower()) select s;

            LBx_Food.ItemsSource = lst;
        }

        private void CoBx_Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LBx_Food.ItemsSource = ((sender as ComboBox).SelectedItem as Category).Foods;
        }

        private void Btn_FinishOrder_Click(object sender, RoutedEventArgs e)
        {
            var ord = Dg_ProcessingOrders.SelectedItem as Order;

            bool removeFromProcessingList = true;
            foreach (var food in ord.OrderedFoods)
            {
                if (!food.ServedStatus)
                {
                    removeFromProcessingList = false;
                }
            }

            if (removeFromProcessingList)
            {
                ordersInProcessingList.Remove(ord);
                App.ordersCompletedList.Add(ord);
                OrdersInKitchenWindow.UpdateFoodsServed(ord);

                Dg_ProcessingOrders.ItemsSource = ordersInProcessingList;
                Dg_CompletedOrders.ItemsSource = App.ordersCompletedList;

                MyStorage.WriteXML<ObservableCollection<Order>>("ordersInProcessing.xml", ordersInProcessingList);
                MyStorage.WriteXML<ObservableCollection<Order>>("ordersCompleted.xml", App.ordersCompletedList);

            }
            else
            {
                MessageBox.Show("There are still orders to be served");
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MyStorage.WriteXML<ObservableCollection<Order>>("ordersInProcessing.xml", ordersInProcessingList);
            MyStorage.WriteXML<ObservableCollection<Order>>("ordersCompleted.xml", App.ordersCompletedList);
        }
    }
}
