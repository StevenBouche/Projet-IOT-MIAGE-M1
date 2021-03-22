using DataAccess.IOT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiIOT.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class DataIOTController : ControllerBase
    {

        private DataIOTManager Manager { get; set; }

        public DataIOTController(DataIOTManager manager)
        {
            this.Manager = manager;
        }

        [HttpGet("equipment/{id}")]
        public ActionResult<List<DataIOT>> EquipmentData(string id)
        {
            return this.Manager.GetAllDataOfEquipment(id);
        }

        [HttpGet("all")]
        public ActionResult<List<DataIOT>> All()
        {
            return this.Manager.ReadAll();
        }

        [HttpGet("get/{id}")]
        public ActionResult<DataIOT> Get(string id)
        {
            return this.Manager.ReadById(id);
        }

        [HttpPost("create")]
        public ActionResult<DataIOT> Create([FromBody] DataIOT channel)
        {
            return this.Manager.Create(channel);
        }

        [HttpPost("edit")]
        public ActionResult<DataIOT> Edit([FromBody] DataIOT channel)
        {
            return this.Manager.Update(channel);
        }

        [HttpDelete("delete/{id}")]
        public ActionResult<bool> Delete(string id)
        {
            return this.Manager.DeleteById(id);
        }

        [HttpPost("search")]
        public ActionResult<List<DataIOT>> Search([FromBody] SearchDataPeriodView search)
        {
            return this.Manager.Search(search);
        }

        [HttpPost("searchLast")]
        public ActionResult<List<DataIOT>> SearchLast([FromBody] SearchDataPeriodView search)
        {
            return this.Manager.SearchLast(search);
        }

    }
}
