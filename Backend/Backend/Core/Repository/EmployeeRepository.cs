using Backend.Core.Data;
using Backend.Core.Data.Entities;
using Backend.Core.RepositoryInterface;
using Backend.Core.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Backend.Core.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateNews(NewsModel news)
        {
            await _context.NewsList.AddAsync(news);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<NewsModel>> GetAllNews()
        {
            return await _context.NewsList.Include(item => item.User)
                .Where(item => item.IsDeleted == false).ToListAsync();
        }

        public async Task<List<NewsModel>> GetUsersNews(string userId)
        {
            return await _context.NewsList.Include(item => item.User)
                .Where(item => item.UserId == userId && item.IsDeleted == false).ToListAsync();
        }

        public async Task<NewsModel> GetSingleNews(string newsId)
        {
            return await _context.NewsList.Include(item => item.User)
                .Where(item => item.NewsId == newsId && item.IsDeleted == false).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateNews(NewsModel model)
        {
            var result = await _context.NewsList.Where(x => x.NewsId == model.NewsId)
                                            .AsNoTracking().FirstOrDefaultAsync();

            if (result == null)
                throw new Exception($"No News Found With Id: {model.NewsId}");

            _context.NewsList.Update(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteNews(string newsId)
        {
            var result = await _context.NewsList.FindAsync(newsId);

            if (result == null)
                throw new Exception($"No News Found With Id: {newsId}");

            result.IsActive = false;
            result.IsDeleted = true;

            _context.NewsList.Update(result);
            await _context.SaveChangesAsync();

            return true;
        }



        public async Task<bool> CreateAdvertisements(AdvertisementModel model)
        {
            await _context.AdvertisementList.AddAsync(model);

            model.Images.ForEach(item =>
            {
                item.AdvertisementId = model.AdvertisementId;
            });

            await _context.ImageList.AddRangeAsync(model.Images);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<AdvertisementModel>> GetAllAdvertisements(string? search)
        {
            var result = (from advertisement in _context.AdvertisementList
                          join
                    user in _context.UserList on advertisement.UserId equals user.UserId
                          select (new AdvertisementModel
                          {
                              UserId = advertisement.UserId,
                              AdvertisementId = advertisement.AdvertisementId,
                              CreatedAt = advertisement.CreatedAt,
                              Content = advertisement.Content,
                              IsActive = advertisement.IsActive,
                              IsDeleted = advertisement.IsDeleted,
                              Subject = advertisement.Subject,
                              Images = _context.ImageList
                                          .Where(item => item.AdvertisementId == advertisement.AdvertisementId)
                                          .ToList(),
                              User = _context.UserList.Include(item => item.User)
                                          .Where(item => item.UserId == advertisement.UserId)
                                          .FirstOrDefault()
                          })).Where(item => item.IsDeleted == false).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                return result.Where(item =>
                        item.Subject.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ||
                        item.Content.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            return result;
            //return await _context.AdvertisementList.Include(item => item.User)
            //    .Where(item => item.IsDeleted == false).ToListAsync();
        }

        public async Task<List<AdvertisementModel>> GetUsersAdvertisements(string userId)
        {
            var result = await (from advertisement in _context.AdvertisementList
                                join
                          user in _context.UserList on advertisement.UserId equals user.UserId
                                select (new AdvertisementModel
                                {
                                    UserId = advertisement.UserId,
                                    AdvertisementId = advertisement.AdvertisementId,
                                    CreatedAt = advertisement.CreatedAt,
                                    Content = advertisement.Content,
                                    IsActive = advertisement.IsActive,
                                    IsDeleted = advertisement.IsDeleted,
                                    Subject = advertisement.Subject,
                                    Images = _context.ImageList
                                                .Where(item => item.AdvertisementId == advertisement.AdvertisementId)
                                                .ToList(),
                                    User = _context.UserList.Include(item => item.User)
                                                .Where(item => item.UserId == advertisement.UserId)
                                                .FirstOrDefault()
                                })).Where(item => item.UserId == userId)
                                   .Where(item => item.IsDeleted == false)
                                   .ToListAsync();
            return result;
        }

        public async Task<AdvertisementModel> GetSingleAdvertisement(string advertisementId)
        {
            var result = await (from advertisement in _context.AdvertisementList
                                join
                          user in _context.UserList on advertisement.UserId equals user.UserId
                                select (new AdvertisementModel
                                {
                                    UserId = advertisement.UserId,
                                    AdvertisementId = advertisement.AdvertisementId,
                                    CreatedAt = advertisement.CreatedAt,
                                    Content = advertisement.Content,
                                    IsActive = advertisement.IsActive,
                                    IsDeleted = advertisement.IsDeleted,
                                    Subject = advertisement.Subject,
                                    Images = _context.ImageList
                                                .Where(item => item.AdvertisementId == advertisement.AdvertisementId)
                                                .ToList(),
                                    User = _context.UserList.Include(item => item.User)
                                                .Where(item => item.UserId == advertisement.UserId)
                                                .FirstOrDefault()
                                })).Where(item => item.AdvertisementId == advertisementId)
                                   .FirstOrDefaultAsync();

            if (result == null)
                throw new Exception($"No Advertisement Found With This Id {advertisementId}");

            return result;
        }

        public async Task<bool> UpdateAdvertisements(AdvertisementModel model)
        {
            var result = await _context.AdvertisementList.Where(x => x.AdvertisementId == model.AdvertisementId)
                .AsNoTracking().FirstOrDefaultAsync();

            if (result == null)
                throw new Exception($"No Advertisement Found With Id: {model.AdvertisementId}");


            _context.AdvertisementList.Update(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAdvertisements(string advertisementId)
        {
            var result = await _context.AdvertisementList.FindAsync(advertisementId);

            if (result == null)
                throw new Exception($"No Advertisement Found With Id: {advertisementId}");

            result.IsActive = false;
            result.IsDeleted = true;

            _context.AdvertisementList.Update(result);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<PostModel>> GetAdminPosts()
        {
            var result = (from post in _context.PostList
                          join
                    user in _context.UserList on post.UserId equals user.UserId
                          select (new PostModel
                          {
                              PostId = post.PostId,
                              UserId = post.UserId,
                              Subject = post.Subject,
                              CreatedAt = post.CreatedAt,
                              IsActive = post.IsActive,
                              IsDeleted = post.IsDeleted,
                              Content = post.Content,
                              Image = post.Image,
                              User = _context.UserList
                                          .Where(item => item.UserId == post.UserId)
                                          .FirstOrDefault(),

                              LikeCount = _context.LikeList.Where(item => item.ContentId == post.PostId).Count(),
                          })).Where(item => item.IsDeleted == false).ToList();

            return result;
        }

        public async Task<List<PostModel>> GetAllPosts(string? search, string userId)
        {
            var result = (from post in _context.PostList
                          join
                    user in _context.UserList on post.UserId equals user.UserId
                          select (new PostModel
                          {
                              PostId = post.PostId,
                              UserId = post.UserId,
                              Subject = post.Subject,
                              CreatedAt = post.CreatedAt,
                              IsActive = post.IsActive,
                              IsDeleted = post.IsDeleted,
                              Content = post.Content,
                              Image = post.Image,
                              User = _context.UserList
                                          .Where(item => item.UserId == post.UserId)
                                          .FirstOrDefault(),

                              IsLiked = _context.LikeList.Where(item => item.ContentId == post.PostId
                                             && item.UserId == userId).FirstOrDefault() != null ? true : false,

                              LikeCount = _context.LikeList.Where(item => item.ContentId == post.PostId).Count(),
                          })).Where(item => item.IsActive == true && item.IsDeleted == false).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                return result.Where(item => item.Subject.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                           || item.Content.Contains(search, StringComparison.CurrentCultureIgnoreCase)).ToList();

            }
            return result;
        }

        public async Task<List<PostModel>> GetUsersPosts(string postUser, string userId)
        {
            var result = await (from post in _context.PostList
                                join
                          user in _context.UserList on post.UserId equals user.UserId
                                select (new PostModel
                                {
                                    PostId = post.PostId,
                                    UserId = post.UserId,
                                    Subject = post.Subject,
                                    CreatedAt = post.CreatedAt,
                                    IsActive = post.IsActive,
                                    IsDeleted = post.IsDeleted,
                                    Content = post.Content,
                                    Image = post.Image,
                                    User = _context.UserList
                                                .Where(item => item.UserId == post.UserId)
                                                .FirstOrDefault(),

                                    IsLiked = _context.LikeList.Where(item => item.ContentId == post.PostId
                                             && item.UserId == userId).FirstOrDefault() != null ? true : false,

                                    LikeCount = _context.LikeList.Where(item => item.ContentId == post.PostId).Count(),
                                })).Where(item => item.UserId == postUser && item.IsActive == true && item.IsDeleted == false)
                                   .ToListAsync();
            return result;
        }

        public async Task<PostModel> GetSinglePost(string postId, string userId)
        {
            var result = await (from post in _context.PostList
                                join
                          user in _context.UserList on post.UserId equals user.UserId
                                select (new PostModel
                                {
                                    PostId = post.PostId,
                                    UserId = post.UserId,
                                    Subject = post.Subject,
                                    CreatedAt = post.CreatedAt,
                                    IsActive = post.IsActive,
                                    IsDeleted = post.IsDeleted,
                                    Content = post.Content,
                                    Image = post.Image,
                                    User = _context.UserList
                                                .Where(item => item.UserId == post.UserId)
                                                .FirstOrDefault(),

                                    IsLiked = _context.LikeList.Where(item => item.ContentId == post.PostId
                                             && item.UserId == userId).FirstOrDefault() != null ? true : false,

                                    LikeCount = _context.LikeList.Where(item => item.ContentId == post.PostId).Count(),
                                })).Where(item => item.PostId == postId && item.IsActive == true && item.IsDeleted == false)
                                      .FirstOrDefaultAsync();
            return result;
        }

        public async Task<bool> CreatePost(PostModel model)
        {
            await _context.PostList.AddAsync(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePost(PostModel model)
        {
            var result = await _context.PostList.Where(x => x.PostId == model.PostId)
                                .AsNoTracking().FirstOrDefaultAsync();

            if (result == null)
                throw new Exception($"No Post Found With Id: {model.PostId}");

            _context.PostList.Update(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApprovePost(string postId)
        {
            var result = await _context.PostList.FindAsync(postId);

            if (result == null)
                throw new Exception("$No Post Found With Id: {postId}");
            result.IsActive = true;

            _context.PostList.Update(result);
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<bool> DeletePost(string postId)
        {
            var result = await _context.PostList.FindAsync(postId);

            if (result == null)
                throw new Exception($"No Post Found With Id: {postId}");

            result.IsActive = false;
            result.IsDeleted = true;

            _context.PostList.Update(result);

            var comments = await _context.CommentList.ToListAsync();
            _context.CommentList.RemoveRange(comments);

            await _context.SaveChangesAsync();

            return true;

        }

        //public async Task<List<CommentModel>> GetAllComments(string postId)
        //{
        //    return await _context.CommentList.Where(item => item.PostId == postId).ToListAsync();
        //}
        public async Task<List<CommentModel>> GetAllComments(string postId)
        {
            var result = await _context.CommentList.Include(item => item.User)
                .Where(item => item.PostId == postId)
                .ToListAsync();

            if (result == null)
                throw new Exception($"No Post Found With Id: {postId}");

            return result;

        }


        public async Task<bool> CreateComment(CommentModel model)
        {
            await _context.CommentList.AddAsync(model);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteComment(int commentId)
        {
            var result = await _context.CommentList.FindAsync(commentId);

            if (result == null)
                throw new Exception($"No Comment Found With Id: {commentId}");

            _context.CommentList.Remove(result);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HandleLikes(string postId, string userId)
        {
            var result = await _context.LikeList.Where(item => item.ContentId == postId
                                && item.UserId == userId).FirstOrDefaultAsync();
            if (result == null)
            {
                var res = new LikeModel()
                {
                    ContentId = postId,
                    UserId = userId
                };

                await _context.LikeList.AddAsync(res);
            }
            else
            {
                _context.LikeList.Remove(result);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DashbordViewModel> GetDashboard()
        {
            //var result = await _context.PostList
            //    .GroupBy(item => item.IsActive).Select(item => item.Count()).ToListAsync();

            DashbordViewModel model = new DashbordViewModel()
            {
                ApprovedItemsCount = await _context.PostList.Where(item => item.IsActive == true).CountAsync(),
                PendingItemsCount = await _context.PostList.Where(item => item.IsActive == false && item.IsDeleted == false).CountAsync(),
                RejectedItemsCount=await _context.PostList.Where(item=>item.IsDeleted==true).CountAsync()
            };
            return model;
        }
    }
}
