using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController:BaseApiController
{
    private readonly DataContext context;
    private readonly ITokenService tokenService;

    public AccountController(DataContext context, ITokenService tokenService)
    {
        this.context = context;
        this.tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
    {
        if(await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken");    
        }
        return Ok();
        // using var hmac = new HMACSHA512();

        // var user = new AppUser
        // {
        //     UserName = registerDto.Username.ToLower(),
        //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        //     PasswordSalt = hmac.Key
        // };
        // this.context.Users.Add(user);
        // await this.context.SaveChangesAsync();

        // var userDto = new UserDTO
        // {
        //     Username = user.UserName,
        //     Token = tokenService.CreateToken(user)
        // };

        // return Ok(userDto);

    }
    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        var user = await this.context.Users.FirstOrDefaultAsync(u=>u.UserName == loginDto.Username.ToLower());
        if(user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for(int i = 0;i <computedHash.Length;i++)
        {
            if(computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid username or password");
            }
        }
        var userDto = new UserDTO
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };

        return Ok(userDto);
    }

    private async Task<bool> UserExists(string username){
        return await this.context.Users.AnyAsync(u=>u.UserName.ToLower() == username.ToLower());
    }
}
