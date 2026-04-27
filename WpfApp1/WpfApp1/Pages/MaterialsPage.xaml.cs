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
    public partial class MaterialsPage : Page
    {
        private readonly УчетСебестоимостиContext _context;
        private Материалы _currentMaterial;
        private ICollectionView _materialsView;

        public MaterialsPage()
        {
            InitializeComponent();
            _context = new УчетСебестоимостиContext();
            LoadData();
        }

        private void LoadData()
        {
            _context.Материалыs.Load();
            MaterialsGrid.ItemsSource = _context.Материалыs.Local.ToObservableCollection();
            ClearForm();

            _materialsView = CollectionViewSource.GetDefaultView(MaterialsGrid.ItemsSource);
            _materialsView.Filter = MaterialsFilter;

            var units = (MaterialsGrid.ItemsSource as System.Collections.IEnumerable)?
                .Cast<object>()
                .Select(m => m?.GetType().GetProperty("ЕдиницаИзмерения")?.GetValue(m) as string)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct()
                .OrderBy(u => u)
                .ToList() ?? new List<string>();

            units.Insert(0, "Все");
            UnitFilterComboBox.ItemsSource = units;
            UnitFilterComboBox.SelectedIndex = 0;
        }

        private void MaterialsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialsGrid.SelectedItem is Материалы selected)
            {
                _currentMaterial = selected;
                TxtName.Text = selected.Название;
                TxtUnit.Text = selected.ЕдиницаИзмерения;
                TxtPrice.Text = selected.ЦенаЗаЕдиницу.ToString("0.##");
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            MaterialsGrid.SelectedItem = null;
            _currentMaterial = null;
            TxtName.Text = string.Empty;
            TxtUnit.Text = string.Empty;
            TxtPrice.Text = "0";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtName.Text.Trim();
            string unit = TxtUnit.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(unit))
            {
                MessageBox.Show("Заполните название и единицу измерения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtPrice.Text.Replace('.', ','), out decimal price) || price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentMaterial == null)
            {
                var newMat = new Материалы { Название = name, ЕдиницаИзмерения = unit, ЦенаЗаЕдиницу = price };
                _context.Материалыs.Add(newMat);
                _currentMaterial = newMat;
            }
            else
            {
                _currentMaterial.Название = name;
                _currentMaterial.ЕдиницаИзмерения = unit;
                _currentMaterial.ЦенаЗаЕдиницу = price;
            }

            try
            {
                _context.SaveChanges();
                MaterialsGrid.Items.Refresh();
                MessageBox.Show("Материал сохранен!", "Ок", MessageBoxButton.OK, MessageBoxImage.Information);
                MaterialsGrid.SelectedItem = _currentMaterial;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка базы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMaterial == null) return;

            if (MessageBox.Show($"Удалить '{_currentMaterial.Название}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _context.Материалыs.Remove(_currentMaterial);
                _context.SaveChanges();
                ClearForm();
            }
        }

        private void MaterialsSearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => _materialsView?.Refresh();
        private void UnitFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => _materialsView?.Refresh();

        private bool MaterialsFilter(object obj)
        {
            if (obj is Материалы item)
            {
                string searchText = MaterialsSearchTextBox.Text?.ToLower() ?? "";
                bool matchesSearch = string.IsNullOrWhiteSpace(searchText) || 
                                     (item.Название != null && item.Название.ToLower().Contains(searchText));

                string selectedUnit = UnitFilterComboBox.SelectedItem as string ?? "Все";
                bool matchesUnit = selectedUnit == "Все" || item.ЕдиницаИзмерения == selectedUnit;

                return matchesSearch && matchesUnit;
            }
            return false;
        }
    }
}
