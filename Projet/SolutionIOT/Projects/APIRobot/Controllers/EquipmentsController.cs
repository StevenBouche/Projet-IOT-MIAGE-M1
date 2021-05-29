using APIRobot.HostedServices;
using APIRobot.Models;
using APIRobot.Models.Auth;
using APIRobot.Models.Data;
using APIRobot.Services;
using APIRobot.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace APIRobot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentsController : ControllerBase
    {

        private readonly DataRobotService ManagerData;
        private readonly UsersService ManagerUsers;
        private readonly ICacheEquipment Cache;
        public EquipmentsController(ICacheEquipment cache, UsersService ManagerUsers, DataRobotService ManagerData)
        {
            this.Cache = cache;
            this.ManagerData = ManagerData;
            this.ManagerUsers = ManagerUsers;
        }

        [HttpGet]
        public ActionResult<List<EquipmentConnection>> Get()
        {
            var connections = this.Cache.GetEquipmentConnected();

            return Ok(connections);
        }

        [HttpGet("data/{id}")]
        public ActionResult<List<DataRobot>> EquipmentData(string id)
        {
            return ManagerData.GetAllDataOfEquipment(id);
        }

        [HttpGet("datas")]
        public ActionResult<List<DataRobot>> All()
        {
            return ManagerData.ReadAll();
        }

        [HttpPost("searchData")]
        public ActionResult<List<DataRobot>> Search([FromBody] SearchDataPeriodView search)
        {
            return ManagerData.Search(search);
        }

        [HttpPost("searchDataLast")]
        public ActionResult<List<DataRobot>> SearchLast([FromBody] SearchDataPeriodView search)
        {
            return ManagerData.SearchLast(search);
        }
    }
}
