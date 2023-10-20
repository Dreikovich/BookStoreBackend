using System.Text;

namespace WebApiFood.Repositories
{
    public class BookRepository
    {

        public string BuildQueryByFilers(string status, string author, decimal rating)
        {
            var queryBuilder = new StringBuilder("SELECT * FROM books WHERE 1=1 ");
            if (!string.IsNullOrEmpty(status))
            {
                queryBuilder.Append("AND status=@status ");
            }
            if (!string.IsNullOrEmpty(author))
            {
                queryBuilder.Append("AND author=@author ");
            }
            if (!rating.Equals(-1))
            {
                queryBuilder.Append("AND rating=@rating ");
            }
            return queryBuilder.ToString();
        }
    
        public string GetQueryBuFilters(string status, string author, decimal rating)
        {
            return BuildQueryByFilers(status, author, rating);
        }
    }
}
