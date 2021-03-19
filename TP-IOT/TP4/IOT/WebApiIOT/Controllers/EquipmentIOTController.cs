using DataAccess.IOT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiIOT.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class EquipmentIOTController : ControllerBase
    {
        
        private EquipmentIOTManager Manager { get; set; }

        public EquipmentIOTController(EquipmentIOTManager manager)
        {
            this.Manager = manager;
        }

        [HttpGet("all")]
        public ActionResult<List<EquipmentIOT>> All()
        {
            return this.Manager.ReadAll();
        }

        [HttpGet("allConnected")]
        public ActionResult<List<EquipmentIOT>> AllConnected()
        {
            return this.Manager.ReadAllConnected();
        }

        [HttpGet("get/{id}")]
        public ActionResult<EquipmentIOT> Get(string id)
        {
            return this.Manager.ReadById(id);
        }

        [HttpPost("create")]
        public ActionResult<EquipmentIOT> Create([FromBody] EquipmentIOT channel)
        {
            return this.Manager.Create(channel);
        }

        [HttpPost("edit")]
        public ActionResult<EquipmentIOT> Edit([FromBody] EquipmentIOT channel)
        {
            return this.Manager.Update(channel);
        }

        [HttpDelete("delete/{id}")]
        public ActionResult<bool> Delete(string id)
        {
            return this.Manager.DeleteById(id);
        }

    }
}
