using S50MVVM.Utilities;
using System.Threading;
using System.Windows.Input;
using S50MVVM.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using System.Net.Http;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace S50MVVM.ViewModel
{
    class SettingVM : ViewModelBase
    {
        private const string ClientId = "xxxxxx";
        private const string ClientSecret = "xxxxxx";
        private const string RedirectUri = "https://example.com/callback";

        private string _accessToken;
        private List<WaveOutEvent> _waveOutEvents; // Lista para almacenar las instancias de WaveOutEvent
        private bool _isPlaying;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Cancions> _canciones;
        public ObservableCollection<Cancions> Canciones
        {
            get { return _canciones; }
            set
            {
                _canciones = value;
                OnPropertyChanged(nameof(Canciones));
            }
        }

        public string NombrePlaylist { get; set; }

        private Cancions _selectedSong;
        public Cancions SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSong)));
            }
        }

        private float _volume = 0.5f; // Valor de volumen inicial
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                    UpdateVolume(); // Método para actualizar el volumen de la reproducción de audio
                }
            }
        }

        public ICommand ListBoxDoubleClickCommand { get; private set; }

        public SettingVM()
        {
            ListBoxDoubleClickCommand = new RelayCommand(ExecuteListBoxDoubleClickCommand);
            Canciones = new ObservableCollection<Cancions>();
            CargarCancionesDesdeBaseDeDatos();
            _waveOutEvents = new List<WaveOutEvent>(); // Inicializa la lista de WaveOutEvent
        }

        // Función que se ejecuta cuando se hace doble clic en un elemento de la lista
        private void ExecuteListBoxDoubleClickCommand(object parameter)
        {
            var selectedSong = parameter as Cancions;
            if (selectedSong != null)
            {
                string songName = selectedSong.Titulo;
                MessageBox.Show(songName);
                string artistName = selectedSong.Artista;

                IniciarReproduccion(songName, artistName);
            }
            else
            {
                MessageBox.Show("Si us plau, escull una cançó.", "Advertència", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }


        // Método para actualizar el volumen de la reproducción de audio
        private void UpdateVolume()
        {
            foreach (var waveOut in _waveOutEvents)
            {
                if (waveOut != null)
                {
                    waveOut.Volume = Volume; // Aplica el volumen al objeto WaveOutEvent
                }
            }
        }

        // Método para iniciar la reproducción de una canción
        private async Task IniciarReproduccion(string songName, string artistName)
        {
            MessageBox.Show("Se ha entrado a la funcion de reproducir preview");

            if (SelectedSong == null)
            {
                MessageBox.Show("Selecciona una canción.");
                return;
            }

            string accessToken = _accessToken;

            string query = $"{songName} artist:{artistName}";
            string encodedQuery = Uri.EscapeDataString(query);
            string searchUrl = $"https://api.spotify.com/v1/search?q={encodedQuery}&type=track";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(searchUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var tracks = Newtonsoft.Json.Linq.JObject.Parse(responseContent)["tracks"]["items"];

                        var firstTrack = tracks.FirstOrDefault();
                        if (firstTrack != null)
                        {
                            var trackUri = firstTrack["preview_url"].ToString();

                            if (!string.IsNullOrEmpty(trackUri))
                            {
                                using (var waveOut = new WaveOutEvent())
                                {
                                    _waveOutEvents.Add(waveOut); // Agrega la instancia de WaveOutEvent a la lista
                                    using (var webClient = new HttpClient())
                                    {
                                        var audioBytes = await webClient.GetByteArrayAsync(trackUri);
                                        using (var memoryStream = new MemoryStream(audioBytes))
                                        {
                                            using (var mp3Reader = new Mp3FileReader(memoryStream))
                                            {
                                                waveOut.Init(mp3Reader);
                                                waveOut.Volume = Volume; // Aplica el volumen al objeto WaveOutEvent
                                                waveOut.Play();

                                                // Actualiza la barra de progreso mientras se reproduce el audio
                                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                                {
                                                    await Task.Delay(500);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("La canción no tiene una URL de vista previa disponible.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se encontraron canciones.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        // Método para cargar las canciones desde la base de datos
        private void CargarCancionesDesdeBaseDeDatos()
        {
            // Obtiene el nombre de la lista de reproducción del hilo principal
            string nombrePlaylist = ((CustomerProjectIdentity)Thread.CurrentPrincipal.Identity).Llista;

            // Llama al método correspondiente a la base de datos para cargar las canciones
            Canciones = BBDD.CargarCancionesDesdeBaseDeDatos(nombrePlaylist);
        }
    }
}
