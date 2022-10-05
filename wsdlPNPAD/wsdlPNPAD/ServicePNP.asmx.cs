using System;
using System.DirectoryServices;
using System.Web.Services;
using System.Collections;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.DirectoryServices.AccountManagement;
using System.Net;
using Xceed.Wpf.Toolkit;
using Microsoft.Azure.Documents;
using SearchScope = System.DirectoryServices.SearchScope;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using Newtonsoft;

using System.Management.Instrumentation;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace wsdlPNPAD
{
    /// <summary>
    /// Descripción breve de PNP_AD
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class PNP_AD : System.Web.Services.WebService
    {
      //  private string ldapPath = "LDAP://" + ConfigurationManager.AppSettings["ldap.host"] + "/DC=intranetpnp,DC=pnp";
       
        [WebMethod()]
        public bool ValidaUsuario(string user, string pass)
        {
            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));
            DirectoryEntry directoryEntry = new DirectoryEntry();
            directoryEntry.Path = path;
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;


            // LLEVARLO AL WEB CONFIG

            //directoryEntry.Username = ConfigurationManager.AppSettings["ldap.username"];
            //directoryEntry.Password = ConfigurationManager.AppSettings["ldap.password"];


            directoryEntry.Username = user.ToString();
            directoryEntry.Password = pass.ToString();


            string filter = Convert.ToString("sAMAccountName=") + user;
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, filter);
            directorySearcher.SearchScope = SearchScope.Subtree;
           
            bool bSucceded = false;
            try
            {
                SearchResult searchResult = directorySearcher.FindOne();

                bSucceded = true;
               
                directoryEntry.Close();

            }
            catch (Exception ex)
            {
                string strError = ex.Message;
            
                directoryEntry.Close();
            }
            return bSucceded;
        }


        private static async Task<dynamic> AnalizaTransaccion(string cip)
        {

            string responseString = "";

            dynamic data = new ResponseAnalizador();

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("API-USER", "PNP");
            string url = "https://ticnowidnow2fa.policia.gob.pe/id-now-services/api/v0/rest-hub/authentication/analizeTransaction";
            var enviar = new Request()
            {
                commonParamsRequest = new Commonparamsrequest
                {
                    channel = "WEB",
                    appId = "PNP WEB",
                    forwaredIp = "",
                    userAgent = ""
                },
                commonUserDataRequest = new Commonuserdatarequest
                {
                    userId = cip,
                    userType = "CP"
                }
                ,
                transactionType = "LOGIN",
                transactionName = "LOGIN KEYCLOAK",
                segment = null,
                deviceId = null,
                location = null,
                transactionHash = null,
                subTypeTransaction = "LOGIN KEYCLOAK PNP",
                additionalData = "",
                factsList = null,
                riskEngineData = null,
                transactionData = null,
                signData = null
            };
            var enviarJSON = System.Text.Json.JsonSerializer.Serialize(enviar);
            try
            {


                var enviarContent = new StringContent(enviarJSON.ToString(), Encoding.UTF8, "application/json");
                var responseHttp = httpClient.PostAsync(url, enviarContent).Result;


                if (responseHttp.IsSuccessStatusCode)
                {

                    responseString = await responseHttp.Content.ReadAsStringAsync();

                    data = JsonConvert.DeserializeObject(responseString);


                }
                else
                {
                    Console.WriteLine("Ocurrio un error ...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // return "data" + responseString;
            return data;



        }


        [WebMethod()]
        public bool ValidaTieneToken(string cip)
        {

            dynamic obj = AnalizaTransaccion(cip).GetAwaiter().GetResult();


            if (obj.code == 22)
            {

                return true;

            }

            return false;

        }

        private static async Task<bool> AutorizaTransaccionTokenAsync(string cip, string ChallengeData)

        {

            dynamic obj = AnalizaTransaccion(cip).GetAwaiter().GetResult();


            if (obj.code == 22)
            {


                string uuid = "";

                uuid = obj.uuid;

                string response = "";

                dynamic data = new ResponseAutorizador();


                var httpClient = new System.Net.Http.HttpClient();
                httpClient.DefaultRequestHeaders.Add("API-USER", "PNP");
                string url = "https://ticnowidnow2fa.policia.gob.pe/id-now-services/api/v0/rest-hub/authentication/autorizeTransaction";
                var enviar = new AuthorizadorRequest()
                {
                    commonParamsRequest = new Commonparamsrequest
                    {
                        channel = "WEB",
                        appId = "PNP WEB",
                        forwaredIp = "",
                        userAgent = ""
                    },
                    commonUserDataRequest = new Commonuserdatarequest
                    {
                        userId = cip,
                        userType = "CP"
                    },

                    uuidTransaction = uuid,
                    challengeData = ChallengeData,
                    authId = "02915beb-d017-4ec6-b1d3-b30ddee13f6f",
                    additionalAuthValues = null,
                    additionalChallengeData = null,
                    externalSessionId = null,
                    signData = null,
                    riskEngineData = null
                };

                var enviarJSON = System.Text.Json.JsonSerializer.Serialize(enviar);

                try
                {


                    var enviarContent = new StringContent(enviarJSON.ToString(), Encoding.UTF8, "application/json");
                    var responseHttp = httpClient.PostAsync(url, enviarContent).Result;


                    if (responseHttp.IsSuccessStatusCode)
                    {

                        response = await responseHttp.Content.ReadAsStringAsync();

                        data = JsonConvert.DeserializeObject(response);

                        //codigo = data.code;

                        if (data.code == "00") {

                            return true;

                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Ocurrio un error ...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return false;
            }

            return false;
        }

        [WebMethod()]
        public bool AutorizaToken(string cip, string token)
        {


            bool obj = AutorizaTransaccionTokenAsync(cip, token).GetAwaiter().GetResult();

            return obj;

        }



        [WebMethod()]
        public User RecuperaUsuarioVigencia(string v_cuenta_usuario, string password)
        {
          
            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));
            DirectoryEntry directoryEntry = new DirectoryEntry();
            directoryEntry.Path = path;
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;
            directoryEntry.Username = v_cuenta_usuario.ToString();
            directoryEntry.Password = password.ToString();



            string filter = Convert.ToString("sAMAccountName=") + v_cuenta_usuario;


            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, filter);
            directorySearcher.SearchScope = SearchScope.Subtree;

        
            User clsDatosUsuario = new User();

            try
            {
                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult == null)
                {
                    clsDatosUsuario.Usuario = string.Empty;
                    clsDatosUsuario.Password = string.Empty;
                    clsDatosUsuario.GradoNombreApellidos = string.Empty;
                    clsDatosUsuario.CarnetPNP = string.Empty;
                    clsDatosUsuario.Nombre = string.Empty;
                    clsDatosUsuario.Apellidos = string.Empty;
                    clsDatosUsuario.Email = string.Empty;
                    clsDatosUsuario.Estado = string.Empty;
                    clsDatosUsuario.Ruta = string.Empty;
                    clsDatosUsuario.pwdLastSet = string.Empty;                 
                    clsDatosUsuario.VigenciaPass = string.Empty;

                }
                else
                {
                    string Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();
                    Console.WriteLine("estado: " + Estado);

                    if (Estado == "514" | Estado == "66050")
                        Estado = "DESHABILITADO";
                    else
                        Estado = "HABILITADO";
                    clsDatosUsuario.Usuario = searchResult.Properties["sAMAccountName"][0].ToString();
                   // clsDatosUsuario.Password = password.ToString();
                    clsDatosUsuario.GradoNombreApellidos = searchResult.Properties["cn"][0].ToString();
                    clsDatosUsuario.CarnetPNP = searchResult.Properties["sAMAccountName"][0].ToString();
                    clsDatosUsuario.Nombre = searchResult.Properties["GivenName"][0].ToString();
                    clsDatosUsuario.Apellidos = searchResult.Properties["sn"][0].ToString();

                    //---- con el pwdLastSet ---
                

                    long value = (long)searchResult.Properties["pwdLastSet"][0];

                    DateTime pls = DateTime.FromFileTime(value);

                    TimeSpan dias = DateTime.Now.Subtract(pls);

                    string fecha = dias.Days.ToString();

                 


                    //--- string descripcion y agregando el procedure y guardandolo en vigencia

                    string Descripcion="DateExpire";

                    string cn = System.Configuration.ConfigurationManager.ConnectionStrings["WsdlPNP"].ConnectionString;
                    SqlConnection conn = null;

                    conn = new SqlConnection(cn);
                    SqlCommand cmd = new SqlCommand("vigenciapass", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    cmd.Parameters.Add("@Descripcion", System.Data.SqlDbType.VarChar).Value = Descripcion;


                    cmd.Parameters.Add("@Vigencia", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();


                    //--- convirtiendo a int
                   
                  int vigencia = Convert.ToInt32(cmd.Parameters["@Vigencia"].Value);
                

                    string mensaje;
                    //bool mensaje = false;
                                

                        //USUARIO
                        if (Convert.ToInt32(fecha) > Convert.ToInt32(vigencia))
                        {

                        mensaje = "Clave caducada";
                        clsDatosUsuario.VigenciaPass = mensaje;
                        //return mensaje;

                        }
                        else
                        {

                        mensaje = "Clave vigente";
                        clsDatosUsuario.VigenciaPass = mensaje;
                       // return mensaje = true;
                           // conn.Close();
                        }

                    clsDatosUsuario.Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();
                    if (searchResult.Properties["mail"].Count == 0)
                        clsDatosUsuario.Email = string.Empty;
                    else
                        clsDatosUsuario.Email = searchResult.Properties["mail"][0].ToString();
                    clsDatosUsuario.Estado = Estado.ToString();
                    clsDatosUsuario.Ruta = searchResult.Path.ToString();

                }
            }

            catch (Exception ex)
            {
               
                throw new Exception("ERROR METODO RecuperaUsuarioExisteAD: " + ex.Message);
           

            }

            //return false;
            return clsDatosUsuario;
        
    }


        [WebMethod()]
        public User RecuperaUsuarioExisteAD(string l_cuenta_usuario, string password)
        {
            
            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));
            DirectoryEntry directoryEntry = new DirectoryEntry();
            directoryEntry.Path = path;
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;
            directoryEntry.Username = l_cuenta_usuario.ToString();
            directoryEntry.Password = password.ToString();
            string filter = Convert.ToString("sAMAccountName=") + l_cuenta_usuario;
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, filter);
            directorySearcher.SearchScope = SearchScope.Subtree;
            User clsDatosUsuario = new User();


            try
            {
                SearchResult searchResult = directorySearcher.FindOne();
                if (searchResult == null)
                {
                    clsDatosUsuario.Usuario = string.Empty;
                    clsDatosUsuario.Password = string.Empty;
                    clsDatosUsuario.GradoNombreApellidos = string.Empty;
                    clsDatosUsuario.CarnetPNP = string.Empty;
                    clsDatosUsuario.Nombre = string.Empty;
                    clsDatosUsuario.Apellidos = string.Empty;
                    clsDatosUsuario.Email = string.Empty;
                    clsDatosUsuario.Estado = string.Empty;
                    clsDatosUsuario.Ruta = string.Empty;

                }
                else
                {

                    string Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();
                    Console.WriteLine("estado: " + Estado);

                    if (Estado == "514" | Estado == "66050")
                        Estado = "DESHABILITADO";
                    else
                        Estado = "HABILITADO";
                    clsDatosUsuario.Usuario = searchResult.Properties["sAMAccountName"][0].ToString();
                    clsDatosUsuario.Password = password.ToString();
                    clsDatosUsuario.GradoNombreApellidos = searchResult.Properties["cn"][0].ToString();
                    clsDatosUsuario.CarnetPNP = searchResult.Properties["sAMAccountName"][0].ToString();
                    clsDatosUsuario.Nombre = searchResult.Properties["GivenName"][0].ToString();
                    clsDatosUsuario.Apellidos = searchResult.Properties["sn"][0].ToString();


                    clsDatosUsuario.Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();
                    if (searchResult.Properties["mail"].Count == 0)
                        clsDatosUsuario.Email = string.Empty;
                    else
                        clsDatosUsuario.Email = searchResult.Properties["mail"][0].ToString();
                    clsDatosUsuario.Estado = Estado.ToString();
                    clsDatosUsuario.Ruta = searchResult.Path.ToString();

                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR METODO RecuperaUsuarioExisteAD: " + ex.Message);
              

            }
            return clsDatosUsuario;
        }
        [WebMethod()]
        public bool ExisteUsuarioLDAP(string l_cuenta_usuario, string passworde, int idsist)
        {

            bool valUnidad = Exist(idsist);

            if (!valUnidad)
            {
                Auditoria(l_cuenta_usuario, idsist, false, "El identificador del sistema no existe");
                return false;
            }

            string mesaje;
            string estad;
            bool result = false;

            //string path = "LDAP://" + ConfigurationManager.AppSettings["ldap.host"] + "/DC=intranetpnp,DC=pnp";              


            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));
            DirectoryEntry directoryEntry = new DirectoryEntry();
            directoryEntry.Path = path;
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;


            // LLEVARLO AL WEB CONFIG

            directoryEntry.Username = ConfigurationManager.AppSettings["ldap.username"];
            directoryEntry.Password = ConfigurationManager.AppSettings["ldap.password"];



            string filter = Convert.ToString("sAMAccountName=") + l_cuenta_usuario;
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, filter);
            directorySearcher.SearchScope = SearchScope.Subtree;

            SearchResult searchResult = directorySearcher.FindOne();

            bool efectivo = false;

            efectivo = ValidaUsuario(l_cuenta_usuario, passworde);

            try
            {

                if (searchResult == null)
                {

                    result = false;
                    mesaje = " Usuario no registrado:";

                }
                else
                {
                    estad = searchResult.Properties["Useraccountcontrol"][0].ToString();


                    if (estad == "514" | estad == "66050")
                    {

                        mesaje = "DESHABILITADO";


                    }
                    else

                    if (efectivo == false)
                    {

                        result = false;
                        mesaje = "Error de autenticacion usuario o password incorrecto";

                    }


                    else
                    {
                        result = true;
                        mesaje = "Usuario con contrasena correcta";
                    }


                }


            }

            catch (Exception ex)
            {

                throw new Exception("Error de autenticacion con el usuario o contraseña:" + ex.Message);

            }

            Auditoria(l_cuenta_usuario, idsist, result, mesaje);

            return result;
        }

        private bool Exist(int idsist)
        {

            bool exist = false;

            string cn = System.Configuration.ConfigurationManager.ConnectionStrings["WsdlPNP"].ConnectionString;
            SqlConnection conn = null;

            conn = new SqlConnection(cn);


            SqlCommand cmd = new SqlCommand("consultasist", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();

            cmd.Parameters.Add("@Idsist", System.Data.SqlDbType.VarChar).Value = @idsist;



            SqlDataReader reader = cmd.ExecuteReader();

            // POR ESTA OTRA FORMA TMB TRABAJA:  //conn.Open();
            //string sql = "select * from UserSist WHERE idsist = @idsist";
            //SqlCommand cmd = new SqlCommand(sql, conn);
            //cmd.Parameters.AddWithValue("@idsist", idsist);
            //SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                exist = true;
                conn.Close();
            }

            else
            {
                conn.Close();
                return exist;

            }


            return exist;
        }

        [WebMethod()]
        public bool UsuarioHabilitadoLDAP(string v_cuenta_usuario)
        {

            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));

            bool flag = false;

            using (DirectoryEntry directoryEntry = new DirectoryEntry(path, "netadmin", "netsql0.", AuthenticationTypes.Secure))
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = (Convert.ToString("(|(samAccountName=") + v_cuenta_usuario) + "))";
                    directorySearcher.PropertiesToLoad.Add("samAccountName");
                    directorySearcher.PropertiesToLoad.Add("userAccountControl");

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())

                    {
                        foreach (SearchResult searchResult in searchResultCollection)
                        {
                            int num = System.Convert.ToInt32(searchResult.Properties["Useraccountcontrol"][0]);
                            System.Convert.ToString(searchResult.Properties["samAccountName"][0]);
                            bool flag2 = (num & 2) > 0;
                            flag = flag2;
                        }
                    }
                }
            }
            return !flag;
        }
        [WebMethod()]
        public User RecuperaDatosUsuarioExisteLDAP(string v_cuenta_usuario)
        {

           
            string path = Convert.ToString(Convert.ToString("LDAP://172.31.1.7/DC=intranetpnp,DC=pnp"));

            DirectoryEntry directoryEntry = new DirectoryEntry();
            directoryEntry.Path = path;
            directoryEntry.AuthenticationType = AuthenticationTypes.Secure;


            directoryEntry.Username = ConfigurationManager.AppSettings["ldap.username"];
            directoryEntry.Password = ConfigurationManager.AppSettings["ldap.password"];


            string filter = Convert.ToString("sAMAccountName=") + v_cuenta_usuario;
            DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry, filter);
            directorySearcher.SearchScope = SearchScope.Subtree;
            User clsDatosUsuario = new User();


            try
            {
                SearchResult searchResult = directorySearcher.FindOne();
                if (searchResult == null)
                {
                    clsDatosUsuario.Usuario = string.Empty;
                    clsDatosUsuario.Password = string.Empty;
                    clsDatosUsuario.GradoNombreApellidos = string.Empty;
                    clsDatosUsuario.CarnetPNP = string.Empty;
                    clsDatosUsuario.Nombre = string.Empty;
                    clsDatosUsuario.Apellidos = string.Empty;
                    clsDatosUsuario.Email = string.Empty;
                    clsDatosUsuario.Estado = string.Empty;
                    clsDatosUsuario.Ruta = string.Empty;
                   
                }
                else
                {
                    
                    string Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();
                    if (Estado == "514" | Estado == "66050")
                        Estado = "DESHABILITADO";
                    else
                        Estado = "HABILITADO";
                    clsDatosUsuario.Usuario = searchResult.Properties["sAMAccountName"][0].ToString();
                    clsDatosUsuario.Password = string.Empty;
                    clsDatosUsuario.GradoNombreApellidos = searchResult.Properties["cn"][0].ToString();
                    clsDatosUsuario.CarnetPNP = searchResult.Properties["sAMAccountName"][0].ToString();
                    clsDatosUsuario.Nombre = searchResult.Properties["GivenName"][0].ToString();
                    clsDatosUsuario.Apellidos = searchResult.Properties["sn"][0].ToString();

                    clsDatosUsuario.Estado = searchResult.Properties["Useraccountcontrol"][0].ToString();

                    if (searchResult.Properties["mail"].Count == 0)
                        clsDatosUsuario.Email = string.Empty;
                    else
                        clsDatosUsuario.Email = searchResult.Properties["mail"][0].ToString();
                    clsDatosUsuario.Estado = Estado.ToString();
                    clsDatosUsuario.Ruta = searchResult.Path.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR METODO RecuperaDatosUsuarioExisteLDAP: " + ex.Message);
            }
            return clsDatosUsuario;
        }


        private void Auditoria(string l_cuenta_usuario, int idsist, bool result, string mesaje)

        {


            string cn = System.Configuration.ConfigurationManager.ConnectionStrings["WsdlPNP"].ConnectionString;
            SqlConnection conn = null;

            conn = new SqlConnection(cn);
            SqlCommand cmd = new SqlCommand("consultauser", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            conn.Open();
            cmd.Parameters.Add("@IdSis", System.Data.SqlDbType.VarChar).Value = idsist;
            cmd.Parameters.Add("@CipUser", System.Data.SqlDbType.VarChar).Value = l_cuenta_usuario;
            cmd.Parameters.Add("@Result", System.Data.SqlDbType.VarChar).Value = result;
            cmd.Parameters.Add("@Mesaje", System.Data.SqlDbType.VarChar).Value = mesaje;

            SqlDataReader reader = cmd.ExecuteReader();

            result = false;
            conn.Close();


        }
    } 
}
    




    
    
 

     
