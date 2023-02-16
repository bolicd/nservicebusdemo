using DapperGenericRepository;
using Infrastructure.Model;

namespace Infrastructure.Repository;

public class MainTableRepository : GenericRepository<MainTable>
{
    public MainTableRepository(string connectionString, string tableName) : base(connectionString, tableName)
    {
    }
}