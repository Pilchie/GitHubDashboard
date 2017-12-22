using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GitHubDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/Query")]
    public class QueryController : Controller
    {
        [HttpGet("[action]")]
        public int Count()
        {
            return 42;
        }
    }
}