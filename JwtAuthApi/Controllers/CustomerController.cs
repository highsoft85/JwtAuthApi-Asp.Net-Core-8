using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApi.Controllers;

//[ApiVersion(1.0)]
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    [HttpGet, Authorize]
    //[HttpGet, Authorize(Roles = "Manager")] // In case of authorize with Role
    public IEnumerable<string> Get()
    {
        return new string[] { "John Doe", "Jane Doe" };
    }
}
