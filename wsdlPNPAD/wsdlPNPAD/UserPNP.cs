using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsdlPNPAD
{
    public class User
    {
        private string x_usuario;
        private string x_password;
        private string x_gradonombre;
        private string x_carnetpnp;
        private string x_nombre;
        private string x_email;
        private string x_apellidos;
        private string x_estado;
        private string x_ruta;
        private string x_pwdLastSet;
        
        private string x_VigenciaPass;
        public string Usuario
        {
            get
            {
                return x_usuario;
            }
            set
            {
                x_usuario = value;
            }
        }
        public string Password
        {
            get
            {
                return x_password;
            }
            set
            {
                x_password = value;
            }
        }
        public string GradoNombreApellidos
        {
            get
            {
                return x_gradonombre;
            }
            set
            {
                x_gradonombre = value;
            }
        }
        public string CarnetPNP
        {
            get
            {
                return x_carnetpnp;
            }
            set
            {
                x_carnetpnp = value;
            }
        }
        public string Nombre
        {
            get
            {
                return x_nombre;
            }
            set
            {
                x_nombre = value;
            }
        }
        public string Email
        {
            get
            {
                return x_email;
            }
            set
            {
                x_email = value;
            }
        }
        public string Apellidos
        {
            get
            {
                return x_apellidos;
            }
            set
            {
                x_apellidos = value;
            }
        }
        public string Estado
        {
            get
            {
                return x_estado;
            }
            set
            {
                x_estado = value;
            }
        }
        public string Ruta
        {
            get
            {
                return x_ruta;
            }
            set
            {
                x_ruta = value;
            }
        }

        public string pwdLastSet
        {
            get
            {
                return x_pwdLastSet;
            }
            set
            {
                x_pwdLastSet = value;
            }
        }

       

        public string VigenciaPass
        {
            get
            {
                return x_VigenciaPass;
            }
            set
            {
                x_VigenciaPass = value;
            }


        }
    }
}



