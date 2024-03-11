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

        // Mètode per a navegar a la pàgina d'inici
        private void Home(object obj) => CurrentView = new HomeVM();

        // Mètode per a navegar a la pàgina de clients
        private void Customer(object obj) => CurrentView = new CustomerVM();

        // Mètode per a navegar a la pàgina de configuració
        private void Setting(object obj) => CurrentView = new SettingVM();

        public NavigationVM()
        {
            ListBoxDoubleClickCommand = new RelayCommand(ListBoxDoubleClick);
            HomeCommand = new RelayCommand(Home);
            CustomersCommand = new RelayCommand(Customer);
            SettingsCommand = new RelayCommand(Setting);

            // Pàgina d'inici
            CurrentView = new HomeVM();
            CargarListasReproduccionDesdeBaseDeDatos(Thread.CurrentPrincipal.Identity.Name);
        }

        // Mètode per a carregar les llistes en el ListBox
        private void CargarListasReproduccionDesdeBaseDeDatos(string nombreUsuario)
        {
            // Truca al mètode de BBDD per a carregar les llistes de reproducció
            ListasReproduccion = BBDD.CargarListasReproduccionDesdeBaseDeDatos(nombreUsuario);
        }

        // Mètode per a gestionar el doble clic en el ListBox
        private void ListBoxDoubleClick(object selectedItem)
        {
            if (selectedItem is ListaReproduccion playlist)
            {
                // Estableix el nom de la llista com a identitat de l'usuari actual
                Thread.CurrentPrincipal = new GenericPrincipal(new CustomerProjectIdentity(Thread.CurrentPrincipal.Identity.Name, playlist.NombreLista), null);

                // Navega a la pàgina de configuració amb el context de la llista seleccionada
                CurrentView = new Settings { DataContext = new SettingVM { NombrePlaylist = playlist.NombreLista } };
            }
        }
    }
}
