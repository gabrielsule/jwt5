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




    }
}