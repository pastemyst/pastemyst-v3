using System.Net;
using MongoDB.Driver;
using PasteMyst.Web.Exceptions;
using PasteMyst.Web.Models;

namespace PasteMyst.Web.Services;

public class AnnouncementService(MongoService mongo, UserContext userContext)
{
    public async Task<Announcement> GetLatestAnnouncement(CancellationToken cancellationToken)
    {
        return (await mongo.Announcements.FindAsync(x => true, new FindOptions<Announcement>
        {
            Sort = Builders<Announcement>.Sort.Descending(x => x.CreatedAt)
        }, cancellationToken)).ToList(cancellationToken).FirstOrDefault();
    }

    public async Task<List<Announcement>> GetAllAnnouncements(CancellationToken cancellationToken)
    {
        return (await mongo.Announcements.FindAsync(x => true, new FindOptions<Announcement>
        {
            Sort = Builders<Announcement>.Sort.Descending(x => x.CreatedAt)
        }, cancellationToken)).ToList(cancellationToken);
    }

    public async Task CreateAnnouncement(AnnouncementCreateInfo createInfo, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn() || !userContext.Self.IsAdmin)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "Only admins can create new announcements.");
        }

        var announcement = new Announcement
        {
            Title = createInfo.Title,
            Content = createInfo.Content,
            CreatedAt = DateTime.UtcNow
        };

        await mongo.Announcements.InsertOneAsync(announcement, cancellationToken: cancellationToken);
    }

    public async Task DeleteAnnouncement(string id, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn() || !userContext.Self.IsAdmin)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "Only admins can delete announcements.");
        }

        await mongo.Announcements.DeleteOneAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public async Task EditAnnouncement(string id, AnnouncementCreateInfo editInfo, CancellationToken cancellationToken)
    {
        if (!userContext.IsLoggedIn() || !userContext.Self.IsAdmin)
        {
            throw new HttpException(HttpStatusCode.Unauthorized, "Only admins can edit announcements.");
        }

        var update = Builders<Announcement>.Update
            .Set(x => x.Title, editInfo.Title)
            .Set(x => x.Content, editInfo.Content);

        await mongo.Announcements.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
    }
}