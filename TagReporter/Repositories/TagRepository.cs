using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TagReporter.Contracts.Repositories;
using TagReporter.DTOs;
using TagReporter.Models;

namespace TagReporter.Repositories;

public class TagRepository: ITagRepository
{
    private readonly AppDbContext _dbContext;

    public TagRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public List<Tag> FindAll() => _dbContext.Tags.ToList();

    public async Task<List<Tag>> FindAllAsync() => await _dbContext.Tags.ToListAsync();

    public async Task Create(Tag tag)
    {
        if (string.IsNullOrEmpty(tag.Name))
            throw new Exception("[Create] tag.Name is null or empty");
        _dbContext.Add(tag);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(Guid uuid, Tag tag)
    {
        var found = await _dbContext.Tags.Where((t) => t.Uuid == uuid).FirstOrDefaultAsync();
        if (found != null) throw new Exception($"[Update] Tag with uuid - {uuid} exist");
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();
    }

    public Task Delete(Guid uuid, Tag tag)
    {
        throw new NotImplementedException();
    }

    public void RemoveAll() => _dbContext.Tags.RemoveRange(_dbContext.Tags);
    public async Task StoreTagsFromCloud(WstAccount wstAccount)
    {
        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler { CookieContainer = cookieContainer };
        using var client = new HttpClient(handler) { BaseAddress = CommonResources.BaseAddress };
        cookieContainer.Add(CommonResources.BaseAddress, new Cookie("WTAG", wstAccount.SessionId));
        var content = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
        var result = await client.PostAsync("/ethClient.asmx/GetTagList2", content);
        if (result.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Error {result.StatusCode}: {result.RequestMessage}");

        var responseBody = await result.Content.ReadAsStringAsync();

        var jsonResponse = JsonConvert.DeserializeObject<DefaultWstResponse<TagResponse>>(responseBody);
        if (jsonResponse?.D == null || jsonResponse.D.Count == 0)
            return;

        foreach (var tag in jsonResponse.D.Select(tagResponse => new Tag
                 {
                     Uuid = new Guid(tagResponse.uuid ?? string.Empty),
                     Name = tagResponse.name,
                     TagManagerName = tagResponse.managerName,
                     TagManagerMac = tagResponse.mac,
                     Account = wstAccount
                 }))
        {
            var tagFromDb = await _dbContext.Tags.FindAsync(tag.Uuid);
            if (tagFromDb != null)
                continue;

            await Create(tag);
        }
    }
}
