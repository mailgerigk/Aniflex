using Aniflex.MVVM.Models;
using Aniflex.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aniflex.Provider;
public sealed class Subsplease : IProvider
{
    private const string UriBase = "https://subsplease.org";
    private const string ShowsUri = $"{UriBase}/shows/";
    private const string SeasonUri = $"{UriBase}/api/?f=schedule&tz=Europe/Berlin";
    private const string ApiUri = $"{UriBase}/api/?f=show&tz=Europe/Berlin&sid=";

    public string Name => "subsplease.org";

    private static readonly Regex showsFirst = new Regex("<div class=\"all-shows-link\">(.*?)</div>", RegexOptions.Singleline);
    private static readonly Regex showsSecond = new Regex("<a href=\"(.*?)\" title=\"(.*?)\">", RegexOptions.Singleline);
    private static readonly Regex seasonUrisRegex = new Regex("\"page\":\"(.*?)\"");
    private static readonly Regex sidRegex = new Regex("sid=\"(.*?)\"");
    private static readonly Regex thumbnailRegex = new Regex("<img class=\"img-responsive img-center\" src=\"(.*?)\"");
    private static readonly Regex descriptionRegex = new Regex("<div class=\"series-syn\">.*?<p>(.*?)</p>", RegexOptions.Singleline);

    private struct ApiEpisode
    {
        [JsonPropertyName("time")]
        public string Time { get; set; }
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }
        [JsonPropertyName("show")]
        public string Show { get; set; }
        [JsonPropertyName("episode")]
        public string Episode { get; set; }
        [JsonPropertyName("downloads")]
        public ApiDownload[] Downloads { get; set; }
    }

    private struct ApiDownload
    {
        [JsonPropertyName("res")]
        public string Resolution { get; set; }
        [JsonPropertyName("torrent")]
        public string Torrent { get; set; }
        [JsonPropertyName("magnet")]
        public string Magnet { get; set; }
        [JsonPropertyName("xdcc")]
        public string XDCC { get; set; }

        public VideoQuality VideoQuality => Resolution switch
        {
            "360" => VideoQuality.p360,
            "480" => VideoQuality.p480,
            "540" => VideoQuality.p540,
            "720" => VideoQuality.p720,
            "1080" => VideoQuality.p1080,
            _ => throw new NotImplementedException(),
        };
    }

    public async Task GetShows(WebContext context, ProviderProgress progress)
    {
        string showsSource = await context.Get(ShowsUri).ConfigureAwait(false);
        List<string> seasonShows = await GetSeasonAnimes(context).ConfigureAwait(false);
        var animes = new List<Anime>();
        foreach (Match match in showsFirst.Matches(showsSource))
        {
            string hrefCollection = match.Captures.First().Value;
            foreach (Match showMatch in showsSecond.Matches(hrefCollection))
            {
                string showUri = showMatch.Groups[1].Value;
                string showTitle = showMatch.Groups[2].Value;
                var anime = new Anime
                {
                    Name = showTitle,
                    Uri = $"{UriBase}{showUri}",
                };
                anime.IsInSeason = seasonShows.Any(uri => uri == anime.Uri);
                animes.Add(anime);
            }
        }

        progress.Total = animes.Count;


        await Task.WhenAll(animes.Select(anime =>
        {
            return Task.Run(async () =>
            {
                try
                {
                    await GetAnimeInformation(context, anime);
                    progress.Report(anime);
                }
                catch (Exception)
                {

                }
            });
        }));
    }

    private async Task<List<string>> GetSeasonAnimes(WebContext context)
    {
        string seasonSource = await context.Get(SeasonUri).ConfigureAwait(false);
        var shows = new List<string>();
        foreach (Match match in seasonUrisRegex.Matches(seasonSource))
        {
            string page = match.Groups[1].Value;
            shows.Add($"{ShowsUri}{page}");
        }
        return shows;
    }

    private async Task GetAnimeInformation(WebContext context, Anime anime)
    {
        Debug.Assert(anime.Uri is not null);

        string showSource = await context.Get(anime.Uri).ConfigureAwait(false);
        string sid = sidRegex.Match(showSource).Groups[1].Value;

        if (anime.Thumbnail is null)
        {
            string uriQuery = thumbnailRegex.Match(showSource).Groups[1].Value;
            string uri = $"{UriBase}{uriQuery}";
            anime.Thumbnail = new Thumbnail
            {
                Uri = uri,
            };
        }
        if (anime.Description is null)
        {
            anime.Description = descriptionRegex.Match(showSource).Groups[1].Value;
        }

        string showApiResponse = await context.Get($"{ApiUri}{sid}");
        JsonObject json = JsonSerializer.Deserialize<JsonObject>(showApiResponse)!;
        if (json.ContainsKey("batch"))
        {
            try
            {
                JsonObject batches = json["batch"]!.AsObject();
                foreach ((string _, JsonNode batchObject) in batches)
                {
                    ApiEpisode batch = JsonSerializer.Deserialize<ApiEpisode>(batchObject!.ToJsonString());
                    // for now do nothing with batches
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
        if (json.ContainsKey("episode"))
        {
            try
            {
                JsonObject episodes = json["episode"]!.AsObject();
                foreach ((string episodeName, JsonNode episodeObject) in episodes)
                {
                    ApiEpisode episode = JsonSerializer.Deserialize<ApiEpisode>(episodeObject!.ToJsonString());
                    anime.Episodes.Add(
                        new Episode()
                        {
                            ReleaseTime = DateTime.Now, // episode.ReleaseDate,
                            Magnets = new(episode.Downloads.Select(d =>
                            {
                                return new Magnet()
                                {
                                    Kind = MagnetKind.Episode,
                                    Quality = d.VideoQuality,
                                    Uri = d.Magnet,
                                };
                            })),
                            Id = episode.Episode,
                        }
                    );
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
