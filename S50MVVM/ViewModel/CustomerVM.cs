using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using Newtonsoft.Json;
using S50MVVM.Model;
using S50MVVM.Utilities;

namespace S50MVVM.ViewModel
{
    class CustomerVM : Utilities.ViewModelBase
    {
        private const string ClientId = "xxxx";
        private const string ClientSecret = "xxxx";
        private const string RedirectUri = "https://example.com/callback";

        private string _accessToken;
        private List<WaveOutEvent> _waveOutEvents; // Llista per emmagatzemar les instàncies de WaveOutEvent
        private bool _isPlaying;

        private TimeSpan _currentTime;
        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }

        private TimeSpan _totalDuration;
        public TimeSpan TotalDuration
        {
            get { return _totalDuration; }
            set
            {
                _totalDuration = value;
                OnPropertyChanged(nameof(TotalDuration));
            }
        }

        public double TotalDurationSeconds => TotalDuration.TotalSeconds;

        public ICommand ListBoxDoubleClickCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand StopCommand { get; set; }

        public string SearchQuery { get; set; }
        public ObservableCollection<Song> SearchResults { get; set; }
        public Song SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                OnPropertyChanged(nameof(SelectedSong));
                if (_selectedSong != null)
                {
                    StopCurrentSong(); // Atura la reproducció de la cançó actual abans de reproduir la nova
                    Task.Run(() => BuscarIReproduirMusica());
                }
            }
        }
        private Song _selectedSong;

        public ICommand SearchCommand { get; private set; }

        private float _volume = 0.5f; // Valor de volum inicial
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                    UpdateVolume(); // Mètode per actualitzar el volum de la reproducció d'àudio
                }
            }
        }

        public CustomerVM()
        {
            ListBoxDoubleClickCommand = new RelayCommand(_ => StopCurrentSong());
            PlayPauseCommand = new RelayCommand(_ => PlayPause());
            StopCommand = new RelayCommand(_ => Stop());
            SearchResults = new ObservableCollection<Song>();
            SelectedSong = null;
            SearchCommand = new RelayCommand(async (_) => await Search(null));
            Task.Run(() => ObtenirAccessToken());
            _waveOutEvents = new List<WaveOutEvent>(); // Inicialitza la llista de WaveOutEvent
        }

        // Obtenir el token d'accés per a l'autenticació amb Spotify
        private async Task ObtenirAccessToken()
        {
            using (var client = new HttpClient())
            {
                var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64Auth}");

                var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(result);
                    _accessToken = json.access_token;
                }
                else
                {
                    MessageBox.Show("S'ha produït un error en obtenir el token d'accés.");
                }
            }
        }

        // Cerca cançons a Spotify
        private async Task Search(object parameter)
        {
            if (string.IsNullOrEmpty(SearchQuery))
                return;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await client.GetAsync($"https://api.spotify.com/v1/search?type=track&q={SearchQuery}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(result);
                    SearchResults.Clear();
                    foreach (var track in searchResponse.Tracks.Items)
                    {
                        SearchResults.Add(track);
                    }
                }
                else
                {
                    MessageBox.Show("S'ha produït un error en realitzar la cerca.");
                }
            }
        }

        // Atura la reproducció de la cançó actual
        private void StopCurrentSong()
        {
            // Atura la reproducció de totes les cançons a la llista
            foreach (var waveOut in _waveOutEvents)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }
            _waveOutEvents.Clear();
            _isPlaying = false;
        }

        // Reprodueix o pausa la cançó actual
        private void PlayPause()
        {
            foreach (var waveOut in _waveOutEvents)
            {
                if (waveOut != null)
                {
                    if (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        waveOut.Pause();
                        _isPlaying = false;
                    }
                    else if (waveOut.PlaybackState == PlaybackState.Paused)
                    {
                        waveOut.Play();
                        _isPlaying = true;
                    }
                }
            }
        }

        // Atura la reproducció de la cançó actual
        private void Stop()
        {
            StopCurrentSong();
        }

        // Cerca la vista prèvia de la cançó seleccionada i la reprodueix
        private async Task BuscarIReproduirMusica()
        {
            if (SelectedSong == null)
            {
                MessageBox.Show("Selecciona una cançó.");
                return;
            }

            string accessToken = _accessToken;
            string songName = SelectedSong.Name;
            string artistName = SelectedSong.Artists[0].Name;

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
                                    _waveOutEvents.Add(waveOut); // Afegeix la instància de WaveOutEvent a la llista
                                    using (var webClient = new HttpClient())
                                    {
                                        var audioBytes = await webClient.GetByteArrayAsync(trackUri);
                                        using (var memoryStream = new MemoryStream(audioBytes))
                                        {
                                            using (var mp3Reader = new Mp3FileReader(memoryStream))
                                            {
                                                TotalDuration = mp3Reader.TotalTime;
                                                waveOut.Init(mp3Reader);
                                                waveOut.Volume = Volume; // Aplica el volum a l'objecte WaveOutEvent
                                                waveOut.Play();

                                                // Actualitza la barra de progrés mentre es reprodueix l'àudio
                                                while (waveOut.PlaybackState == PlaybackState.Playing)
                                                {
                                                    Application.Current.Dispatcher.Invoke(() =>
                                                    {
                                                        CurrentTime = mp3Reader.CurrentTime;
                                                    });
                                                    await Task.Delay(500);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("La cançó no té una URL de vista prèvia disponible.", "Advertència", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No s'han trobat cançons.");
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

        // Mètode per actualitzar el volum de la reproducció d'àudio
        private void UpdateVolume()
        {
            foreach (var waveOut in _waveOutEvents)
            {
                if (waveOut != null)
                {
                    waveOut.Volume = Volume; // Aplica el volum a l'objecte WaveOutEvent
                }
            }
        }
    }
}
