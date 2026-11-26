using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace kursach_apteca
{
    /// <summary>
    /// Логика взаимодействия для Medicines_Page.xaml
    /// </summary>
    public partial class Medicines_Page : Page
    {
        public ObservableCollection<CartItem> CartItems { get; } = new ObservableCollection<CartItem>();
        private Buyers selectedBuyer;
        private bool _isInitialized = false;
      


        public Medicines_Page(Sellers seller)
        {
            InitializeComponent();
            LoadSellers();
            var currentDrugs = PharmacyEntities.GetContext().Drugs.ToList();
            MedicinesListView.ItemsSource = currentDrugs;
            ComboType.SelectedIndex = 0;
            UpdateDrugs();
            this.Loaded += Medicines_Page_Loaded;

            FIOTB.Text = $"{seller.LastName} {seller.FirstName} ";

            if (seller.Role == 2) RoleTB.Text = "Кассир-администратор";
            else
            {
                RoleTB.Text = "Кассир";
                BtnDrudsAdd.Visibility = Visibility.Collapsed;
            }
        }
        private void Medicines_Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            UpdateDrugs();
            ComboType.SelectedIndex = 0;
       
        }


        private void UpdateDrugs()
        {
            if (ComboType == null || TBSearch == null || MedicinesListView == null)
            {
                return;
            }

            var currentDrugs = PharmacyEntities.GetContext().Drugs.ToList();


            if (ComboType.SelectedIndex == 1)
            {
                currentDrugs = currentDrugs.Where(p => p.Categoty == "Лекарство").ToList();
            }
            else if (ComboType.SelectedIndex == 2)
            {
                currentDrugs = currentDrugs.Where(p => p.Categoty == "БАД").ToList();
            }
            else if (ComboType.SelectedIndex == 3)
            {
                currentDrugs = currentDrugs.Where(p => p.Categoty == "Косметика и уход").ToList();
            }

            currentDrugs = currentDrugs.Where(p => p.Name.ToLower().Contains(TBSearch.Text.ToLower()) || p.ViewN.ToLower().Contains(TBSearch.Text.ToLower())).ToList();

            MedicinesListView.ItemsSource = currentDrugs;

            



        }
        
       
      

        private void MedicinesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void MedicinesListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            if (FindVisualParent<Button>(source) != null)
            {
                return;
            }

            var listViewItem = FindVisualParent<ListViewItem>(source);
            var selectedDrug = listViewItem?.Content as Drugs;
            if (selectedDrug != null && NavigationService != null)
            {
                e.Handled = true;
                NavigationService.Navigate(new DrugDetailsPage(selectedDrug));
            }
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                var parent = child as T;
                if (parent != null)
                {
                    return parent;
                }

                // e.OriginalSource может быть не Visual, а текстовым элементом Run.
                // VisualTreeHelper.GetParent поддерживает только Visual/Visual3D.
                if (child is Visual || child is System.Windows.Media.Media3D.Visual3D)
                {
                    child = VisualTreeHelper.GetParent(child);
                }
                else if (child is FrameworkContentElement frameworkContentElement)
                {
                    child = frameworkContentElement.Parent;
                }
                else if (child is ContentElement contentElement)
                {
                    child = ContentOperations.GetParent(contentElement);
                }
                else
                {
                    child = null;
                }
            }

            return null;
        }

        private void LoadSellers()
        {
            var sellers = PharmacyEntities.GetContext().Sellers
                .ToList()
                .Select(s => new SellerOption
                {
                    SellerID = s.SellerID,
                    FullName = GetFullName(s)
                })
                .ToList();

            CBSeller.ItemsSource = sellers;
            if (sellers.Count == 1)
            {
                CBSeller.SelectedIndex = 0;
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var drug = button?.DataContext as Drugs;
            if (drug == null)
            {
                return;
            }

            var item = CartItems.FirstOrDefault(i => i.Drug.DrugID == drug.DrugID);
            if (item == null)
            {
                CartItems.Add(new CartItem { Drug = drug, Quantity = 1 });
            }
            else if (item.Quantity < drug.Stock)
            {
                item.Quantity++;
            }
            else
            {
                MessageBox.Show("Нельзя добавить больше товара, чем есть на складе.", "Корзина", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as CartItem;
            if (item != null)
            {
                CartItems.Remove(item);
            }
        }

        private void CartQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var item = textBox?.DataContext as CartItem;
            int quantity;
            if (item != null && int.TryParse(textBox.Text, out quantity))
            {
                item.Quantity = quantity < 1 ? 1 : quantity;
            }
        }

        private void TBCustomerPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            var phone = (TBCustomerPhone.Text ?? string.Empty).Trim();
            selectedBuyer = PharmacyEntities.GetContext().Buyers
                .ToList()
                .FirstOrDefault(b => NormalizePhone(b.Phone) == NormalizePhone(phone));

            CustomerStatus.Text = selectedBuyer == null
                ? (string.IsNullOrWhiteSpace(phone) ? "Введите телефон клиента" : "Клиент не найден — сначала добавьте его")
                : "Клиент добавлен: " + GetFullName(selectedBuyer);
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в чек.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var hasCustomerPhone = !string.IsNullOrWhiteSpace(TBCustomerPhone.Text);
            if (hasCustomerPhone && selectedBuyer == null)
            {
                MessageBox.Show("Укажите телефон существующего клиента.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Warning);
                TBCustomerPhone.Focus();
                return;
            }

            var seller = CBSeller.SelectedItem as SellerOption;
            if (seller == null)
            {
                MessageBox.Show("Выберите продавца.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Warning);
                CBSeller.Focus();
                return;
            }

            foreach (var item in CartItems)
            {
                if (item.Quantity < 1 || item.Quantity > item.Drug.Stock)
                {
                    MessageBox.Show("Проверьте количество товара «" + item.Drug.Name + "» и остаток на складе.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (item.Drug.IsPrescription &&
                    (string.IsNullOrWhiteSpace(item.DoctorName) ||
                     !item.IssueDate.HasValue ||
                     item.IssueDate.Value.Date > DateTime.Today))
                {
                    if (item.IssueDate.HasValue && item.IssueDate.Value.Date > DateTime.Today)
                    {
                        MessageBox.Show(
                            "Дата выдачи рецепта не может быть будущей.",
                            "Оплата",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Для рецептурного товара \"" + item.Drug.Name + "\" укажите ФИО врача и дату выдачи рецепта.",
                            "Оплата",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }

                    return;
                }

                if (item.Drug.IsPrescription && selectedBuyer == null)
                {
                    MessageBox.Show("Для рецептурного товара необходимо указать телефон клиента.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TBCustomerPhone.Focus();
                    return;
                }
            }

            var context = PharmacyEntities.GetContext();
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    foreach (var item in CartItems)
                    {
                        Recipes recipe = null;
                        if (item.Drug.IsPrescription)
                        {
                            recipe = new Recipes
                            {
                                BuyerID = selectedBuyer.BuyerID,
                                IssueDate = item.IssueDate.Value,
                                DoctorName = item.DoctorName,
                                Diagnosis = "Дата выдачи рецепта: " + item.IssueDate.Value.ToString("dd.MM.yyyy"),
                                DrugID = item.Drug.DrugID
                            };
                            context.Recipes.Add(recipe);
                        }

                        item.Drug.Stock -= item.Quantity;
                        context.Sales.Add(new Sales
                        {
                            SaleDate = DateTime.Today,
                            Quantity = item.Quantity,
                            DrugID = item.Drug.DrugID,
                            BuyerID = selectedBuyer == null ? (int?)null : selectedBuyer.BuyerID,
                            SellerID = seller.SellerID,
                            Recipes = recipe,
                            ByPrescription = item.Drug.IsPrescription
                        });
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }

                CartItems.Clear();
                UpdateDrugs();
                MessageBox.Show("Продажа успешно оформлена.", "Оплата", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось оформить продажу:\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string NormalizePhone(string phone)
        {
            return new string((phone ?? string.Empty).Where(char.IsDigit).ToArray()).TrimStart('7').TrimStart('8');
        }

        private static string GetFullName(Buyers buyer)
        {
            return string.Join(" ", new[] { buyer.LastName, buyer.FirstName, buyer.Patronymic }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string GetFullName(Sellers seller)
        {
            return string.Join(" ", new[] { seller.LastName, seller.FirstName, seller.Patronymic }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Если текст пустой — показываем подсказку, иначе скрываем
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(TBSearch.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            UpdateDrugs();
        }


        private void ComboType_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            UpdateDrugs();
        }


        private void BtnSales_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SalesDrug());
        }


        private void BtnExpiry_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new BestBeforDatePage());
        }

        
        private void BtnDrudsAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddNewDrugPage());
        }

        private void BtnNewBuyers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddNewBuyersPage());
        }

    public class CartItem
    {
        public Drugs Drug { get; set; }
        public int Quantity { get; set; }
        public string DoctorName { get; set; }
        public string Diagnosis { get; set; }
        public DateTime? IssueDate { get; set; }
    }

    public class SellerOption
    {
        public int SellerID { get; set; }
        public string FullName { get; set; }
    }
    }
}
    
