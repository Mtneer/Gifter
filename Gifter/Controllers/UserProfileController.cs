using Gifter.Models;
using Gifter.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Gifter.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileRepository _userProfileRepository;
        public UserProfileController(IUserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        // GET: api/<UserProfileController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_userProfileRepository.GetAll());
        }

        // GET api/<UserProfileController>/5
        [HttpGet("{id}")]
        public IActionResult GetByFirebaseUserId(string firebaseUserId)
        {
            var profile = _userProfileRepository.GetByFirebaseUserId(firebaseUserId);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        // GET api/<UserProfileController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var profile = _userProfileRepository.GetById(id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        // GET api/<UserProfileController>/GetWithPosts/1
        [HttpGet("GetWithPosts/{id}")]
        public IActionResult GetWithPosts(int id)
        {
            var profile = _userProfileRepository.GetUserByIdWithPosts(id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        // POST api/<UserProfileController>
        [HttpPost]
        public IActionResult Post(UserProfile userProfile)
        {
            userProfile.DateCreated = DateTime.Now;
            _userProfileRepository.Add(userProfile);
            return CreatedAtAction("Get", new { id = userProfile.Id }, userProfile);
        }

        // PUT api/<UserProfileController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, UserProfile userProfile)
        {
            if (id != userProfile.Id)
            {
                return BadRequest();
            }

            _userProfileRepository.Update(userProfile);
            return NoContent();
        }

        // DELETE api/<UserProfileController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userProfileRepository.Delete(id);
            return NoContent();
        }

        // Get the current UserProfileId from the Authentication Cookie
        private int GetCurrentUserId()
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(id);
        }
    }
}
