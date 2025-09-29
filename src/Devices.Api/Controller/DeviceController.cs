using System.Text.Json;
using Devices.Api.Data;
using Devices.Api.DTOs;
using Devices.Api.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Devices.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly DevicesDbContext _db;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(DevicesDbContext db, ILogger<DevicesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // POST: api/devices
        [HttpPost]
        public async Task<ActionResult<DeviceReadDto>> Create(DeviceCreateDto dto)
        {
            var device = new Device
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Brand = dto.Brand,
                State = dto.State,
                CreationTime = DateTimeOffset.UtcNow
            };

            _db.Devices.Add(device);
            await _db.SaveChangesAsync();

            var read = new DeviceReadDto
            {
                Id = device.Id,
                Name = device.Name,
                Brand = device.Brand,
                State = device.State,
                CreationTime = device.CreationTime
            };

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, read);
        }

        // GET: api/devices/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DeviceReadDto>> GetById(Guid id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();

            return new DeviceReadDto
            {
                Id = device.Id,
                Name = device.Name,
                Brand = device.Brand,
                State = device.State,
                CreationTime = device.CreationTime
            };
        }

        // GET: api/devices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceReadDto>>> GetAll([FromQuery] string? brand, [FromQuery] DeviceState? state)
        {
            var q = _db.Devices.AsQueryable();
            if (!string.IsNullOrWhiteSpace(brand)) q = q.Where(d => d.Brand == brand);
            if (state.HasValue) q = q.Where(d => d.State == state.Value);

            var list = await q.Select(d => new DeviceReadDto
            {
                Id = d.Id,
                Name = d.Name,
                Brand = d.Brand,
                State = d.State,
                CreationTime = d.CreationTime
            }).ToListAsync();

            return Ok(list);
        }

        // PUT: api/devices/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Replace(Guid id, DeviceUpdateDto dto)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();

            // Business rule: cannot update name/brand if in-use
            if (device.State == DeviceState.InUse &&
                (device.Name != dto.Name || device.Brand != dto.Brand))
            {
                return BadRequest("Cannot update name or brand while device is in-use.");
            }

            device.Name = dto.Name;
            device.Brand = dto.Brand;
            device.State = dto.State;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/devices/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonElement body)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();

            if (Request.ContentType == "application/json-patch+json")
            {
                var patch = await HttpContext.Request.ReadFromJsonAsync<JsonPatchDocument<DeviceUpdateDto>>();
                if (patch == null) return BadRequest();

                var dto = new DeviceUpdateDto
                {
                    Name = device.Name,
                    Brand = device.Brand,
                    State = device.State
                };

                patch.ApplyTo(dto, ModelState);
                if (!ModelState.IsValid) return ValidationProblem(ModelState);

                if (device.State == DeviceState.InUse &&
                    (device.Name != dto.Name || device.Brand != dto.Brand))
                {
                    return BadRequest("Cannot update name or brand while device is in-use.");
                }

                device.Name = dto.Name;
                device.Brand = dto.Brand;
                device.State = dto.State;

                await _db.SaveChangesAsync();
                return NoContent();
            }

            // Handle partial JSON object (application/json)
            var partial = await HttpContext.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
            if (partial == null) return BadRequest();

            if (partial.ContainsKey("creationTime"))
                return BadRequest("CreationTime cannot be updated.");

            if (device.State == DeviceState.InUse &&
                (partial.ContainsKey("name") || partial.ContainsKey("brand")))
            {
                return BadRequest("Cannot update name or brand while device is in-use.");
            }

            if (partial.ContainsKey("name"))
                device.Name = partial["name"].GetString() ?? device.Name;

            if (partial.ContainsKey("brand"))
                device.Brand = partial["brand"].GetString() ?? device.Brand;

            if (partial.ContainsKey("state"))
            {
                var stateStr = partial["state"].GetString();
                if (!string.IsNullOrEmpty(stateStr) &&
                    Enum.TryParse<DeviceState>(stateStr, true, out var st))
                {
                    device.State = st;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/devices/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var device = await _db.Devices.FindAsync(id);
            if (device == null) return NotFound();

            if (device.State == DeviceState.InUse)
                return BadRequest("In-use devices cannot be deleted.");

            _db.Devices.Remove(device);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}