using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TheCodeCamp.Data;
using TheCodeCamp.Models;

namespace TheCodeCamp.Controllers
{
    [RoutePrefix("api/camps")]
    public class CampsController : ApiController
    {
        private readonly ICampRepository _Repository;
        private readonly IMapper _Mapper;

        public CampsController(ICampRepository repository, IMapper mapper)
        {
            _Repository = repository;
            _Mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var result = await _Repository.GetAllCampsAsync(includeTalks);

                // Mapping
                var mappedResult = _Mapper.Map<IEnumerable<CampModel>>(result);

                return Ok(mappedResult);
            }
            catch (Exception ex)
            {
                // TODO: Add Logging
                return InternalServerError();
            }
        }

        [Route("{moniker}", Name = "GetCamp")]
        public async Task<IHttpActionResult> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await _Repository.GetCampAsync(moniker, includeTalks);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(_Mapper.Map<CampModel>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("searchByDate/{eventDate:datetime}")]
        [HttpGet]
        public async Task<IHttpActionResult> SearchByEventDate(DateTime eventDate, bool includeTalks = false)
        {
            try
            {
                var result = await _Repository.GetAllCampsByEventDate(eventDate, includeTalks);
                return Ok(_Mapper.Map<CampModel[]>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(CampModel model)
        {
            try
            {
                if (await _Repository.GetCampAsync(model.Moniker) != null)
                {
                    ModelState.AddModelError("Moniker", "Moniker in use");
                }

                if (ModelState.IsValid)
                {
                    var camp = _Mapper.Map<Camp>(model);

                    _Repository.AddCamp(camp);
                    if (await _Repository.SaveChangesAsync())
                    {
                        var newModel = _Mapper.Map<CampModel>(camp);

                        return CreatedAtRoute("GetCamp", new { moniker = newModel.Moniker }, newModel);
                    };
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{moniker}")]
        public async Task<IHttpActionResult> Put(string moniker, CampModel model)
        {
            try
            {
                var camp = await _Repository.GetCampAsync(moniker);

                if (camp == null) return NotFound();

                _Mapper.Map(model, camp);

                if (await _Repository.SaveChangesAsync())
                {
                    return Ok(_Mapper.Map<CampModel>(camp));
                }
                else
                {
                    return InternalServerError();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{moniker}")]
        public async Task<IHttpActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await _Repository.GetCampAsync(moniker);
                if (camp == null) return NotFound();

                _Repository.DeleteCamp(camp);

                if (await _Repository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return InternalServerError();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
                throw;
            }
        }
    }
}
