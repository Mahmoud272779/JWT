using JWT.API.Data.Context;
using JWT.API.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration;

        public AuthController(ApplicationDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.configuration = configuration;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> login(LoginModel m)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = db.Users.FirstOrDefault(u => m.password == u.password && m.username == u.username);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.token!=null)
            {
                return Ok("User was already Created before and his token: " +
                    ""+user.token);
            }

            //create the Token

            dto obj = new dto(configuration);

            string token = obj.createToken(m);

            user.token = token;
            db.SaveChanges();
            return Ok("UserToken: " +token);
        }

        [HttpPost("SignUp")]

        public async Task<IActionResult> adduser(LoginModel u)
        {
            var user = db.Users.FirstOrDefault(c => c.username == u.username || c.password == u.password);
            if (user != null)
            {
                return Ok("Username or Password exists");
            }


            //LoginModel loginModel = new LoginModel
            //{
            //    username = u.username,
            //    password = u.password,
            //};

            dto obj = new dto(configuration);

            string t = obj.createToken(u);

            var result = new User
            {
                password = u.password,
                username = u.username,
                token = t
            };
            db.Users.Add(result);
            db.SaveChanges();
            return Ok(result);
        }

        [HttpGet("GetallUsersWithTokens")]
        public async Task<IActionResult> GetallUsersWithTokens()
        {
            var allusers = db.Users.ToList();

            return Ok(allusers.Select(u => new
            {
                username = u.username,
                Token = u.token


            }));

        }

    }

    public class dto
    {
        private readonly IConfiguration configuration;

        public dto(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string createToken(LoginModel user)
        {

            var claims = new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub,user.username),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var signkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

            var signCred = new SigningCredentials(signkey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: configuration["Jwt:issuer"],

        audience: configuration["Jwt:Audience"],
        claims: claims,

        signingCredentials: signCred,
        expires: DateTime.Now.AddDays(7)

    );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }


}
