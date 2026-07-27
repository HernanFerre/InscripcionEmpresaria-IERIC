
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

namespace Auth
{
    [Route("v1/[controller]")]
    [ApiController]
    public class AutorizacionController : ControllerBase
    {
        private readonly IAutorizacionRepository _autorizacionRepository;


        public AutorizacionController(IAutorizacionRepository autorizacion)
        {
            _autorizacionRepository = autorizacion;
        }

        [HttpGet]
        public async Task<ActionResult<AutorizacionDTO>> Index(string token)
        {

            return Ok(_autorizacionRepository.AutorizarAsync(token));
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<ActionResult<Guid>> RegistrarUsuario(string token)
        {
            return Ok(await _autorizacionRepository.RegistrarUsuarioAsync(token));
        }

        [HttpPost]
        [Route("ActivarUsuario")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<bool>> ActivarUsuario(Guid id, string roles, string area)
        {
            return Ok(await _autorizacionRepository.ActivarUsuarioAsync(id, Rol.FromStringRepresentation(roles), area));
        }
    }
}