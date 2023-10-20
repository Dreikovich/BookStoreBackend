
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Identity;
using WebApiFood.Models;
using WebApiFood.Repositories;
using WebApiFood.Utilities;


namespace WebApiFood.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly BookRepository _bookRepository;
        private readonly ReaderRepository _readerRepository;


        public BookController(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            using var connection = _dbConnectionFactory.CreateConnection();
            _bookRepository = new BookRepository();
            _readerRepository  = new ReaderRepository(_dbConnectionFactory);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var sqlQuery = "SELECT * FROM books";
            var books = new List<Book>(); 
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(new Book
                    {
                        BookId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Description = reader.GetString(3),
                        Rating = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                        Status = reader.GetString(5)
                    });
                }
                connection.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
            return Ok(books);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var sqlQuery = "Select * from books where BookID = @id";
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var book = new Book
                    {
                        BookId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Description = reader.GetString(3),
                        Rating = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                        Status = reader.GetString(5),
                    };
                    connection.Close();
                    return Ok(book);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
            return NotFound();
        }



        [HttpGet(template: "filters", Name = "GetBooksByFilters")]
        public IActionResult Get([FromQuery] Filters filters)
        {
            var sqlQuery = _bookRepository.GetQueryBuFilters(filters.Status ?? "", filters.Author ?? "", filters.Rating?? -1);
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            if (!filters.Rating.Equals(null))
            {
                command.Parameters.AddWithValue("@rating", filters.Rating);
            }
            if (!string.IsNullOrEmpty(filters.Status))
            {
                command.Parameters.AddWithValue("@status", filters.Status);
            }
            if (!string.IsNullOrEmpty(filters.Author))
            {
                command.Parameters.AddWithValue("@author", filters.Author);
            }

            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                var books = new List<Book>();
                while (reader.Read())
                {
                    var book = new Book()
                    {
                        BookId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Description = reader.GetString(3),
                        Rating = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                        Status = reader.GetString(5),
                    };
                    books.Add(book);
                }

                return Ok(books);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpGet(template: "filterBooksByReader", Name = "GetBooksByReaders")]

        public IActionResult Get([FromQuery] string readerName)
        {
            var readerId = _readerRepository.GetReaderIdByName(readerName);
            var sqlQuery = "Select * from books JOIN readers_book On Books.BookId=readers_book.BookId where readers_book.ReaderId = @readerId";
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            command.Parameters.AddWithValue("@readerId", readerId);
            try
            {
                connection.Open();
                var reader = command.ExecuteReader();
                var books = new List<Book>();
                while (reader.Read())
                {
                    var book = new Book()
                    {
                        BookId = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Author = reader.GetString(2),
                        Description = reader.GetString(3),
                        Rating = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                        Status = reader.GetString(5)
                    };
                    books.Add(book);
                }

                return Ok(books);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        [HttpPost]
        public IActionResult AddNewBook([FromBody] Book book)
        {
            var sqlQuery = "INSERT INTO books (Title, Author, Description, Rating, Status) VALUES (@title, @author, "
                + "@description, @rating, @status)";
            using var connection = _dbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sqlQuery, (SqlConnection)connection);
            command.Parameters.AddWithValue("@title", book.Title);
            command.Parameters.AddWithValue("@author", book.Author);
            command.Parameters.AddWithValue("@description", book.Description);
            command.Parameters.AddWithValue("@rating", book.Rating ?? default(decimal));
            command.Parameters.AddWithValue("@status", book.Status??"");
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
            
        }

        [HttpPut("{id}")]

        public IActionResult UpdateBook(int id, [FromBody] Book book)
        {
            try
            {
                using var connection = _dbConnectionFactory.CreateConnection();
                connection.Open();
                var checkExistanceQuery = "Select  COUNT(*) from books where BookId = @id";
                using var checkExistanceCommand = new SqlCommand(checkExistanceQuery, (SqlConnection)connection);
                checkExistanceCommand.Parameters.AddWithValue("@id", id);
                int count = (int)checkExistanceCommand.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound();
                }

                var updateQuery = "UPDATE Books SET Title = @Title, Author = @Author, " +
                                  "Description = @Description, Rating = @Rating, Status = @Status " +
                                  "WHERE BookId = @id";
                using var updateCommand = new SqlCommand(updateQuery, (SqlConnection)connection);
                updateCommand.Parameters.AddWithValue("@id", id);
                updateCommand.Parameters.AddWithValue("@Title", book.Title);
                updateCommand.Parameters.AddWithValue("@Author", book.Author);
                updateCommand.Parameters.AddWithValue("@Description", book.Description);
                updateCommand.Parameters.AddWithValue("@Rating", book.Rating);
                updateCommand.Parameters.AddWithValue("@Status", book.Status);
                updateCommand.ExecuteNonQuery();
                connection.Close();
                return Ok($"Book with ID {id} updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                using var connection = _dbConnectionFactory.CreateConnection();
                connection.Open();
                var checkExistanceQuery = "Select  COUNT(*) from books where BookId = @id";
                using var checkExistanceCommand = new SqlCommand(checkExistanceQuery, (SqlConnection)connection);
                checkExistanceCommand.Parameters.AddWithValue("@id", id);
                int count = (int)checkExistanceCommand.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound();
                }

                var deleteQuery = "DELETE FROM Books WHERE BookId = @id";
                using var deleteCommand = new SqlCommand(deleteQuery, (SqlConnection)connection);
                deleteCommand.Parameters.AddWithValue("@id", id);
                deleteCommand.ExecuteNonQuery();
                connection.Close();
                return Ok($"Book with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"An error occurred while processing the request. {ex.Message}");
            }
        }


    }

    

}