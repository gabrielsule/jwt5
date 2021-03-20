Inicialización del proyecto
```bash
mkdir jwt5 && cd jwt5

dotnet new webapi
```

Instalar librerías
```bash
dotnet add package Microsoft.AspNetCore.Authentication 

dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Creación de modelo
```csharp
namespace jwt5.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
```

creación de repositorio (fake)
```csharp
using jwt5.Models;
using System.Collections.Generic;
using System.Linq;

namespace jwt5.Repositories
{
    public static class UserRepo
    {
        public static UserModel Get(string username, string password)
        {
            var users = new List<UserModel>();

            users.Add(new UserModel { Id = 1, Username = "clarke", Password = "abc123", Role = "admin" });

            users.Add(new UserModel { Id = 2, Username = "asimov", Password = "abc123", Role = "dev" });

            users.Add(new UserModel { Id = 3, Username = "bradbury", Password = "abc123", Role = "test" });

            users.Add(new UserModel { Id = 4, Username = "dick", Password = "abc123", Role = "user" });

            return users.Where(x => x.Username.ToLower() == username.ToLower() && x.Password == password).FirstOrDefault();
        }
    }
}
```

Creación de seteo de secret key
> para no incluirla en el appsettings.json
```csharp
namespace jwt5
{
    public static class Settings
    {
        public static string Secret = "6hXdT*kjC8Jg86E5b#4r22ttDf!5s#in";
    }
}
```

Creación de servicio de token
```csharp
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using jwt5.Models;

namespace jwt5.Services
{
    public static class TokenService
    {
        private const double EXPIRE_HOURS = 1.0;

        public static string CreateToken(UserModel user)
        {
            var key = Encoding.ASCII.GetBytes(Settings.Secret);

            var tokenHandler = new JwtSecurityTokenHandler();

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),

                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(EXPIRE_HOURS),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
```

Modificación de startup.cs
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

...

public void ConfigureServices(IServiceCollection services)
{
    //configuracion de jwt
    var key = Encoding.ASCII.GetBytes(Settings.Secret);

    services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

    });
}

...

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // uso de la autenticacion
    app.UseAuthentication();

    app.UseAuthorization();
}    
```

Creación de controlador
```csharp
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using jwt5.Models;
using jwt5.Services;
using jwt5.Repositories;

namespace jwt5.Controllers
{
    [Route("api/[controller]")]
    public class DummyController : Controller
    {

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public ActionResult<dynamic> Authenticate([FromBody] UserModel model)
        {
            var user = UserRepo.Get(model.Username, model.Password);

            if (user == null)
            {
                return NotFound(new { message = "credenciales inválidas" });
            }

            var token = TokenService.CreateToken(user);

            user.Password = "";

            return new
            {
                user = user,
                token = token
            };
        }

        [HttpGet]
        [Route("anonymous")]
        [AllowAnonymous]
        public string Anonymous()
        {
            return "only anonymous";
        }

        [HttpGet]
        [Route("authenticated")]
        [Authorize]
        public string Authenticated()
        {
            return String.Format("Authenticated - {0}", User.Identity.Name);
        }

        [HttpGet]
        [Route("admin")]
        [Authorize(Roles = "admin")]
        public string Admin()
        {
            return "only admin";
        }

        [HttpGet]
        [Route("developer")]
        [Authorize(Roles = "admin, developer")]
        public string Dev()
        {
            return "only admin & developer";
        }

        [HttpGet]
        [Route("tester")]
        [Authorize(Roles = "admin, tester")]
        public string Tester()
        {
            return "only admin & tester";
        }

        [HttpGet]
        [Route("user")]
        [Authorize(Roles = "admin, user")]
        public string User()
        {
            return "only admin & user";
        }
    }
}
```

Ejecutar web api
```bash
dotnet run
```

Testear desde terminal, insomnia o postman
```bash
curl -X GET "http://localhost:5000/api/Dummy/anonymous" -H  "accept: text/plain"

curl -X POST "http://localhost:5000/api/Dummy/login" -H  "accept: */*" -H  "Content-Type: application/json" -d "{\"username\":\"clarke\",\"password\":\"abc123\"}"

curl -X GET "http://localhost:5000/api/Dummy/authenticated" -H  "accept: text/plain"

curl -X GET "http://localhost:5000/api/Dummy/admin" -H  "accept: text/plain"
```