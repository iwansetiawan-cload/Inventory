using E_OneWeb.DataAccess.Data;
using E_OneWeb.DataAccess.Repository.IRepository;
using E_OneWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository
{
    public class ArticleRepository : Repository<Article>, IArticleRepository
    {
        private readonly ApplicationDbContext _db;

        public ArticleRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
            
        public void Update(Article article)
        {
            var objFromDb = _db.Article.FirstOrDefault(s => s.Id == article.Id);
            if (objFromDb != null)
            {
          
                objFromDb.Title = article.Title;
                objFromDb.Content = article.Content;
                objFromDb.Flag = article.Flag;

                _db.SaveChanges();
            }
        }

    }
}
