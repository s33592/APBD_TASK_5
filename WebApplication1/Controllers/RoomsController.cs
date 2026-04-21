using Microsoft.AspNetCore.Mvc;
using WebApplication1.Database;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? minCapacity, [FromQuery] bool? hasProjector, [FromQuery] bool? activeOnly)
        {
            var query = DataStore.Rooms.AsQueryable();

            if (minCapacity.HasValue) query = query.Where(r => r.Capacity >= minCapacity);
            if (hasProjector.HasValue) query = query.Where(r => r.HasProjector == hasProjector);
            if (activeOnly == true) query = query.Where(r => r.IsActive);

            return Ok(query.ToList());
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            return room == null ? NotFound() : Ok(room);
        }


        [HttpGet("building/{buildingCode}")]
        public IActionResult GetByBuilding(string buildingCode)
        {
            var rooms = DataStore.Rooms
                .Where(r => r.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(rooms);
        }


        [HttpPost]
        public IActionResult Create(Room room)
        {
            room.Id = DataStore.NextRoomId;
            DataStore.Rooms.Add(room);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }


        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Room updatedRoom)
        {
            var index = DataStore.Rooms.FindIndex(r => r.Id == id);
            if (index == -1) return NotFound();

            updatedRoom.Id = id;
            DataStore.Rooms[index] = updatedRoom;
            return Ok(updatedRoom);
        }


        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var room = DataStore.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            if (DataStore.Reservations.Any(res => res.RoomId == id))
            {
                return Conflict("Cannot delete room because it has associated reservations.");
            }

            DataStore.Rooms.Remove(room);
            return NoContent();
        }
    }
}
