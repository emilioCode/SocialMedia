using Microsoft.Extensions.Options;
using SocialMedia.Core.CustomEntities;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.Interfaces;
using SocialMedia.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMedia.Core.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaginationOptions _paginationOptions;

        public PostService(IUnitOfWork unitOfWork, IOptions<PaginationOptions> options)
        {
            _unitOfWork = unitOfWork;
            _paginationOptions = options.Value;
        }

        public PagedList<Post> GetPosts(PostQueryFilter filters)
        {
            filters.PageNumber = filters.PageNumber == 0 ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == 0 ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var posts = _unitOfWork.PostRepository.GetAll();
            if (filters.UserId != null)
            {
                posts = posts.Where(x => x.UserId == filters.UserId);
            }
            if (filters.Description != null)
            {
                posts = posts.Where(x => x.Description.ToLower().Contains(filters.Description.ToLower()));
            }
            if (filters.Date != null)
            {
                posts = posts.Where(x => x.Date.ToShortDateString() == filters.Date?.ToShortDateString());
            }

            var pagedPosts = PagedList<Post>.Create(posts, filters.PageNumber, filters.PageSize);

            return pagedPosts;
        }

        public async Task<Post> GetPost(int id)
        {
            return await _unitOfWork.PostRepository.GetById(id);
        }

        public async Task InsertPost(Post post)
        {
            var user = await _unitOfWork.UserRepository.GetById(post.UserId);
            if(user == null)
            {
                throw new BusinessException("User doesn't exist");
            }
            var userPost = await _unitOfWork.PostRepository.GetPostByUser(post.UserId);
            if (userPost.Count() < 10)
            {
                var lasPost = userPost.Last();
                var totalDays = (DateTime.Now - lasPost.Date).TotalDays;
                if (totalDays < 7)
                {
                    var daystoWait =  7 - userPost.Count();
                    var text = daystoWait + " " + ((daystoWait > 1) ? "days" : "day");
                    throw new BusinessException($"You are not able to publish the post. You'll have to wait {text}");
                }
            }
            if (post.Description.Contains("Sex"))
            {
                throw new BusinessException("Content not allowed");
            }
            post.Date = DateTime.Now;
            await _unitOfWork.PostRepository.Add(post);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UpdatePost(Post post)
        {
            var existingPost = await _unitOfWork.PostRepository.GetById(post.Id);
            if (existingPost == null)
            {
                throw new BusinessException("Post doesn't exist");
            }
            existingPost.Description = post.Description;
            existingPost.Image = post.Image;

            _unitOfWork.PostRepository.Update(existingPost);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePost(int id)
        {
            await _unitOfWork.PostRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
