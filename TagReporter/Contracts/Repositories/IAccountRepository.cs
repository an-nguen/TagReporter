using System.Collections.Generic;
using System.Threading.Tasks;
using TagReporter.Models;

namespace TagReporter.Contracts.Repositories;

public interface IAccountRepository
{
    public List<WstAccount> FindAll();
    public Task<List<WstAccount>> FindAllAsync();

    public Task Create(WstAccount account);
    public Task Update(string email, WstAccount account);

    public Task Delete(string email);
}