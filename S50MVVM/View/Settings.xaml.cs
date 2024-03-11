using S50MVVM.Model;
using S50MVVM.ViewModel;
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

namespace S50MVVM.View
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = new SettingVM();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Obtener el DataContext del ListBox (que debería ser tu ViewModel)
            var viewModel = DataContext as SettingVM;

            // Obtener el elemento seleccionado del ListBox
            var selectedSong = ((ListBox)sender).SelectedItem as Cancions;

            // Llamar al comando en el ViewModel pasando la canción seleccionada como parámetro
            viewModel?.ListBoxDoubleClickCommand.Execute(selectedSong);
        }
    }
}
