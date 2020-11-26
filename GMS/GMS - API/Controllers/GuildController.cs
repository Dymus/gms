﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GMS___Business_Layer;
using GMS___Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GMS___API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuildController : ControllerBase
    {
        public IOptions<ClientSettings> clientSettings;
        private EventProcessor eventProcessor;
        private EventCharacterProcessor eventCharacterProcessor;
        public GuildController(IOptions<ClientSettings> clientSettings)
        {
            this.clientSettings = clientSettings;
            eventProcessor = new EventProcessor();
            eventCharacterProcessor = new EventCharacterProcessor();
        }

        [HttpGet("{guildId}")]
        public IEnumerable<Event> GetAll(string guildId) => eventProcessor.GetAllGuildEvents(guildId);

        [HttpGet("events/{eventId}")]
        public IEnumerable<Event> Get(string eventId) => eventProcessor.GetEventByID(Int32.Parse(eventId));

        [HttpGet("{guildId}/type/{type}")]
        public IEnumerable<Event> GetByType(string guildId, string type) => eventProcessor.GetAllGuildEventsByEventType(guildId, type);

        /*[HttpGet("{guildId}/Name/{name}")]
        public IEnumerable<Event> GetByName(string guildId, string name) => eventProcessor.GetAllGuildEventsByName(guildId, name);*/

        //Get all the events a user participates in (takes guildID and characterName)
        [HttpGet("{guildId}/character/{characterName}")]
        public IEnumerable<Event> GetByCharacterName(string guildID, string characterName) => eventProcessor.GetGuildEventsByCharacterName(guildID, characterName);

        [HttpPost("events/insert")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Event> Post([FromBody] Event e)
        {
            if (eventProcessor.InsertEvent(e.Name, e.EventType, e.Location, e.Date, e.Description, e.MaxNumberOfCharacters, e.GuildID)) 
            {
                return e;
            }
            return BadRequest("Invalid data.");
        }

        [HttpPost("events/join")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<EventCharacter> Post([FromBody] EventCharacter ec)
        {
            if (eventCharacterProcessor.JoinEvent(ec.EventID, ec.CharacterName, ec.CharacterRole, ec.SignUpDateTime)) { 
                return ec;
            }
            return BadRequest("Invalid data.");
        }

        [HttpDelete("events/{eventId}/withdraw/{characterName}")]
        public string DeleteEventCharacter(string eventId, string characterName)
        {
            if (eventCharacterProcessor.DeleteEventCharacterByEventIDAndCharacterName(Int32.Parse(eventId), characterName))
            {
                return "Succesfully deleted";
            }
            else
            {
                return "Not succesfully deleted";
            }
        }

        [HttpDelete("events/remove/{eventId}")]
        public string DeleteEvent(string eventId) 
        {
            if(eventProcessor.DeleteEventByID(Int32.Parse(eventId)))
            {
                return "Succesfully deleted";
            } else
            {
                return "Not succesfully deleted";
            }
        }

    }
}
