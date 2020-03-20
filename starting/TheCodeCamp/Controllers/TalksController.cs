using AutoMapper;
using System;
using System.Collections;
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
    [RoutePrefix("api/camps/{moniker}/talks")]
    public class TalksController : ApiController
    {
        private readonly ICampRepository _Repository;
        private readonly IMapper _Mapper;

        public TalksController(ICampRepository repository, IMapper mapper)
        {
            _Repository = repository;
            _Mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var results = await _Repository.GetTalksByMonikerAsync(moniker, includeSpeakers);

                return Ok(_Mapper.Map<IEnumerable<TalkModel>>(results));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{id:int}", Name = "GetTalk")]
        public async Task<IHttpActionResult> Get(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var result = await _Repository.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                if (result == null) return NotFound();

                return Ok(_Mapper.Map<TalkModel>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(string moniker, TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var camp = await _Repository.GetCampAsync(moniker);
                    if (camp != null)
                    {
                        var talk = _Mapper.Map<Talk>(model);
                        talk.Camp = camp;

                        // Map the speaker if necessary
                        if (model.Speaker != null)
                        {
                            var speaker = await _Repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                            if (speaker != null) talk.Speaker = speaker;
                        }

                        _Repository.AddTalk(talk);

                        if (await _Repository.SaveChangesAsync())
                        {
                            return CreatedAtRoute("GetTalk", 
                                new { moniker = moniker, id = talk.TalkId }, _Mapper.Map<TalkModel>(talk));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Put(string moniker, int talkId, TalkModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var talk = await _Repository.GetTalkByMonikerAsync(moniker, talkId, true);
                    if (talk == null) return NotFound();

                    // Automapping will ignore the speaker
                    _Mapper.Map(model, talk);

                    // Change speaker if needed
                    if (talk.Speaker.SpeakerId != model.Speaker.SpeakerId)
                    {
                        var speaker = await _Repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                        if (speaker != null) talk.Speaker = speaker;
                    }

                    if (await _Repository.SaveChangesAsync())
                    {
                        return Ok(_Mapper.Map<TalkModel>(talk));
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                var talk = await _Repository.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null) return NotFound();

                _Repository.DeleteTalk(talk);

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
            }
        }
    }
}
