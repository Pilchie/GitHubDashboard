using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace GitHubDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/Query")]
    public class QueryController : Controller
    {
        private readonly GitHubClient _gitHubClient;

        public QueryController(GitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        [HttpGet("[action]")]
        public async Task<int> Count()
        {
            var milestonesClient = new MilestonesClient(new ApiConnection(_gitHubClient.Connection));
            var milestoneRequest = new MilestoneRequest { State = ItemStateFilter.Open };
            var milestones = await milestonesClient.GetAllForRepository("dotnet", "project-system", milestoneRequest);
            var milestonesByName = milestones.ToDictionary(m => m.Title, m => m.Number);

            var issueRequest = new RepositoryIssueRequest
            {
                Milestone = milestonesByName["15.6"].ToString(),
                State = ItemStateFilter.Open,
            };

            var issues = await _gitHubClient.Issue.GetAllForRepository("dotnet", "project-system", issueRequest);
            var count = issues.Where(i => i.PullRequest == null).Count();
            return count;
        }
    }
}
