using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using S50MVVM.Model;
using S50MVVM.Utilities;
using S50MVVM.View;

namespace S50MVVM.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private ObservableCollection<ListaReproduccion> _listasReproduccion;
        public ObservableCollection<ListaReproduccion> ListasReproduccion
        {
            get { return _listasReproduccion; }
            set
            {
                _listasReproduccion = value;
                OnPropertyChanged(nameof(ListasReproduccion));
            }
        }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand ListBoxDoubleClickCommand { get; set; }
        public ICommand CustomersCommand { get; set; }
        public ICommand ProductsCommand { get; set; }
        public ICommand OrdersCommand { get; set; }
        public ICommand TransactionsCommand { get; set; }
        public ICommand ShipmentsCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Customer(object obj) => CurrentView = new CustomerVM();
        private void Product(object obj) => CurrentView = new ProductVM();
        private void Order(object obj) => CurrentView = new OrderVM();
        private void Transaction(object obj) => CurrentView = new TransactionVM();
        private void Shipment(object obj) => CurrentView = new ShipmentVM();
        private void Setting(object obj) => CurrentView = new SettingVM();

        public NavigationVM()
        {
            ListBoxDoubleClickCommand = new RelayCommand(ListBoxDoubleClick);
            HomeCommand = new RelayCommand(Home);
            CustomersCommand = new RelayCommand(Customer);
            ProductsCommand = new RelayCommand(Product);
            OrdersCommand = new RelayCommand(Order);
            TransactionsCommand = new RelayCommand(Transaction);
            ShipmentsCommand = new RelayCommand(Shipment);
            SettingsCommand = new RelayCommand(Setting);

            // Startup Page
            CurrentView = new HomeVM();
            CargarListasReproduccionDesdeBaseDeDatos();
        }

        // Método para cargar las listas en el ListBox
        private void CargarListasReproduccionDesdeBaseDeDatos()
        {
            ListasReproduccion = new ObservableCollection<ListaReproduccion>();

            string connectionString = ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString;

            string query = "SELECT * FROM ListasReproduccion";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ListasReproduccion.Add(new ListaReproduccion
                    {
                        Id = Convert.ToInt32(reader["PlaylistID"]),
                        NombreLista = reader["NombreLista"].ToString(),
                        URLImagenLista = reader["URLImagenLista"].ToString(),
                    });
                }
                reader.Close();
            }
        }

        private void ListBoxDoubleClick(object selectedItem)
        {
            if (selectedItem is ListaReproduccion playlist)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new CustomerProjectIdentity(Thread.CurrentPrincipal.Identity.Name, playlist.NombreLista), null);
                CurrentView = new Settings { DataContext = new SettingVM { NombrePlaylist = playlist.NombreLista } };
            }
        }
    }
}
