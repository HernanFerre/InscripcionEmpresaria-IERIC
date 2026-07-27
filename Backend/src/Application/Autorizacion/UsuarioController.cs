
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Linq;
using static Auth.JWT;

namespace Auth
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;


        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        [Route("All")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> GetAllAsync()
        {

            return Ok(await _usuarioRepository.GetAllAsync());
        }

        [HttpPost]
        [Route("RegistrarContacto")]
        public async Task<ActionResult<bool>> RegistrarContacto(string email, string contacto)
        {
            AuthorizationIdentity identidad = JWT.GetAuthorizationIdentity(this.User.Claims);

            return Ok(await _usuarioRepository.RegistrarContacto(new Guid(identidad.id), email, contacto, identidad.name));
        }

        [HttpPost]
        [Route("RegistrarContactoById")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<bool>> RegistrarContacto(Guid id, string email, string contacto)
        {
            AuthorizationIdentity identidad = JWT.GetAuthorizationIdentity(this.User.Claims);

            return Ok(await _usuarioRepository.RegistrarContacto(id, email, contacto, identidad.name));
        }
    }
}