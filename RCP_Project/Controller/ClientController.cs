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
    public class ClientController : ControllerBase
    {
        private readonly LocalDbContext _context;

        public ClientController(LocalDbContext context)
        {
            _context = context;
        }

        // GET: api/Client
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var individuals = await _context.Individuals.ToListAsync();
            var companies = await _context.Companies.ToListAsync();

            return Ok(individuals.Cast<Client>().Concat(companies.Cast<Client>()));
        }

        // GET: api/Client/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var individual = await _context.Individuals.FindAsync(id);
            var company = await _context.Companies.FindAsync(id);

            if (individual != null)
            {
                return individual;
            }
            else if (company != null)
            {
                return company;
            }
            else
            {
                return NotFound();
            }
        }

        // POST: api/Client/AddIndividual
        [Authorize]
        [HttpPost("AddIndividual")]
        public async Task<ActionResult<Individual>> AddIndividual(IndividualAddDTO individualDTO)
        {
            var existingIndividual = await _context.Individuals
                .FirstOrDefaultAsync(i => i.PESEL == individualDTO.PESEL);

            if (existingIndividual != null)
            {
                return Conflict(new { message = "An individual with the same PESEL already exists." });
            }

            var individual = new Individual
            {
                FirstName = individualDTO.FirstName,
                LastName = individualDTO.LastName,
                PESEL = individualDTO.PESEL,
                Address = individualDTO.Address,
                Email = individualDTO.Email,
                PhoneNumber = individualDTO.PhoneNumber
            };

            _context.Individuals.Add(individual);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = individual.ID }, individual);
        }

        // POST: api/Client/AddCompany
        [Authorize]
        [HttpPost("AddCompany")]
        public async Task<ActionResult<Company>> AddCompany(CompanyAddDTO companyDTO)
        {
            var company = new Company
            {
                CompanyName = companyDTO.CompanyName,
                KRS = companyDTO.KRS,
                Address = companyDTO.Address,
                Email = companyDTO.Email,
                PhoneNumber = companyDTO.PhoneNumber
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = company.ID }, company);
        }

        // DELETE: api/Client/5
        [Authorize(Policy = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var individual = await _context.Individuals.FindAsync(id);
            var company = await _context.Companies.FindAsync(id);

            if (individual != null)
            {
                individual.IsDeleted = true;
                _context.Entry(individual).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            else if (company != null)
            {
                return BadRequest("Data about companies cannot be removed.");
            }
            else
            {
                return NotFound();
            }

            return NoContent();
        }
        
        // PUT: api/Client/UpdateIndividual/5
        [Authorize(Policy = "Admin")]
        [HttpPut("UpdateIndividual/{id}")]
        public async Task<IActionResult> UpdateIndividual(int id, IndividualUpdateDTO individualDTO)
        {
            var existingIndividual = await _context.Individuals.FindAsync(id);

            if (existingIndividual == null)
            {
                return NotFound();
            }

            existingIndividual.FirstName = individualDTO.FirstName;
            existingIndividual.LastName = individualDTO.LastName;
            existingIndividual.Address = individualDTO.Address;
            existingIndividual.Email = individualDTO.Email;
            existingIndividual.PhoneNumber = individualDTO.PhoneNumber;

            _context.Entry(existingIndividual).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        // PUT: api/Client/UpdateCompany/5
        [Authorize(Policy = "Admin")]
        [HttpPut("UpdateCompany/{id}")]
        public async Task<IActionResult> UpdateCompany(int id, CompanyUpdateDTO companyDTO)
        {
            var existingCompany = await _context.Companies.FindAsync(id);

            if (existingCompany == null)
            {
                return NotFound();
            }

            existingCompany.CompanyName = companyDTO.CompanyName;
            existingCompany.Address = companyDTO.Address;
            existingCompany.Email = companyDTO.Email;
            existingCompany.PhoneNumber = companyDTO.PhoneNumber;

            _context.Entry(existingCompany).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
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

        private bool ClientExists(int id)
        {
            return _context.Individuals.Any(e => e.ID == id) || _context.Companies.Any(e => e.ID == id);
        }
    }
}
