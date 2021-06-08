using System;
using Microsoft.AspNetCore.Mvc;
using Gifter.Repositories;
using Gifter.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Gifter.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_postRepository.GetAll());
        }

        // api/post/search?q=SOMETHING&sortDesc=TRUEorFALSE
        [HttpGet("search")]
        public IActionResult Search(string q, bool sortDesc)
        {
            return Ok(_postRepository.Search(q, sortDesc));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var post = _postRepository.GetById(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        // Define a Get endpoint that retrieves all of the posts from the database and joins any data from the UserProfile and Comment tables
        [HttpGet("GetWithComments")]
        public IActionResult GetWithComments()
        {
            // Retrieve posts from the database via the GetAllWithComments method from the repository
            var posts = _postRepository.GetAllWithComments();
            // return the list of posts
            return Ok(posts);
        }

        // Define a Get endpoint that retrieves a post from the database based on it's ID and joins any data from the UserProfile and Comment tables
        [HttpGet("GetWithComments/{id}")]
        public IActionResult GetWithComments(int id)
        {
            // Retrieve the post from the database via the GetByIdWithComments method from the repository
            Post post = _postRepository.GetPostByIdWithComments(id);
            // return the post
            return Ok(post);
        }

        [HttpPost]
        public IActionResult Post(Post post)
        {
            post.DateCreated = DateTime.Now;
            post.UserProfileId = 1;
            //post.UserProfileId = GetCurrentUserId();
            _postRepository.Add(post);
            return CreatedAtAction("Get", new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            _postRepository.Update(post);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _postRepository.Delete(id);
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
