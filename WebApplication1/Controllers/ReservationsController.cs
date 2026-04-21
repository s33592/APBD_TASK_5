using Microsoft.AspNetCore.Mvc;
using WebApplication1.Database;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {

        [HttpGet]
        public IActionResult GetFiltered([FromQuery] DateOnly? date, [FromQuery] string? status, [FromQuery] int? roomId)
        {
            var query = DataStore.Reservations.AsQueryable();

            if (date.HasValue)
                query = query.Where(r => r.Date == date.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (roomId.HasValue)
                query = query.Where(r => r.RoomId == roomId.Value);

            return Ok(query.ToList());
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var res = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            return res == null ? NotFound() : Ok(res);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Reservation res)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == res.RoomId);

            if (room == null) return NotFound("Room does not exist.");
            if (!room.IsActive) return BadRequest("Room is currently inactive.");

            bool isOverlapping = DataStore.Reservations.Any(existing =>
                existing.RoomId == res.RoomId &&
                existing.Date == res.Date &&
                existing.Status != "cancelled" &&
                res.StartTime < existing.EndTime && res.EndTime > existing.StartTime);

            if (isOverlapping) return Conflict("Overlap detected: The room is already booked for this time.");

            res.Id = DataStore.NextReservationId;
            DataStore.Reservations.Add(res);
            return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Reservation updatedRes)
        {
            var index = DataStore.Reservations.FindIndex(r => r.Id == id);
            if (index == -1) return NotFound();

            updatedRes.Id = id;
            DataStore.Reservations[index] = updatedRes;
            return Ok(updatedRes);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var res = DataStore.Reservations.FirstOrDefault(r => r.Id == id);
            if (res == null) return NotFound();

            DataStore.Reservations.Remove(res);
            return NoContent();
        }
    }
}