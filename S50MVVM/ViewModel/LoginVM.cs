using S50MVVM.Utilities;
using S50MVVM;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Input;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Security.Cryptography.X509Certificates;
using S50MVVM.Model;
using System.Security.Principal;
using System.Threading;

namespace S50MVVM.ViewModel
{
    class LoginVM : Utilities.ViewModelBase
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private void Login(object parameter)
        {
            string username = Username.Trim();
            string password = Password.Trim();
            // Resto del código...
        }

        string connectionString = ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString;

        public ICommand LoginCommand { get; }

        public LoginVM()
        {
            LoginCommand = new RelayCommand(CanLogin);
        }

        private void CanLogin(object parameter)
        {
            var login = new Login();
            // Obtener los valores del TextBox y PasswordBox
            string username = Username.Trim();
            string password = Password.Trim();
            // Consulta SQL para verificar la existencia del usuario
            string query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @Username AND Contraseña = @Password";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    connection.Open();

                    int count = (int)command.ExecuteScalar(); // Obtener el recuento de filas que coinciden con las credenciales

                    if (count > 0)
                    {
                        Thread.CurrentPrincipal = new GenericPrincipal(new CustomerProjectIdentity(username.ToLower(), ""), null);
                        var mainwin = new MainWindow();
                        mainwin.Show();
                        login.Close();
                    }
                    else
                    {
                        // El usuario no existe o las credenciales son incorrectas
                        MessageBox.Show("Nombre de usuario o contraseña incorrectos");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar iniciar sesión: " + ex.Message);
            }
        }
    }
}
