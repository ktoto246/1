using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class LaborPage : Page
    {
        private readonly УчетСебестоимостиContext _context;
        private Трудозатраты _currentLabor;

        private ICollectionView _laborView;

        public LaborPage()
        {
            InitializeComponent();
            _context = new УчетСебестоимостиContext();
            LoadData();
        }

        private void LoadData()
        {
            _context.Трудозатратыs.Include(t => t.IdПродуктаNavigation).Load();
            _context.Продукцияs.Load();

            LaborGrid.ItemsSource = _context.Трудозатратыs.Local.ToObservableCollection();
            CmbProduct.ItemsSource = _context.Продукцияs.Local.ToObservableCollection();

            _laborView = CollectionViewSource.GetDefaultView(LaborGrid.ItemsSource);
            _laborView.Filter = LaborFilter;

            var products = _context.Трудозатратыs.Local
                .Select(l => l.IdПродуктаNavigation)
                .Where(p => p != null)
                .Distinct()
                .ToList();

            var productList = new List<Продукция> { new Продукция { Название = "Все" } };
            productList.AddRange(products);
            LaborProductFilterComboBox.ItemsSource = productList;
            LaborProductFilterComboBox.SelectedIndex = 0;

            ClearForm();
        }

        private void LaborGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LaborGrid.SelectedItem is Трудозатраты selected)
            {
                _currentLabor = selected;
                CmbProduct.SelectedValue = selected.IdПродукта;
                TxtWorkName.Text = selected.НазваниеРаботы;
                TxtCost.Text = selected.СтоимостьРаботы.ToString("0.##");
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            LaborGrid.SelectedItem = null;
            _currentLabor = null;
            CmbProduct.SelectedIndex = -1;
            TxtWorkName.Text = string.Empty;
            TxtCost.Text = "0";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProduct.SelectedValue == null)
            {
                MessageBox.Show("Выберите продукт из списка!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string workName = TxtWorkName.Text.Trim();
            if (string.IsNullOrEmpty(workName))
            {
                MessageBox.Show("Введите название работы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtCost.Text.Replace('.', ','), out decimal cost) || cost < 0)
            {
                MessageBox.Show("Стоимость должна быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentLabor == null)
            {
                var newLabor = new Трудозатраты
                {
                    IdПродукта = (int)CmbProduct.SelectedValue,
                    НазваниеРаботы = workName,
                    СтоимостьРаботы = cost
                };
                _context.Трудозатратыs.Add(newLabor);
                _currentLabor = newLabor;
            }
            else
            {
                _currentLabor.IdПродукта = (int)CmbProduct.SelectedValue;
                _currentLabor.НазваниеРаботы = workName;
                _currentLabor.СтоимостьРаботы = cost;
            }

            try
            {
                _context.SaveChanges();
                LaborGrid.Items.Refresh();
                MessageBox.Show("Трудозатраты сохранены!", "Ок", MessageBoxButton.OK, MessageBoxImage.Information);
                LaborGrid.SelectedItem = _currentLabor;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLabor == null) return;

            if (MessageBox.Show($"Удалить операцию '{_currentLabor.НазваниеРаботы}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _context.Трудозатратыs.Remove(_currentLabor);
                _context.SaveChanges();
                ClearForm();
            }
        }

        private void LaborSearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => _laborView?.Refresh();
        private void LaborProductFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => _laborView?.Refresh();

        private bool LaborFilter(object obj)
        {
            if (obj is Трудозатраты item)
            {
                string searchText = LaborSearchTextBox.Text?.ToLower() ?? "";
                bool matchesSearch = string.IsNullOrWhiteSpace(searchText) || 
                                     (item.НазваниеРаботы != null && item.НазваниеРаботы.ToLower().Contains(searchText));

                bool matchesProduct = true;
                
                string selectedProduct = "";
                if (LaborProductFilterComboBox.SelectedItem is Продукция prod)
                    selectedProduct = prod.Название;
                else if (LaborProductFilterComboBox.SelectedItem is string s)
                    selectedProduct = s;
                else if (LaborProductFilterComboBox.SelectedItem != null)
                    selectedProduct = LaborProductFilterComboBox.Text; 

                if (selectedProduct != "Все" && !string.IsNullOrWhiteSpace(selectedProduct))
                {
                    matchesProduct = item.IdПродуктаNavigation != null && 
                                     item.IdПродуктаNavigation.Название == selectedProduct;
                }

                return matchesSearch && matchesProduct;
            }
            return false;
        }
    }
}
