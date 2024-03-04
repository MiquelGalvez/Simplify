using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using S50MVVM.Model;

namespace S50MVVM.ViewModel
{
    class SettingVM: Utilities.ViewModelBase
    {
        
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Cancions> _canciones;
        public ObservableCollection<Cancions> Canciones
        {
            get { return _canciones; }
            set
            {
                _canciones = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Canciones)));
            }
        }

        public string NombrePlaylist { get; set; }


        private TimeSpan _duracionSeleccionada;
        public TimeSpan DuracionSeleccionada
        {
            get { return _duracionSeleccionada; }
            set
            {
                _duracionSeleccionada = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DuracionSeleccionada)));
            }
        }

        private TimeSpan _tiempoTranscurrido;
        public TimeSpan TiempoTranscurrido
        {
            get { return _tiempoTranscurrido; }
            set
            {
                _tiempoTranscurrido = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TiempoTranscurrido)));
            }
        }

        private double _progresoReproduccion;
        public double ProgresoReproduccion
        {
            get { return _progresoReproduccion; }
            set
            {
                _progresoReproduccion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgresoReproduccion)));
            }
        }

        private DispatcherTimer _timer;
        private DateTime _inicioReproduccion;

        public SettingVM()
        {
            // Inicializar la colección de canciones
            Canciones = new ObservableCollection<Cancions>();

            // Establecer valores iniciales para TiempoTranscurrido y DuracionSeleccionada
            TiempoTranscurrido = TimeSpan.Zero;
            DuracionSeleccionada = TimeSpan.MaxValue; // O cualquier otro valor inicial razonable

            // Llamar al método para cargar canciones desde la base de datos
            CargarCancionesDesdeBaseDeDatos();

          

            // Resto del código de inicialización del timer, etc.
        }
        private int ObtenerIdPlaylistPorNombre(string nomplaylist)
        {
            int playlistId; // Inicializamos el ID como -1 para indicar que no se ha encontrado ninguna lista de reproducción con ese nombre

            string consulta = "SELECT PlaylistID FROM ListasReproduccion WHERE NombreLista = @NombreLista";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@NombreLista", nomplaylist);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value && result is int)
                        {
                            return playlistId = (int)result;
                        }
                        else
                        {
                            // El resultado no es del tipo esperado o es nulo
                            Console.WriteLine("El resultado de ExecuteScalar no es del tipo esperado o es nulo.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción adecuadamente (por ejemplo, registrándola o lanzándola nuevamente)
                        Console.WriteLine("Error al obtener el ID de la lista de reproducción desde la base de datos: " + ex.Message);
                    }
                }
            }
            return -1;
        }

        private void CargarCancionesDesdeBaseDeDatos()
        {
            // Obtener el ID de la lista de reproducción utilizando su nombre
            int playlistId = ObtenerIdPlaylistPorNombre(((CustomerProjectIdentity)Thread.CurrentPrincipal.Identity).Llista);
            
            if (playlistId <= 0)
            {
                // Si no se encuentra la lista de reproducción, puedes manejarlo aquí
                // Por ejemplo, mostrar un mensaje de error
                Console.WriteLine("La lista de reproducción no se encontró en la base de datos.");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString;

            // Consulta para seleccionar las canciones de la lista de reproducción actual
            string query = @"SELECT C.*
                FROM CancionesEnListaReproduccion CLR
                INNER JOIN Canciones C ON CLR.SongID = C.SongID
                WHERE CLR.PlaylistID = @PlaylistId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlaylistId", playlistId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Canciones.Add(new Cancions
                        {
                            Titulo = reader["Titulo"].ToString(),
                            Artista = reader["Artista"].ToString(),
                            Album = reader["Album"].ToString(),
                            Duracio = TimeSpan.Parse(reader["Duracion"].ToString()),
                            Genere = reader["Genero"].ToString(),
                            AnyLlancement = reader["AnioLanzamiento"].ToString(),
                            URLImagenAlbum = reader["URLImagenAlbum"].ToString()
                        });
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Manejar la excepción adecuadamente (por ejemplo, registrándola o lanzándola nuevamente)
                    Console.WriteLine("Error al cargar canciones desde la base de datos: " + ex.Message);
                }
            }
        }

        private void IniciarReproduccion(TimeSpan duracion)
        {
            DuracionSeleccionada = duracion;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TiempoTranscurrido = DateTime.Now - _inicioReproduccion;
            ProgresoReproduccion = TiempoTranscurrido.TotalSeconds / DuracionSeleccionada.TotalSeconds * 100; // Calcular el progreso en porcentaje

            if (TiempoTranscurrido >= DuracionSeleccionada)
            {
                _timer.Stop();
                TiempoTranscurrido = TimeSpan.Zero;
                ProgresoReproduccion = 0; // Restablecer el progreso de reproducción
            }
        }

        private RelayCommand _iniciarReproduccionCommand;
        public ICommand IniciarReproduccionCommand
        {
            get
            {
                if (_iniciarReproduccionCommand == null)
                {
                    _iniciarReproduccionCommand = new RelayCommand(ExecuteIniciarReproduccionCommand);
                }
                return _iniciarReproduccionCommand;
            }
        }

        private void ExecuteIniciarReproduccionCommand(object parameter)
        {
            if (parameter is Cancions cancion)
            {
                // Iniciar reproducción y establecer la duración de la canción seleccionada
                IniciarReproduccion(cancion.Duracio);

                // Reiniciar la barra de progreso
                TiempoTranscurrido = TimeSpan.Zero;
                ProgresoReproduccion = 0;
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
