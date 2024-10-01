using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> userManager;
    private readonly ITokenService tokenService;
    private readonly IMapper mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        this.userManager = userManager;
        this.tokenService = tokenService;
        this.mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
    {
        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken");
        }



        var user = mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var userDto = new UserDTO
        {
            Username = user.UserName,
            Token = await tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };

        return Ok(userDto);

    }
    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        var user = await userManager.Users
            .Include(u => u.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == loginDto.Username.ToUpper());
        if (user == null || user.UserName == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if(!result)
        {
            System.Console.WriteLine(user.PasswordHash);
            return Unauthorized();    
        }

        var userDto = new UserDTO
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = await tokenService.CreateToken(user),
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url
        };

        return Ok(userDto);
    }

    private async Task<bool> UserExists(string username)
    {
        return await this.userManager.Users.AnyAsync(u => u.NormalizedUserName == username.ToUpper());
    }
}
