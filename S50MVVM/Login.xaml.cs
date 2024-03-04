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
using System.Windows.Shapes;
using S50MVVM.ViewModel; // Asegúrate de importar el espacio de nombres donde se encuentra LoginVM

namespace S50MVVM
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            DataContext = new LoginVM(); // Establece el ViewModel como el contexto de datos de la ventana}
        }
    }
}

