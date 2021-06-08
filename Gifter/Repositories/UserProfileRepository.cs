using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gifter.Models;
using Gifter.Utils;
using Gifter.Repositories;
using Microsoft.Extensions.Configuration;

namespace Gifter.Repositories
{
    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        public UserProfileRepository(IConfiguration configuration) : base(configuration) { }

        public List<UserProfile> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT up.Id, up.Name, up.Email, up.DateCreated AS UserProfileDateCreated, 
                               up.ImageUrl AS UserProfileImageUrl, up.Bio
                          FROM UserProfile up 
                      ORDER BY up.DateCreated";

                    var reader = cmd.ExecuteReader();

                    var profiles = new List<UserProfile>();
                    while (reader.Read())
                    {
                        UserProfile profile = new UserProfile()
                        {
                            Id = DbUtils.GetInt(reader, "Id"),
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                        };
                        if (DbUtils.IsDbNull(reader, "UserProfileImageUrl"))
                        {
                            profile.ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl");
                        }
                        if (DbUtils.IsDbNull(reader, "Bio"))
                        {
                            profile.Bio = DbUtils.GetString(reader, "Bio");
                        }
                        profiles.Add(profile);
                    }

                    reader.Close();

                    return profiles;
                }
            }
        }
        public UserProfile GetByFirebaseUserId(string firebaseUserId)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT up.Id, up.FirebaseUserId, up.Name, up.Email, up.Bio, up.DateCreated AS UserProfileDateCreated, 
                               up.ImageUrl AS UserProfileImageUrl
                          FROM UserProfile up 
                         WHERE up.FirebaseUserId = @Id";

                    DbUtils.AddParameter(cmd, "@Id", firebaseUserId);

                    var reader = cmd.ExecuteReader();

                    UserProfile userProfile = null;
                    if (reader.Read())
                    {
                        userProfile = new UserProfile()
                        {
                            Id = DbUtils.GetInt(reader, "Id"),
                            FirebaseUserId = firebaseUserId,
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                        };
                        if (DbUtils.IsDbNull(reader, "UserProfileImageUrl"))
                        {
                            userProfile.ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl");
                        }
                        if (DbUtils.IsDbNull(reader, "Bio"))
                        {
                            userProfile.Bio = DbUtils.GetString(reader, "Bio");
                        }
                    }

                    reader.Close();

                    return userProfile;
                }
            }
        }

        public UserProfile GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT up.Id, up.Name, up.Email, up.Bio, up.DateCreated AS UserProfileDateCreated, 
                               up.ImageUrl AS UserProfileImageUrl
                          FROM UserProfile up 
                         WHERE up.Id = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    var reader = cmd.ExecuteReader();

                    UserProfile userProfile = null;
                    if (reader.Read())
                    {
                        userProfile = new UserProfile()
                        {
                            Id = DbUtils.GetInt(reader, "Id"),
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                        };
                        if (DbUtils.IsDbNull(reader, "UserProfileImageUrl"))
                        {
                            userProfile.ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl");
                        }
                        if (DbUtils.IsDbNull(reader, "Bio"))
                        {
                            userProfile.Bio = DbUtils.GetString(reader, "Bio");
                        }
                    }

                    reader.Close();

                    return userProfile;
                }
            }
        }

        public UserProfile GetUserByIdWithPosts(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                          SELECT up.Id AS UserProfileId, up.Name, up.Bio, up.Email, up.DateCreated AS UserProfileDateCreated,
                                 up.ImageUrl AS UserProfileImageUrl,

                                 p.Id AS PostId, p.Title, p.Caption, p.DateCreated AS PostDateCreated,
                                 p.ImageUrl AS PostImageUrl, p.UserProfileId AS PostUserProfileId

                            FROM UserProfile up
                       LEFT JOIN Post p ON p.UserProfileId = up.Id
                           WHERE UserProfileId = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    var reader = cmd.ExecuteReader();

                    UserProfile profile = null;
                    while (reader.Read())
                    {
                        // Check the posts List to see if this particular post has already been read.
                        var existingProfileId = profile == null ? 0 : profile.Id;

                        // if the existingPost result is null, create a new instance of a Post and fill it with all of the data from this row of the reader.
                        if (existingProfileId == 0)
                        {
                            profile = new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "UserProfileId"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                Posts = new List<Post>(),
                            };
                            if (DbUtils.IsNotDbNull(reader, "UserProfileImageUrl"))
                            {
                                profile.ImageUrl = DbUtils.GetString(reader, "UserProfileImageUrl");
                            }
                            if (DbUtils.IsNotDbNull(reader, "Bio"))
                            {
                                profile.Bio = DbUtils.GetString(reader, "Bio");
                            }
                        }

                        // By default, if the existingPost is NOT NULL, it will not be re-entered into the posts List

                        // At this point, "existingPost" is populated with either A. a new instance of a Post or B. a Post that had been read into "posts" on a previous iteration of the reader
                        // Using the IsNotDbNull method from the DbUtils.cs class, check to see if the column for "CommentId" is null.
                        // If the column for "CommentId" is NOT NULL, create a new instance of a comment and append it to the Comments (List<Comment>) property of the existingPost
                        if (DbUtils.IsNotDbNull(reader, "PostId"))
                        {
                            Post post = new Post()
                            {
                                Id = DbUtils.GetInt(reader, "PostId"),
                                Title = DbUtils.GetString(reader, "Title"),
                                DateCreated = DbUtils.GetDateTime(reader, "PostDateCreated"),
                                ImageUrl = DbUtils.GetString(reader, "PostImageUrl"),
                                UserProfileId = DbUtils.GetInt(reader, "PostUserProfileId"),
                            };
                            if (DbUtils.IsNotDbNull(reader, "Caption"))
                            {
                                post.Caption = DbUtils.GetString(reader, "Caption");
                            }
                            profile.Posts.Add(post);
                        }
                    }

                    reader.Close();

                    return profile;
                }
            }
        }

        public void Add(UserProfile userProfile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    INSERT INTO UserProfile (FirebaseUserId, Name, Email, DateCreated, ImageUrl)
                    OUTPUT INSERTED.ID
                    VALUES (@FirebaseUserId, @Name, @Email, @DateCreated, @ImageUrl)";

                    DbUtils.AddParameter(cmd, "@FirebaseUserId", userProfile.FirebaseUserId);
                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);

                    userProfile.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(UserProfile userProfile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    UPDATE UserProfile
                        SET FirebaseUserId = @FirebaseUserId,
                            Name = @Name,
                            Email = @Email,
                            DateCreated = @DateCreated,
                            ImageUrl = @ImageUrl
                        WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@FirebaseUserId", userProfile.FirebaseUserId);
                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@Id", userProfile.Id);

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
                    cmd.CommandText = "DELETE FROM UserProfile WHERE Id = @Id";
                    DbUtils.AddParameter(cmd, "@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
