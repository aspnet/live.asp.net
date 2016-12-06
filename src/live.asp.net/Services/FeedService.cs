using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace live.asp.net.Services
{
    public class FeedService
    {
        private readonly IShowsService _showsService;

        public FeedService(IShowsService showsService)
        {
            _showsService = showsService;
        }

        public async Task<string> GetFeed(string appRoot, ClaimsPrincipal User)
        {
            var feed = new AtomFeed
            {
                Id = appRoot,
                Icon = appRoot + "favicon.ico",
                Logo = appRoot + "images/asplogo-new.png",
                Links = new List<Link>(new[]
                {
                    new Link { Type = "text/html", Rel = "alternate", Href = appRoot },
                    new Link { Type = "application/atom+xml", Rel = "self", Href = appRoot + "feed.atom" }
                }),
                Title = "ASP.NET Community Standup Episodes",
                Author = new Person { Name = "ASP.NET Community Standup", Uri = appRoot },
                Entries = new List<Entry>()
            };

            // Start iterating over all recorded shows
            var showsResult = await _showsService.GetRecordedShowsAsync(User, false);
            var shows = showsResult.Shows;
            if (shows != null)
            {
                foreach (var show in shows)
                {
                    feed.Entries.Add(new Entry
                    {
                        Id = show.Url,
                        Updated = show.ShowDate.DateTime.ToString("s") + "Z",
                        Links = new List<Link>(new[]
                        {
                            new Link { Type = "text/html", Rel = "alternate", Href = show.Url },
                            new Link { Type = "video/mp4", Rel = "enclosure", Href = show.Url, Title = show.Title }
                        }),
                        Title = show.Title,
                        Content = new Content
                        {
                            Type = "html",
                            ContentText = string.Format("<a href={0}><img src=\"{1}\"/></a><p>{2}</p>", show.Url, show.ThumbnailUrl, show.Description)
                        }
                    });
                }
            }
            var serializer = new XmlSerializer(typeof(AtomFeed), "http://www.w3.org/2005/Atom");
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, feed, feed.Namespaces);

            return stringWriter.ToString();
        }
    }

    [XmlRoot("feed")]
    public class AtomFeed
    {
        public AtomFeed()
        {
            Namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName(string.Empty, "http://www.w3.org/2005/Atom") });
        }

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; set; }

        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("icon")]
        public string Icon { get; set; }

        [XmlElement("logo")]
        public string Logo { get; set; }

        [XmlElement("link")]
        public List<Link> Links { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("author")]
        public Person Author { get; set; }

        [XmlElement("updated")]
        public string Updated { get; set; } = DateTime.UtcNow.ToString("s") + "Z";

        [XmlElement("entry")]
        public List<Entry> Entries { get; set; }
    }

    [XmlRoot("entry")]
    public class Entry
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("updated")]
        public string Updated { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("link")]
        public List<Link> Links { get; set; }

        [XmlElement("content")]
        public Content Content { get; set; }
    }

    [XmlRoot("content")]
    public class Content
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string ContentText { get; set; }
    }

    [XmlRoot("author")]
    public class Person
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("email")]
        public string Email { get; set; }

        [XmlElement("uri")]
        public string Uri { get; set; }
    }

    [XmlRoot("link")]
    public class Link
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("rel")]
        public string Rel { get; set; }

        [XmlAttribute("href")]
        public string Href { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }
    }
}
