using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProductsPage());
        }

        private void BtnMaterials_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.MaterialsPage());
        }

        private void BtnLabor_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.LaborPage());
        }

        private void BtnComposition_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.CompositionPage());
        }
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.DashboardPage());
        }
    }
}