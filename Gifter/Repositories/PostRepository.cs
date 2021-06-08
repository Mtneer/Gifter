using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Gifter.Models;
using Gifter.Utils;

namespace Gifter.Repositories
{
    public class PostRepository : BaseRepository, IPostRepository
    {
        public PostRepository(IConfiguration configuration) : base(configuration) { }

        public List<Post> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated, 
                                 p.ImageUrl AS PostImageUrl, p.UserProfileId,

                                 up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated, 
                                 up.ImageUrl AS UserProfileImageUrl
                            FROM Post p 
                       LEFT JOIN UserProfile up ON p.UserProfileId = up.id
                        ORDER BY p.DateCreated";

                    var reader = cmd.ExecuteReader();

                    var posts = new List<Post>();
                    while (reader.Read())
                    {
                        posts.Add(new Post()
                        {
                            Id = DbUtils.GetInt(reader, "PostId"),
                            Title = DbUtils.GetString(reader, "Title"),
                            Caption = DbUtils.GetString(reader, "Caption"),
                            DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                            ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                            UserProfileId = DbUtils.GetInt(reader, "UserProfileId"),
                            UserProfile = new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "UserProfileId"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                            },
                        });
                    }

                    reader.Close();

                    return posts;
                }
            }
        }

        // Define a method that retrieves all of the Posts from the database, along with any joined data from the Comment and UserProfile tables
        public List<Post> GetAllWithComments()
        {
            // Instantiate a new SqlConnection called Connection
            using (var conn = Connection)
            {
                // Open the connection to the database
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Set the CommandText to the SQL query syntax required to retrieve the data
                    cmd.CommandText = @"
                                        SELECT p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated,
                                               p.ImageUrl AS PostImageUrl, p.UserProfileId AS PostUserProfileId,

                                               up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated,
                                               up.ImageUrl AS UserProfileImageUrl,

                                               c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                                          FROM Post p
                                               LEFT JOIN UserProfile up ON p.UserProfileId = up.id
                                               LEFT JOIN Comment c on c.PostId = p.id
                                      ORDER BY p.DateCreated";

                    // Execute the query and label the result "reader"
                    var reader = cmd.ExecuteReader();

                    // Initiate a new List of Posts, titled posts
                    var posts = new List<Post>();

                    // while there are rows of readable data in the reader...
                    while (reader.Read())
                    {
                        // Using the GetInt method defined the DbUtils.cs class, find the postId associated with the current row of data in the reader
                        var postId = DbUtils.GetInt(reader, "PostId");

                        // Check the posts List to see if this particular post has already been read.
                        var existingPost = posts.FirstOrDefault(p => p.Id == postId);

                        // if the existingPost result is null, create a new instance of a Post and fill it with all of the data from this row of the reader.
                        if (existingPost == null)
                        {
                            existingPost = new Post()
                            {
                                Id = postId,
                                Title = DbUtils.GetString(reader, "Title"),
                                Caption = DbUtils.GetString(reader, "Caption"),
                                DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                                ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                                UserProfileId = DbUtils.GetInt(reader, "PostUserProfileId"),
                                UserProfile = new UserProfile()
                                {
                                    Id = DbUtils.GetInt(reader, "PostUserProfileId"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                },
                                Comments = new List<Comment>()
                            };

                            posts.Add(existingPost);
                        }

                        // By default, if the existingPost is NOT NULL, it will not be re-entered into the posts List

                        // At this point, "existingPost" is populated with either A. a new instance of a Post or B. a Post that had been read into "posts" on a previous iteration of the reader
                        // Using the IsNotDbNull method from the DbUtils.cs class, check to see if the column for "CommentId" is null.
                        // If the column for "CommentId" is NOT NULL, create a new instance of a comment and append it to the Comments (List<Comment>) property of the existingPost
                        if (DbUtils.IsNotDbNull(reader, "CommentId"))
                        {
                            existingPost.Comments.Add(new Comment()
                            {
                                Id = DbUtils.GetInt(reader, "CommentId"),
                                Message = DbUtils.GetString(reader, "Message"),
                                PostId = postId,
                                UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                            });
                        }
                    }

                    // Once each row of the reader (the returned data from the DB) has been read, close the connection to the database
                    reader.Close();

                    // return the list of posts
                    return posts;
                }
            }
        }

        public List<Post> Search(string criterion, bool sortDescending)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var sql =
                            @"SELECT p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated, 
                                p.ImageUrl AS PostImageUrl, p.UserProfileId,

                                up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated, 
                                up.ImageUrl AS UserProfileImageUrl,

                                c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                           FROM Post p 
                      LEFT JOIN UserProfile up ON p.UserProfileId = up.id
                      LEFT JOIN Comment c on c.PostId = p.id
                          WHERE p.Title LIKE @Criterion OR p.Caption LIKE @Criterion";

                    if (sortDescending)
                    {
                        sql += " ORDER BY p.DateCreated DESC";
                    }
                    else
                    {
                        sql += " ORDER BY p.DateCreated";
                    }

                    cmd.CommandText = sql;
                    DbUtils.AddParameter(cmd, "@Criterion", $"%{criterion}%");
                    var reader = cmd.ExecuteReader();

                    var posts = new List<Post>();
                    
                    while (reader.Read())
                    {
                        // Using the GetInt method defined the DbUtils.cs class, find the postId associated with the current row of data in the reader
                        var postId = DbUtils.GetInt(reader, "PostId");

                        // Check the posts List to see if this particular post has already been read.
                        var existingPost = posts.FirstOrDefault(p => p.Id == postId);

                        // if the existingPost result is null, create a new instance of a Post and fill it with all of the data from this row of the reader.
                        if (existingPost == null)
                        {
                            existingPost = new Post()
                            {
                                Id = postId,
                                Title = DbUtils.GetString(reader, "Title"),
                                Caption = DbUtils.GetString(reader, "Caption"),
                                DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                                ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                                UserProfileId = DbUtils.GetInt(reader, "UserProfileId"),
                                UserProfile = new UserProfile()
                                {
                                    Id = DbUtils.GetInt(reader, "UserProfileId"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                },
                                Comments = new List<Comment>()
                            };

                            posts.Add(existingPost);
                        }

                        // By default, if the existingPost is NOT NULL, it will not be re-entered into the posts List

                        // At this point, "existingPost" is populated with either A. a new instance of a Post or B. a Post that had been read into "posts" on a previous iteration of the reader
                        // Using the IsNotDbNull method from the DbUtils.cs class, check to see if the column for "CommentId" is null.
                        // If the column for "CommentId" is NOT NULL, create a new instance of a comment and append it to the Comments (List<Comment>) property of the existingPost
                        if (DbUtils.IsNotDbNull(reader, "CommentId"))
                        {
                            existingPost.Comments.Add(new Comment()
                            {
                                Id = DbUtils.GetInt(reader, "CommentId"),
                                Message = DbUtils.GetString(reader, "Message"),
                                PostId = postId,
                                UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                            });
                        } 
                        //else if (DbUtils.IsDbNull(reader, "CommentId"))
                        //    {
                        //        //existingPost.Comments
                        //    }
                    }

                    reader.Close();

                    return posts;
                }
            }
        }

        public Post GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated, 
                                 p.ImageUrl AS PostImageUrl, p.UserProfileId,

                                 up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated, 
                                 up.ImageUrl AS UserProfileImageUrl
                            FROM Post p 
                       LEFT JOIN UserProfile up ON p.UserProfileId = up.id
                           WHERE PostId = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    var reader = cmd.ExecuteReader();

                    Post post = null;
                    if (reader.Read())
                    {
                        post = new Post()
                        {
                            Id = id,
                            Title = DbUtils.GetString(reader, "Title"),
                            Caption = DbUtils.GetString(reader, "Caption"),
                            DateCreated = DbUtils.GetDateTime(reader, "DateCreated"),
                            ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                            UserProfileId = DbUtils.GetInt(reader, "UserProfileId"),
                        };
                    }

                    reader.Close();

                    return post;
                }
            }
        }

        public Post GetPostByIdWithComments(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated,
                                 p.ImageUrl AS PostImageUrl, p.UserProfileId AS PostUserProfileId,

                                 up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated,
                                 up.ImageUrl AS UserProfileImageUrl,

                                 c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                            FROM Post p
                       LEFT JOIN UserProfile up ON p.UserProfileId = up.id
                       LEFT JOIN Comment c on c.PostId = p.id
                           WHERE PostId = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    var reader = cmd.ExecuteReader();

                    Post post = null;
                    while (reader.Read())
                    {
                        // Check the posts List to see if this particular post has already been read.
                        var existingPostId = post == null ? 0 : post.Id;

                        // if the existingPost result is null, create a new instance of a Post and fill it with all of the data from this row of the reader.
                        if (existingPostId == 0)
                        {
                            post = new Post()
                            {
                                Id = DbUtils.GetInt(reader, "PostId"),
                                Title = DbUtils.GetString(reader, "Title"),
                                Caption = DbUtils.GetString(reader, "Caption"),
                                DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                                ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                                UserProfileId = DbUtils.GetInt(reader, "PostUserProfileId"),
                                UserProfile = new UserProfile()
                                {
                                    Id = DbUtils.GetInt(reader, "PostUserProfileId"),
                                    Name = DbUtils.GetString(reader, "Name"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                    ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl"),
                                },
                                Comments = new List<Comment>()
                            };
                        }

                        // By default, if the existingPost is NOT NULL, it will not be re-entered into the posts List

                        // At this point, "existingPost" is populated with either A. a new instance of a Post or B. a Post that had been read into "posts" on a previous iteration of the reader
                        // Using the IsNotDbNull method from the DbUtils.cs class, check to see if the column for "CommentId" is null.
                        // If the column for "CommentId" is NOT NULL, create a new instance of a comment and append it to the Comments (List<Comment>) property of the existingPost
                        if (DbUtils.IsNotDbNull(reader, "CommentId"))
                        {
                            post.Comments.Add(new Comment()
                            {
                                Id = DbUtils.GetInt(reader, "CommentId"),
                                Message = DbUtils.GetString(reader, "Message"),
                                PostId = DbUtils.GetInt(reader, "PostId"),
                                UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                            });
                        }
                    }

                    reader.Close();

                    return post;
                }
            }
        }

        public void Add(Post post)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Post (Title, Caption, DateCreated, ImageUrl, UserProfileId)
                        OUTPUT INSERTED.ID
                        VALUES (@Title, @Caption, @DateCreated, @ImageUrl, @UserProfileId)";

                    DbUtils.AddParameter(cmd, "@Title", post.Title);
                    DbUtils.AddParameter(cmd, "@Caption", post.Caption);
                    DbUtils.AddParameter(cmd, "@DateCreated", post.DateCreated);
                    DbUtils.AddParameter(cmd, "@ImageUrl", post.ImageUrl);
                    DbUtils.AddParameter(cmd, "@UserProfileId", post.UserProfileId);

                    post.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(Post post)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE Post
                           SET Title = @Title,
                               Caption = @Caption,
                               DateCreated = @DateCreated,
                               ImageUrl = @ImageUrl,
                               UserProfileId = @UserProfileId
                         WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Title", post.Title);
                    DbUtils.AddParameter(cmd, "@Caption", post.Caption);
                    DbUtils.AddParameter(cmd, "@DateCreated", post.DateCreated);
                    DbUtils.AddParameter(cmd, "@ImageUrl", post.ImageUrl);
                    DbUtils.AddParameter(cmd, "@UserProfileId", post.UserProfileId);
                    DbUtils.AddParameter(cmd, "@Id", post.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Post WHERE Id = @Id";
                    DbUtils.AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
