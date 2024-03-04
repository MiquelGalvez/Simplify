using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using S50MVVM.Model;
using System.Configuration;
using System.Threading;
using S50MVVM.Utilities;
using System.Windows;

namespace S50MVVM.ViewModel
{
    class HomeVM : ViewModelBase
    {
        private string _usuari;
        public string Usuari
        {
            get { return _usuari; }
            set
            {
                _usuari = value;
                OnPropertyChanged(nameof(Usuari));
            }
        }
        public HomeVM()
        {
            Usuari = Thread.CurrentPrincipal.Identity.Name;
        }
    }
}
