using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;

namespace TagReporter.Repositories;

public class AccountRepository: IAccountRepository
{
    private readonly AppDbContext _dbContext;

    public AccountRepository(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }
    public List<WstAccount> FindAll() => _dbContext.WstAccounts.ToList();

    public async Task<List<WstAccount>> FindAllAsync() => await _dbContext.WstAccounts.ToListAsync();

    public async Task Create(WstAccount account)
    {
        if (string.IsNullOrEmpty(account.Email) || string.IsNullOrEmpty(account.Password)) 
            throw new Exception("Account email or password cannot be empty or null");
        await _dbContext.AddAsync(account);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(string email, WstAccount account)
    {
        if (string.IsNullOrEmpty(account.Email) || string.IsNullOrEmpty(account.Password) || string.IsNullOrEmpty(email)) 
            throw new Exception("Account email or password cannot be empty or null");
        var dbAccount = await _dbContext.WstAccounts.FindAsync(email);
        if (dbAccount == null) throw new Exception($"Account with email = {email} not found");
        dbAccount.Email = account.Email;
        dbAccount.Password = account.Password;
        _dbContext.Update(dbAccount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(string email)
    {
        var account = await _dbContext.WstAccounts.FindAsync(email);
        if (account == null)
            throw new Exception("[Delete] Account not found");
        _dbContext.WstAccounts.Remove(account);
        await _dbContext.SaveChangesAsync();
    }
}