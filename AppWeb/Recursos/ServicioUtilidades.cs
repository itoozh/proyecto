using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration; // Asegúrate de tener esta referencia

namespace AppWeb.Recursos
{
    public class ServicioUtilidades
    {
        private readonly IConfiguration _configuration;

        public ServicioUtilidades(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool ValidarRegistroUsuario(string correo, string cedula)
        {
            bool existeCorreo = ExisteCorreo(correo);
            bool existeCedula = ExisteCedula(cedula);

            return existeCorreo || existeCedula;
        }

        public bool ExisteCorreo(string correo)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                using (SqlCommand cmd = new SqlCommand("ValidarRegistroUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@correo", correo));
                    cmd.Parameters.Add(new SqlParameter("@cedula", DBNull.Value)); // Puedes pasar DBNull.Value si no estás usando cédula en esta verificación
                    cmd.Parameters.Add(new SqlParameter("@existeCorreo", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@existeCedula", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return (bool)cmd.Parameters["@existeCorreo"].Value;
                }
            }
        }

        public bool ExisteCedula(string cedula)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("CadenaSQL")))
            {
                using (SqlCommand cmd = new SqlCommand("ValidarRegistroUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@correo", DBNull.Value)); // Puedes pasar DBNull.Value si no estás usando correo en esta verificación
                    cmd.Parameters.Add(new SqlParameter("@cedula", cedula));
                    cmd.Parameters.Add(new SqlParameter("@existeCorreo", SqlDbType.Bit) { Direction = ParameterDirection.Output });
                    cmd.Parameters.Add(new SqlParameter("@existeCedula", SqlDbType.Bit) { Direction = ParameterDirection.Output });

                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return (bool)cmd.Parameters["@existeCedula"].Value;
                }
            }
        }
    }
}
