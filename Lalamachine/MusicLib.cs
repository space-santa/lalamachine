using Qml.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LibLala;
using LalaDb.Data;

namespace Lalamachine
{
    [Signal("scanDone")]
    public class MusicLib
    {
        private Model.MusicLibModel _model;

        public MusicLib()
        {
            _model = new Model.MusicLibModel();
            searchString = "";
            genreFilter = "";
            artistFilter = "";
            albumFilter = "";
        }

        #region properties

        public string searchString { get; set; }

        private string _genreFilter;
        private string _artistFilter;
        private string _albumFilter;

        public string genreFilter
        {
            get => _genreFilter;
            set
            {
                _genreFilter = emptyWhenNullOrAll(value);
            }
        }

        public string artistFilter
        {
            get => _artistFilter;
            set
            {
                _artistFilter = emptyWhenNullOrAll(value);
            }
        }

        public string albumFilter
        {
            get => _albumFilter;
            set
            {
                _albumFilter = emptyWhenNullOrAll(value);
            }
        }

        private string emptyWhenNullOrAll(string value)
        {
            if (value == null || value == Constants.ALL)
            {
                return "";
            }

            return value;
        }

        public bool scanning { get => _model.Scanning; }

        public string genreList
        {
            get
            {
                var list = _model.genreList(searchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }

        public string artistList
        {
            get
            {
                string[] list;
                list = _model.artistList(genreFilter, searchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
        }

        public string albumList
        {
            get
            {
                string[] list;
                list = _model.albumList(artistFilter, genreFilter, searchString);
                list = list.Prepend(Constants.ALL).ToArray();
                return Newtonsoft.Json.JsonConvert.SerializeObject(list.ToArray());
            }
        }

        public string displayLib
        {
            get
            {
                Track[] list;
                list = _model.displayLib(albumFilter, artistFilter, genreFilter, searchString);
                return TracksToTagListString(list);
            }
        }

        private string TracksToTagListString(Track[] tracks)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var tagList = new List<LalaTags>();

            foreach (var track in tracks)
            {
                LalaTags lalaTags = new LalaTags(track);
                tagList.Add(lalaTags);
            }

            sw.Stop();
            Console.WriteLine($"Creating tags took {sw.Elapsed}");

            return Newtonsoft.Json.JsonConvert.SerializeObject(tagList);
        }

        #endregion properties

        public async void scanAsync(string path)
        {
            if (scanning) { return; }
            await _model.scanAsync(path);
            this.ActivateSignal("scanDone");
        }

        public string getMetadataForMrl(string path)
        {
            try
            {
                LalaTags tags = _model.getMetadataForMrl(path);
                return tags.ToJson();
            }
            catch (System.InvalidOperationException)
            {
                return "{}";
            }
        }

        public string getAlbumTracks(string name)
        {
            var tagList = _model.getAlbumTracks(name);
            return Newtonsoft.Json.JsonConvert.SerializeObject(tagList);
        }
    }
}
