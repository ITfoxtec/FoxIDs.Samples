using System;
using System.Collections.Generic;
using AspNetCoreApi1Sample.Policys;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreApi1Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Api1SomeAccessScopeAuthorize]
    public class ValuesApi2Controller : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            // TODO call API2
            throw new NotImplementedException();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            // TODO call API2
            throw new NotImplementedException();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            // TODO call API2
            throw new NotImplementedException();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            // TODO call API2
            throw new NotImplementedException();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // TODO call API2
            throw new NotImplementedException();
        }
    }
}
