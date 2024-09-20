using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository userRepository;
    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
    {
        var users = await this.userRepository.GetMembersAsync();
        return Ok(users);
    }
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
        var user = await this.userRepository.GetMemberAsync(username);

        if(user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

}
