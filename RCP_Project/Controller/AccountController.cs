using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using RCP_Project.Models;
using RCP_Project.DTO;
using RCP_Project.Service;
using LoginRequest = RCP_Project.DTO.LoginRequest;
using RegisterRequest = RCP_Project.DTO.RegisterRequest;

namespace RCP_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(LocalDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]  
        [HttpPost("register")]  
        public IActionResult RegisterEmployee(RegisterRequest model)  
        {  
            var hashedPasswordAndSalt = SecurityService.GetHashedPasswordAndSalt(model.Password);  
            
            var employee = new Employee()  
            {  
                Login = model.Login,
                Role = model.Role,
                Password = hashedPasswordAndSalt.Item1,
                Salt = hashedPasswordAndSalt.Item2,
                RefreshToken = SecurityService.GenerateRefreshToken(),
                RefreshTokenExp = DateTime.Now.AddDays(1)
            };  
            
            
            
            _context.Employees.Add(employee);  
            _context.SaveChanges();  
            return Ok(); 
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            Employee employee = _context.Employees.Where(e => e.Login == loginRequest.Login).FirstOrDefault();

            string passwordHashFromDb = employee.Password;
            string curHashedPassword = SecurityService.GetHashedPasswordWithSalt(loginRequest.Password, employee.Salt);

            if (passwordHashFromDb != curHashedPassword)
            {
                return Unauthorized();
            }

            Claim[] userclaim = new[]
            {
                new Claim(ClaimTypes.Role, employee.Role)
            };

            SymmetricSecurityKey key = new
                SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Issuer"],
                audience: _configuration["Audience"],
                claims: userclaim,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );
            employee.RefreshToken = SecurityService.GenerateRefreshToken();
            employee.RefreshTokenExp = DateTime.Now.AddDays(1);
            _context.SaveChanges();

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = employee.RefreshToken
            });
        }
        
        [Authorize(AuthenticationSchemes = "IgnoreTokenExpirationScheme")]
        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenRequest refreshToken)
        {
            Employee employee = _context.Employees.Where(e => e.RefreshToken == refreshToken.RefreshToken).FirstOrDefault();
            if (employee == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (employee.RefreshTokenExp < DateTime.Now)
            {
                throw new SecurityTokenException("Refresh token expired");
            }
        
            Claim[] userclaim = new[]
            {
                new Claim(ClaimTypes.Role, employee.Role)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken jwtToken = new JwtSecurityToken(
                issuer: _configuration["Issuer"],
                audience: _configuration["Audience"],
                claims: userclaim,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            employee.RefreshToken = SecurityService.GenerateRefreshToken();
            employee.RefreshTokenExp = DateTime.Now.AddDays(1);
            _context.SaveChanges();

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                refreshToken = employee.RefreshToken
            });
        }


        
    }
}