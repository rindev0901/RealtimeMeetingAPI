using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Extensions;
using RealtimeMeetingAPI.Helpers;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms([FromQuery] RoomParams roomParams)
        {
            var comments = await _unitOfWork.RoomRepository.GetAllRoomAsync(roomParams);
            //Response.AddPaginationHeader(comments.CurrentPage, comments.PageSize, comments.TotalCount, comments.TotalPages);

            return Ok(comments);
        }

        [HttpGet("list/{id:guid}")]
        public async Task<ActionResult<Room>> GetRoomById([FromRoute] Guid id)
        {
            var room = await _unitOfWork.RoomRepository.GetRoomById(id);
            if(room == null)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                        Response<object>.Result(null, "Room not already exist",
                        StatusCodes.Status404NotFound)
                    );
            }
            return StatusCode(StatusCodes.Status200OK,
                        Response<Room>.Result(room, "Get room successfully",
                        StatusCodes.Status200OK)
                    );
        }

        [HttpPost]
        [Route("create")]
        public async Task<ActionResult> AddRoom([FromQuery]string name)
        {
            var room = new Room { RoomName = name, UserId = User.GetUserId() };

            _unitOfWork.RoomRepository.AddRoom(room);

            if (await _unitOfWork.Complete())
            {
                var newRoom = await _unitOfWork.RoomRepository.GetRoomDtoById(room.RoomId);
                return StatusCode(StatusCodes.Status200OK,
                        Response<RoomDto>.Result(newRoom, "Add new room successfully",
                        StatusCodes.Status200OK)
                    );
            }

            return StatusCode(StatusCodes.Status400BadRequest,
                        Response<object>.Result(null, "An error occurred while adding a new meeting",
                        StatusCodes.Status400BadRequest)
                    );
        }

        [HttpPut]
        [Route("update/{id:guid}")]
        public async Task<ActionResult> EditRoom([FromRoute]Guid id, [FromQuery]string editName)
        {
            var room = await _unitOfWork.RoomRepository.EditRoom(id, editName);
            if (room != null)
            {
                if (_unitOfWork.HasChanges())
                {
                    if (await _unitOfWork.Complete())
                        return Ok(new RoomDto { RoomId = room.RoomId, RoomName = room.RoomName, UserId = room.UserId.ToString() });
                    return BadRequest("Problem edit room");
                }
                else
                {
                    return NoContent();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult> DeleteRoom([FromRoute]Guid id)
        {
            var entity = await _unitOfWork.RoomRepository.DeleteRoom(id);

            if (entity != null)
            {
                if (await _unitOfWork.Complete())
                    return Ok(new RoomDto { RoomId = entity.RoomId, RoomName = entity.RoomName, UserId = entity.UserId.ToString() });
                return BadRequest("Problem delete room");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllRoom()
        {
            await _unitOfWork.RoomRepository.DeleteAllRoom();

            if (_unitOfWork.HasChanges())
            {
                if (await _unitOfWork.Complete())
                    return Ok();//xoa thanh cong
                return BadRequest("Problem delete all room");
            }
            else
            {
                return NoContent();//ko co gi de xoa
            }
        }
    }
}
