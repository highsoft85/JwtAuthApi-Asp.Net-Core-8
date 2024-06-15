using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Asp.Versioning;
using JwtAuthApi.Data;
using JwtAuthApi.Models;

namespace JwtAuthApi.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("api/[controller]")]
public class PagesController(ILogger<PagesController> logger, ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<PagesController> _logger = logger;

    [Authorize(Roles = "Admin")]
    [HttpPost("new")]
    public async Task<ActionResult<Page>> CreatePage(PageDto pageDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var page = new Page
        {
            Title = pageDto.Title,
            Author = pageDto.Author,
            Body = pageDto.Body,
        };

        _dbContext.Pages.Add(page);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
    }


    [HttpGet("{id:int}"), Authorize]
    public async Task<ActionResult<PageDto>> GetPage(int id)
    {
        var page = await _dbContext.Pages.FindAsync(id);

        if (page is null)
        {
            return NotFound();
        }

        var pageDto = new PageDto
        {
            Id = page.Id,
            Author = page.Author,
            Body = page.Body,
            Title = page.Title
        };

        return pageDto;
    }


    [HttpGet, Authorize]
    public async Task<PagesDto> ListPages()
    {
        var pagesFromDb = await _dbContext.Pages.ToListAsync();

        var pagesDto = new PagesDto();

        foreach (var page in pagesFromDb)
        {
            var pageDto = new PageDto
            {
                Id = page.Id,
                Author = page.Author,
                Body = page.Body,
                Title = page.Title
            };

            pagesDto.Pages.Add(pageDto);
        }

        return pagesDto;
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PageDto>> Delete(int id)
    {
        var page = await _dbContext.Pages.FindAsync(id);
        if (page is null)
        {
            return NotFound();
        }
        var pageDto = new PageDto
        {
            Id = page.Id,
            Author = page.Author,
            Body = page.Body,
            Title = page.Title
        };

        _dbContext.Pages.Remove(page);
        await _dbContext.SaveChangesAsync();

        return pageDto;
    }
}
