using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using S50MVVM.Utilities;
using SpotifyWPFSearch.Helpers;
using SpotifyWPFSearch.Models;

namespace SpotifyWPFSearch.ViewModels
{
    public class CustomerVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<SpotifyArtist> _artists;
        public ObservableCollection<SpotifyArtist> Artists
        {
            get { return _artists; }
            set
            {
                _artists = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }

        public CustomerVM()
        {
            Artists = new ObservableCollection<SpotifyArtist>();
            SearchCommand = new RelayCommand(async () => await SearchAsync());
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Artists.Clear();
                return;
            }

            var result = await SearchHelper.SearchArtistOrSong(SearchText);

            if (result == null)
                return;

            Artists.Clear();
            foreach (var item in result.artists.items)
            {
                Artists.Add(new SpotifyArtist
                {
                    ID = item.id,
                    Image = item.images.Any() ? item.images[0].url : "https://user-images.githubusercontent.com/24848110/33519396-7e56363c-d79d-11e7-969b-09782f5ccbab.png",
                    Name = item.name,
                    Popularity = $"{item.popularity}% popularidad",
                    Followers = $"{item.followers.total.ToString("N")} seguidores"
                });
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
