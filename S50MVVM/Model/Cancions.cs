using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S50MVVM.Model
{
    internal class Cancions
    {
            public int Id { get; set; }
            public string Titulo { get; set; }
            public string Artista { get; set; }
            public string Album { get; set; }
            public TimeSpan Duracio { get; set; }
            public string Genere { get; set; }
            public string AnyLlancement { get; set; }
            public string URLImagenAlbum { get; set; }
    }
}
