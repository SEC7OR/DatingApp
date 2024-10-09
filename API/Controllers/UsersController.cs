using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IPhotoService photoService;
    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.photoService = photoService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await this.unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }
    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
        var user = await this.unitOfWork.UserRepository.GetMemberAsync(username);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {

        var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        mapper.Map(memberUpdateDTO, user);

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }
        return BadRequest("Failed to update the user");
    }
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("Cannot update user");
        }

        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null)
        {
            return BadRequest(result.Error.Message);
        }

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);
        if (await unitOfWork.Complete())
        {
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDTO>(photo));
        }

        return BadRequest("Problem adding photo");

    }
    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null)
        {
            return BadRequest("Could not find user");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain)
        {
            return BadRequest("Cannot use this as main photo");
        }

        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
        if (currentMain != null)
        {
            currentMain.IsMain = false;
        }
        photo.IsMain = true;

        if (await unitOfWork.Complete())
        {
            return NoContent();
        }

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null)
        {
            return BadRequest("User not found");
        }

        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

        if (photo == null || photo.IsMain)
        {
            return BadRequest("This photo cannot be deleted");
        }

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
        }
        user.Photos.Remove(photo);

        if (await unitOfWork.Complete())
        {
            return Ok();
        }

        return BadRequest("Problem deleting photo");
    }

}
