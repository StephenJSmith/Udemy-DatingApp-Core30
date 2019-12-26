using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]

  public class ValuesController : ControllerBase
  {
    private readonly DataContext _context;

    public ValuesController(DataContext context)
    {
      _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetValues()
    {
      var values = await _context.Values.ToListAsync();

      return Ok(values);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetValue(int id)
    {
      var value = await _context.Values.FirstOrDefaultAsync(v => v.Id == id);

      return Ok(value);
    }

    [HttpPost]
    public void Post([FromBody] string value)
    {

    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {

    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {

    }
  }
}