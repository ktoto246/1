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
    public partial class CompositionPage : Page
    {
        private readonly УчетСебестоимостиContext _context;
        private СоставПродукции _currentComposition;

        // Поле
        private ICollectionView _compositionView;

        public CompositionPage()
        {
            InitializeComponent();
            _context = new УчетСебестоимостиContext();
            LoadData();
        }

        private void LoadData()
        {
            _context.СоставПродукцииs.Include(c => c.IdПродуктаNavigation).Include(c => c.IdМатериалаNavigation).Load();
            _context.Продукцияs.Load();
            _context.Материалыs.Load();

            CompositionGrid.ItemsSource = _context.СоставПродукцииs.Local.ToObservableCollection();
            CmbProduct.ItemsSource = _context.Продукцияs.Local.ToObservableCollection();
            CmbMaterial.ItemsSource = _context.Материалыs.Local.ToObservableCollection();

            // В конструкторе страницы (после присвоения ItemsSource для CompositionDataGrid):
            _compositionView = CollectionViewSource.GetDefaultView(CompositionGrid.ItemsSource);
            _compositionView.Filter = CompositionFilter;

            // Подгружаем список продуктов из ItemsSource (Local)
            var products = _context.СоставПродукцииs.Local
                .Select(c => c.IdПродуктаNavigation)
                .Where(p => p != null)
                .Distinct()
                .ToList();

            var productList = new List<Продукция> { new Продукция { Название = "Все" } };
            productList.AddRange(products);
            CompositionProductFilterComboBox.ItemsSource = productList;
            CompositionProductFilterComboBox.SelectedIndex = 0;

            ClearForm();
        }

        private void CompositionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompositionGrid.SelectedItem is СоставПродукции selected)
            {
                _currentComposition = selected;
                CmbProduct.SelectedValue = selected.IdПродукта;
                CmbMaterial.SelectedValue = selected.IdМатериала;
                TxtQuantity.Text = selected.Количество.ToString("0.####");

                // Блокируем комбобоксы, так как нельзя менять первичный ключ EF Core "на лету"
                CmbProduct.IsEnabled = false;
                CmbMaterial.IsEnabled = false;
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            CompositionGrid.SelectedItem = null;
            _currentComposition = null;
            CmbProduct.SelectedIndex = -1;
            CmbMaterial.SelectedIndex = -1;
            TxtQuantity.Text = "0";

            // Разблокируем комбобоксы для создания новой записи
            CmbProduct.IsEnabled = true;
            CmbMaterial.IsEnabled = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProduct.SelectedValue == null || CmbMaterial.SelectedValue == null)
            {
                MessageBox.Show("Выберите продукт и материал!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtQuantity.Text.Replace('.', ','), out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int prodId = (int)CmbProduct.SelectedValue;
            int matId = (int)CmbMaterial.SelectedValue;

            if (_currentComposition == null)
            {
                // Проверка на дубликаты
                bool exists = _context.СоставПродукцииs.Any(c => c.IdПродукта == prodId && c.IdМатериала == matId);
                if (exists)
                {
                    MessageBox.Show("Этот материал уже добавлен в данный продукт! Измените существующую запись.", "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newComp = new СоставПродукции
                {
                    IdПродукта = prodId,
                    IdМатериала = matId,
                    Количество = quantity
                };
                _context.СоставПродукцииs.Add(newComp);
                _currentComposition = newComp;
            }
            else
            {
                // Если редактируем — меняем только количество (ключи заблокированы)
                _currentComposition.Количество = quantity;
            }

            try
            {
                _context.SaveChanges();
                CompositionGrid.Items.Refresh();
                MessageBox.Show("Рецептура сохранена!", "Ок", MessageBoxButton.OK, MessageBoxImage.Information);
                CompositionGrid.SelectedItem = _currentComposition;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentComposition == null) return;

            if (MessageBox.Show("Удалить этот материал из рецепта?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _context.СоставПродукцииs.Remove(_currentComposition);
                _context.SaveChanges();
                ClearForm();
            }
        }

        // Обработчик
        private void CompositionProductFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => _compositionView?.Refresh();

        // Фильтр
        private bool CompositionFilter(object obj)
        {
            if (obj is СоставПродукции item)
            {
                bool matchesProduct = true;
                
                string selectedProduct = "";
                if (CompositionProductFilterComboBox.SelectedItem is Продукция prod)
                    selectedProduct = prod.Название;
                else if (CompositionProductFilterComboBox.SelectedItem is string s)
                    selectedProduct = s;
                else if (CompositionProductFilterComboBox.SelectedItem != null)
                    selectedProduct = CompositionProductFilterComboBox.Text;

                if (selectedProduct != "Все" && !string.IsNullOrWhiteSpace(selectedProduct))
                {
                    matchesProduct = item.IdПродуктаNavigation != null && 
                                     item.IdПродуктаNavigation.Название == selectedProduct;
                }

                return matchesProduct;
            }
            return false;
        }
    }
}
