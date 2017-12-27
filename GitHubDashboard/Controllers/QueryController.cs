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

        [HttpGet("[action]/{owner}/{repository}/{milestone}")]
        public async Task<int> CountByMilestone(string owner, string repository, string milestone)
        {
            var issues = await GetIssuesAsync(owner, repository, milestone);
            var count = issues.Where(i => i.PullRequest == null).Count();
            return count;
        }

        [HttpGet("[action]/{owner}/{repository}/{milestone}")]
        public async Task<AssignedChartResult> AssignedChart(string owner, string repository, string milestone)
        {
            var issues = await GetIssuesAsync(owner, repository, milestone);
            var counts = new Dictionary<string, int>();
            foreach (var i in issues)
            {
                var login = i.Assignee?.Login ?? "(blank)";
                if (counts.ContainsKey(login))
                {
                    counts[login]++;
                }
                else
                {
                    counts[login] = 1;
                }
            }

            return new AssignedChartResult
            {
                milestone = milestone,
                assignees = counts.Select(c => new AssigneeCount { assignee = c.Key, count = c.Value, }).ToArray(),
            };
        }

        private async Task<IReadOnlyList<Issue>> GetIssuesAsync(string owner, string repository, string milestone)
        {
            if (string.IsNullOrWhiteSpace(milestone) ||
                milestone == "any")
            {
                milestone = "*";
            }
            else if (milestone != "none")
            {
                var milestonesClient = new MilestonesClient(new ApiConnection(_gitHubClient.Connection));
                var milestoneRequest = new MilestoneRequest { State = ItemStateFilter.Open };
                var milestones = await milestonesClient.GetAllForRepository(owner, repository, milestoneRequest);
                var milestonesByName = milestones.ToDictionary(m => m.Title, m => m.Number);
                milestone = milestonesByName[milestone].ToString();
            }

            var issueRequest = new RepositoryIssueRequest
            {
                Milestone = milestone,
                State = ItemStateFilter.Open,
            };

            var issues = await _gitHubClient.Issue.GetAllForRepository(owner, repository, issueRequest);
            issues = issues.Where(i => i.PullRequest == null).ToList();
            return issues;
        }
    }

    public class AssigneeCount
    {
        public string assignee;
        public int count;
    }

    public class AssignedChartResult
    {
        public string milestone;
        public AssigneeCount[] assignees;
    }
}
