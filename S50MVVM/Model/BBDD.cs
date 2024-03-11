using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using S50MVVM.Model;

namespace S50MVVM.Model
{
    public class BBDD
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString;

        // Carrega les llistes de reproducció de la base de dades que te el usuari que fa login guardades
        public static ObservableCollection<ListaReproduccion> CargarListasReproduccionDesdeBaseDeDatos(string nombreUsuario)
        {
            ObservableCollection<ListaReproduccion> listasReproduccion = new ObservableCollection<ListaReproduccion>();

            // Buscar el UserID correspondiente al nombre de usuario
            string queryUsuario = "SELECT UserID FROM Usuarios WHERE NombreUsuario = @NombreUsuario";

            int userId;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand commandUsuario = new SqlCommand(queryUsuario, connection);
                commandUsuario.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                connection.Open();
                userId = (int)commandUsuario.ExecuteScalar();
            }

            // Cargar las listas de reproducción asociadas al UserID
            string queryListas = "SELECT * FROM ListasReproduccion WHERE UsuarioID = @UsuarioID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand commandListas = new SqlCommand(queryListas, connection);
                commandListas.Parameters.AddWithValue("@UsuarioID", userId);
                connection.Open();
                SqlDataReader reader = commandListas.ExecuteReader();

                while (reader.Read())
                {
                    listasReproduccion.Add(new ListaReproduccion
                    {
                        Id = Convert.ToInt32(reader["PlaylistID"]),
                        NombreLista = reader["NombreLista"].ToString(),
                        URLImagenLista = reader["URLImagenLista"].ToString(),
                    });
                }
                reader.Close();
            }

            return listasReproduccion;
        }

        // Retorna el id de la playlist en funcio del nom de la mateixa
        public static int ObtenerIdPlaylistPorNombre(string nomplaylist)
        {
            int playlistId = -1;

            string consulta = "SELECT PlaylistID FROM ListasReproduccion WHERE NombreLista = @NombreLista";

            using (SqlConnection conn = new SqlConnection(connectionString))
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
                            playlistId = (int)result;
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

            return playlistId;
        }

        // Funcio que carrega les cançons de la base de dades en funcio a la playlist a la que perteneixen
        public static ObservableCollection<Cancions> CargarCancionesDesdeBaseDeDatos(string nombrePlaylist)
        {
            ObservableCollection<Cancions> canciones = new ObservableCollection<Cancions>();

            int playlistId = ObtenerIdPlaylistPorNombre(nombrePlaylist);

            if (playlistId <= 0)
            {
                // Si no se encuentra la lista de reproducción, puedes manejarlo aquí
                // Por ejemplo, mostrar un mensaje de error
                Console.WriteLine("La lista de reproducción no se encontró en la base de datos.");
                return canciones;
            }

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
                        canciones.Add(new Cancions
                        {
                            Titulo = reader["Titulo"].ToString(),
                            Artista = reader["Artista"].ToString(),
                            Album = reader["Album"].ToString(),
                            Duracio = TimeSpan.Parse(reader["Duracion"].ToString()),
                            Genere = reader["Genero"].ToString(),
                            AnyLlancement = reader["AnioLanzamiento"].ToString(),
                            URLImagenAlbum = reader["URLImagenAlbum"].ToString(),
                            URLCanco = reader["URLCancion"].ToString()
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

            return canciones;
        }

        // Funcio que comprova a la base de dades si els valors introduits perteneixen a un usuari ja registrat
        public static bool AuthenticateUser(string username, string password)
        {
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

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al autenticar al usuario: " + ex.Message);
            }
        }
    }
}
