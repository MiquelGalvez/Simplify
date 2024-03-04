using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S50MVVM.Model
{
    public class BBDD
    {
        private bool checkUser(string name)
        {
            string consulta = "SELECT UserID FROM Usuarios WHERE NombreUsuario = @username";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["S50MVVM.Properties.Settings.CampalansSpotiConnectionString"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.Parameters.AddWithValue("@NombreLista", name);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value && result is int)
                        {
                            return true;
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
            return false;
        }
    }
}
