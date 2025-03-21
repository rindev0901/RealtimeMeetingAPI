﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RealtimeMeetingAPI.Data;
using RealtimeMeetingAPI.Dtos;
using RealtimeMeetingAPI.Entities;
using RealtimeMeetingAPI.Helpers;
using RealtimeMeetingAPI.Interfaces;

namespace RealtimeMeetingAPI.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RoomRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Room> GetRoomById(Guid roomId)
        {
            return await _context.Rooms.Include(x => x.Connections).FirstOrDefaultAsync(x => x.RoomId == roomId);
        }

        public async Task<RoomDto> GetRoomDtoById(Guid roomId)
        {
            return await _context.Rooms.Where(r => r.RoomId == roomId).ProjectTo<RoomDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();//using Microsoft.EntityFrameworkCore;
        }

        public async Task<Room> GetRoomForConnection(string connectionId)
        {
            return await _context.Rooms.Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public void AddRoom(Room room)
        {
            _context.Rooms.Add(room);
        }

        /// <summary>
        /// return null no action to del else delete thanh cong
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Room> DeleteRoom(Guid id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }
            return room;
        }

        public async Task<Room> EditRoom(Guid id, string newName)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.RoomName = newName;
            }
            return room;
        }

        public async Task DeleteAllRoom()
        {
            var list = await _context.Rooms.ToListAsync();
            _context.RemoveRange(list);
        }

        public async Task<PagedList<RoomDto>> GetAllRoomAsync(RoomParams roomParams)
        {
            var list = _context.Rooms.AsQueryable();
            //using AutoMapper.QueryableExtensions; list.ProjectTo<RoomDto>
            return await PagedList<RoomDto>.CreateAsync(list.ProjectTo<RoomDto>(_mapper.ConfigurationProvider).AsNoTracking(), roomParams.PageNumber, roomParams.PageSize);
        }

        public async Task UpdateCountMember(Guid roomId, int count)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.CountMember = count;
            }
        }

        public async Task<List<string>> GetRoomsForUser(Guid userId)
        {
            var list = _context.Rooms.AsQueryable();

            return await list.Where(r => r.UserId == userId).AsNoTracking().Select(r => r.RoomId.ToString()).ToListAsync();
        }
    }
}
