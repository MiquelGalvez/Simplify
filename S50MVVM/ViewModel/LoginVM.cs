using S50MVVM.Utilities;
using System.Windows.Input;
using System.Windows;
using System;
using S50MVVM.Model;
using System.Threading;
using System.Security.Principal;
using System.Security;

namespace S50MVVM.ViewModel
{
    class LoginVM : ViewModelBase
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

        public ICommand LoginCommand { get; }

        public LoginVM()
        {
            // Comanda per a l'inici de sessió
            LoginCommand = new RelayCommand(Login);
        }


        // Funció que comprova els valors de password i user i ens permet fer login
        private void Login(object parameter)
        {
            // Obté els valors del TextBox i PasswordBox
            string username = Username.Trim();
            string password = Password.Trim();

            try
            {
                // Truca al mètode d'autenticació en la classe BBDD
                bool isAuthenticated = BBDD.AuthenticateUser(username, password);

                if (isAuthenticated)
                {
                    // Estableix el contexte de seguretat de l'usuari
                    Thread.CurrentPrincipal = new GenericPrincipal(new CustomerProjectIdentity(username.ToLower(), ""), null);

                    // Obre la finestra principal
                    var mainWin = new MainWindow();
                    mainWin.Show();

                    // Tanca la finestra d'inici de sessió
                    if (parameter is Window loginWindow)
                    {
                        loginWindow.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Nom d'usuari o contrasenya incorrectes");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en intentar iniciar sessió: " + ex.Message);
            }
        }
    }
}
