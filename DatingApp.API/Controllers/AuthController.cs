using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController (IAuthRepository repo, IConfiguration config) {
            _repo = repo;
            _config = config;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register (UserToRegisterDto userToRegister) {

            //TODO : Validate Request
            userToRegister.Username = userToRegister.Username.ToLower ();
            if (await _repo.UserExists (userToRegister.Username)) {
                return BadRequest ("Username already exists!");
            }
            var userToCreate = new User {
                Username = userToRegister.Username
            };
            var createdUser = await _repo.Register (userToCreate, userToRegister.Password);
            //TODO : return CreatedAtRoute()
            return StatusCode (201);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserToLoginDto userToLoginDto) {
            try {
                var userFromRepo = await _repo.Login (userToLoginDto.Username.ToLower (), userToLoginDto.Password);
                if (userFromRepo == null) {
                    return Unauthorized ();
                }
                var claims = new [] {
                    new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString ()),
                    new Claim (ClaimTypes.Name, userFromRepo.Username)
                };
                var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_config.GetSection ("AppSettings:Token").Value));
                var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);
                var tokenDescriptor = new SecurityTokenDescriptor {
                    Subject = new ClaimsIdentity (claims),
                    Expires = DateTime.Now.AddDays (1),
                    SigningCredentials = creds
                };
                var tokenHandler = new JwtSecurityTokenHandler ();
                var token = tokenHandler.CreateToken (tokenDescriptor);
                return Ok (new {
                    token = tokenHandler.WriteToken (token)
                });
            } catch {
                throw new Exception ("Exception while logging in ");
            }
        }
    }
}