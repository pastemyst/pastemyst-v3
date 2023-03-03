using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pastemyst.DbContexts;
using pastemyst.Models;

namespace pastemyst.Controllers;

[ApiController]
[Route("/api/v3/users")]
public class UserController : ControllerBase
{
    private readonly DataContext _dbContext;

    public UserController(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("{username}")]
    [Route("")]
    public async Task<IActionResult> GetUser(string username, [FromQuery] string id)
    {
        User user = null;

        if (username is not null)
        {
            user = await _dbContext.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
        }
        else if (id is not null)
        {
            user = await _dbContext.Users.FindAsync(id);
        }

        return user is not null ? Ok(user) : NotFound();
    }
}