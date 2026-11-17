using System;
using System.Collections.Generic;
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


namespace kursach_apteca
{
    /// <summary>
    /// Логика взаимодействия для Medicines_Page.xaml
    /// </summary>
    public partial class Medicines_Page : Page
    {
        private int _currentPage = 1;
        private int _pageSize = 10; // Сколько лекарств показывать на одной странице
        private int _totalPages = 1;
        public Medicines_Page()
        {
            InitializeComponent();
            var currentDrugs = PharmacyEntities.GetContext().Drugs.ToList();
            MedicinesListView.ItemsSource = currentDrugs;
            ComboType.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            UpdateDrugs();
            this.Loaded += Medicines_Page_Loaded;

        }
        private void Medicines_Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Вызываем ваш метод обновления списка
            UpdateDrugs();

            // Сбрасываем фильтры
            ComboType.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
        }


        private void UpdateDrugs()
        {
            var currentDrugs = PharmacyEntities.GetContext().Drugs.ToList();


            if (ComboType.SelectedIndex == 1)
            {
                currentDrugs = currentDrugs.Where(p => p.Type == "Таблетки").ToList();
            }
            else if (ComboType.SelectedIndex == 2)
            {
                currentDrugs = currentDrugs.Where(p => p.Type == "Капли").ToList();
            }
            else if (ComboType.SelectedIndex == 3)
            {
                currentDrugs = currentDrugs.Where(p => p.Type == "Раствор").ToList();
            }

            currentDrugs = currentDrugs.Where(p => p.Name.ToLower().Contains(TBSearch.Text.ToLower()) || p.Type.ToLower().Contains(TBSearch.Text.ToLower()) || p.ViewN.ToLower().Contains(TBSearch.Text.ToLower())).ToList();

            MedicinesListView.ItemsSource = currentDrugs.ToList();

            if (ComboSort.SelectedIndex == 0)
            {
                MedicinesListView.ItemsSource = currentDrugs;
            }
            if (ComboSort.SelectedIndex == 1)
            {
                MedicinesListView.ItemsSource = currentDrugs.OrderBy(p => p.Name).ToList();
            }
            if (ComboSort.SelectedIndex == 2)
            {
                MedicinesListView.ItemsSource = currentDrugs.OrderBy(p => p.Cost).ToList();
            }
            else if (ComboSort.SelectedIndex == 3)
            {
                MedicinesListView.ItemsSource = currentDrugs.OrderByDescending(p => p.Cost).ToList();
            }



        }
        private void BtnAddMedicine_Click(object sender, RoutedEventArgs e)
        {
            // Переходим на страницу без передачи данных (для добавления)
            NavigationService.Navigate(new addEditPage());
        }
        private void BtnEditMedicine_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var selectedDrug = btn.Tag as Drugs; // Получаем лекарство из тега

            if (selectedDrug != null)
            {
                // Переходим на страницу и передаем туда объект (для редактирования)
                NavigationService.Navigate(new addEditPage(selectedDrug));
            }

        }
        private void BtnDeleteDrug_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        { }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        { }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDrugs();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDrugs();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDrugs();
        }

     
        
    }
}
    
