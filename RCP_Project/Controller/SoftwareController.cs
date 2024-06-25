using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCP_Project.DTO;
using RCP_Project.Models;

namespace RCP_Project.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftwareController : ControllerBase
    {
        private readonly LocalDbContext _context;

        public SoftwareController(LocalDbContext context)
        {
            _context = context;
        }

        // GET: api/Software
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Software>>> GetSoftware()
        {
            return await _context.Software.ToListAsync();
        }

        // GET: api/Software/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Software>> GetSoftware(int id)
        {
            var software = await _context.Software.FindAsync(id);

            if (software == null)
            {
                return NotFound();
            }

            return software;
        }

        // PUT: api/Software/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSoftware(int id, SoftwareDTO softwareDto)
        {
            var software = await _context.Software.FindAsync(id);
            if (software == null)
            {
                return NotFound();
            }

            software.Name = softwareDto.Name;
            software.Description = softwareDto.Description;
            software.CurrentVersion = softwareDto.CurrentVersion;
            software.UpfrontCost = softwareDto.UpfrontCost;
            software.Category = softwareDto.Category;

            _context.Entry(software).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SoftwareExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Software
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Software>> PostSoftware(SoftwareDTO softwareDto)
        {
            var software = new Software
            {
                Name = softwareDto.Name,
                Description = softwareDto.Description,
                CurrentVersion = softwareDto.CurrentVersion,
                UpfrontCost = softwareDto.UpfrontCost,
                Category = softwareDto.Category
            };

            _context.Software.Add(software);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSoftware", new { id = software.ID }, software);
        }

        // DELETE: api/Software/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSoftware(int id)
        {
            var software = await _context.Software.FindAsync(id);
            if (software == null)
            {
                return NotFound();
            }

            _context.Software.Remove(software);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SoftwareExists(int id)
        {
            return _context.Software.Any(e => e.ID == id);
        }
    }
}
