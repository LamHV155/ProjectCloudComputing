using System.Threading.Tasks;
using CCAPIProject.Dtos;

namespace CCAPIProject.Repo
{
    public interface ITableRepo
    {
        Task<Models.Table[]> GetTablesAsync();
        Task<bool> CreateTableAsync(CreateTableDto tableDto);

        Task<bool> RemoveTableAsync(string tableName);
    }
}