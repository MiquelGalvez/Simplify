using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S50MVVM.Model
{
    public class SearchResponse
    {
        public Tracks Tracks { get; set; }
    }

    public class Tracks
    {
        public List<Song> Items { get; set; }
    }

    public class Song
    {
        public string Name { get; set; } // Prioritat del nom de la canço que es busca
        public string PreviewURL { get; set; } // Prioritat de la URL del Preview per poder reproduir el so de la canço que es busca
        public List<Artist> Artists { get; set; } // Llistat d'atrtistes autors de la canço, al mostrarlos nomes escollirem el primer de la llista 
    }

    public class Artist
    {
        public string Name { get; set; }
    }
}
