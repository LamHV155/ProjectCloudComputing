using System.Threading.Tasks;

namespace CCAPIProject.Repo
{
    public interface IItemRepo
    {    
        Task RemoveItem(string tableName, string hashKey, string rangeKey = null);
    }
}