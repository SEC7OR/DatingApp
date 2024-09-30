using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext context;
    private readonly IMapper mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    public async Task<MemberDTO?> GetMemberAsync(string username)
    {
        return await context.Users
        .Where(u=>u.UserName == username)
        .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();

        query = query.Where(u => u.UserName != userParams.CurrentUsername);

        if(userParams.Gender != null)
        {
            query = query.Where(u => u.Gender == userParams.Gender);    
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge-1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u=>u.Created),
            _ => query.OrderByDescending(u=>u.LastActive)
        };

        return await PagedList<MemberDTO>.CreateAsync(query.ProjectTo<MemberDTO>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await context.Users
        .Include(u=>u.Photos)
        .SingleOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users
        .Include(u => u.Photos)
        .ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
