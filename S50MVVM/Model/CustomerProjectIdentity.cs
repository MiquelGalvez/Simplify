using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace S50MVVM.Model
{
    public class CustomerProjectIdentity : IIdentity
    {
        public string Name { get; }

        public string Llista { get; }

        public CustomerProjectIdentity(string usuari, string llista)
        {
            Name = usuari;
            Llista = llista;
        }

        public string AuthenticationType => "CustomAuthentication";
        public bool IsAuthenticated => !string.IsNullOrEmpty(Name);
    }
}
