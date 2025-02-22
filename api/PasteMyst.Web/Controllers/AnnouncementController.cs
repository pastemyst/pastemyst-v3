using Microsoft.AspNetCore.Mvc;
using PasteMyst.Web.Models;
using PasteMyst.Web.Services;

namespace PasteMyst.Web.Controllers;

[ApiController]
[Route("/api/v3/announcements")]
public sealed class AnnouncementController(AnnouncementService announcementService) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<Announcement> GetLatestAnnouncement(CancellationToken cancellationToken)
    {
        return await announcementService.GetLatestAnnouncement(cancellationToken);
    }

    [HttpGet()]
    public async Task<List<Announcement>> GetAllAnnouncements(CancellationToken cancellationToken)
    {
        return await announcementService.GetAllAnnouncements(cancellationToken);
    }

    [HttpPost]
    public async Task CreateAnnouncement(AnnouncementCreateInfo createInfo, CancellationToken cancellationToken)
    {
        await announcementService.CreateAnnouncement(createInfo, cancellationToken);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAnnouncement(string id, CancellationToken cancellationToken)
    {
        await announcementService.DeleteAnnouncement(id, cancellationToken);
    }

    [HttpPatch("{id}")]
    public async Task EditAnnouncement(string id, AnnouncementCreateInfo editInfo, CancellationToken cancellationToken)
    {
        await announcementService.EditAnnouncement(id, editInfo, cancellationToken);
    }
}