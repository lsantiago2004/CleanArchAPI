using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Product.Controllers
{
    /// <summary>
    /// Hello World
    /// </summary>
    [Route("v2")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        /// <summary>
        /// Tests connect to api
        /// </summary>
        /// <returns></returns>
        [HttpGet("Hello")]
        public Sheev.Common.Models.HelloWorldResponse Test()
        {
            var major = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            var minor = Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            var revision = Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();

            var response = new Sheev.Common.Models.HelloWorldResponse()
            {
                WelcomeMessage = $"Products API up and running!",
                Version = $"{major}.{minor}.{revision}",
                Build = Assembly.GetExecutingAssembly().GetName().Version.Build.ToString()
            };

            return response;
        }
    }
}