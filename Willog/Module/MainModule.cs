﻿using System;
using System.Collections.Generic;
using System.Configuration;
using MarkdownSharp;
using Nancy;
using Willog.Models;
using Willog.Services;

namespace Willog.Module
{
    public class MainModule : WillogModule
    {
        public MainModule(IDBFactory dbFactory) : base(dbFactory)
        {
            Get["/"] = _ =>
            {
                int take = Convert.ToInt32(ConfigurationManager.AppSettings["PostNum"]);
                var postList = GetPostList(1,take);

                var maxPage = DB.Post.All().ToList().Count / take;
                dynamic model = new
                {
                    postList = (dynamic) postList,
                    currentPage =(dynamic) 1,
                    hasNext = maxPage > 1,
                    hasPrevious = false,
                    maxPage = maxPage
                };

                return View["Home", model];
            };

            Get[@"/page/(?<id>[\d]{1,2})"] = _ =>
            {
                int take = Convert.ToInt32(ConfigurationManager.AppSettings["PostNum"]);
                int page = (int) _.id;

                var postList = GetPostList(page, take);
                var maxPage = DB.Post.All().ToList().Count / take;

                dynamic model = new
                {
                    postList,
                    currentPage = _.id,
                    hasNext = page < maxPage,
                    hasPrevious = page > 1,
                    maxPage = maxPage
                };

                return View["Home", model];
            };

            Get["/post/{slug}"] = _ =>
            {
                var model = (Post)DB.Post.FindBySlug(_.slug);
                model.Content = new Markdown().Transform(model.Content);
                return View["Post", model];
            };


            Get[@"/(?<page>[a-z]+)"] = _ =>
            {
                var model = DB.Post.FindBySlugAndType(_.page,"page");
                model.Content = new Markdown().Transform(model.Content);
                return View["Page", model];
            };
        }
        
        /// <summary>
        /// Get Post List
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<Post> GetPostList(int page,int take)
        {
            int skip = (page - 1) * take;

            
            var postList = DB.Post.FindAll(DB.Post.Type == "post").OrderByCreatedDescending().Skip(skip).Take(take); ;
            postList = postList.ToList<Post>();
            var markdown = new Markdown();
            foreach (var post in postList)
            {
                post.Content = markdown.Transform(post.Content);
            }

            return postList;
        }
    }
}