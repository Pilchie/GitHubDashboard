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

        [HttpGet("[action]/{owner}/{repository}/{milestone}/{labels}")]
        public async Task<int> CountByMilestone(string owner, string repository, string milestone, string labels)
        {
            try
            {
                // GetIssuesAsync will throw if the milestone doesn't exist in the repo's list of milestones.
                // Just return 0 issues.  Doesn't tell the user that the milestone doesn't exist (most helpful),
                // but returning 0 issues matches GitHub site's behavior.  Returning 0 also renders a good URL
                // that the user can click to go to the site for further debugging of the query...
                var issues = await GetIssuesAsync(owner, repository, milestone, labels);
                var count = issues.Where(i => i.PullRequest == null).Count();
                return count;
            }
            catch
            {
                return 0;
            }
        }

        [HttpGet("[action]/{owner}/{repository}/{milestone}/{labels}")]
        public async Task<AssignedChartResult> AssignedChart(string owner, string repository, string milestone, string labels)
        {
            var issues = await GetIssuesAsync(owner, repository, milestone, labels);
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
                assignees = counts.Select(c => new AssigneeCount { assignee = c.Key, count = c.Value, }).OrderByDescending(a => a.count).ToArray(),
            };
        }


        // The incoming param values come (ultimately) come from parsing the incoming URL in QueryCountComponent in
        // count.component.ts.  The URLs could include milestones and/or labels.  Here, we have to translate the values
        // to GitHub/Octokit to get the desired result set.  There are some quirks that need clarification below...
        private async Task<IReadOnlyList<Issue>> GetIssuesAsync(string owner, string repository, string milestone, string labels)
        {
            // First, for milestone.  The URL might not have an milestone query parameter.  Something like this:
            //    http://<host>/count/nuget/home?label=VS1ES
            // In that case, the milestone parmeter will set to "undefined".  Map that value (and null/whitespace) to
            // NULL in the Octokit issue request.  This tells Octokit "don't consider milestones in this query."  Then
            // Octokit returns issues regardless of thier milestone setting - including issues with _no_ milestone setting.
            // The URL could have "milestone='*'" - the milestone parameter will pass that value.  GitHub/Octokit treats
            // milestone = '*' as "any _set_ milestone."  So Octokit would return all issues that have any milestone setting
            // of any value - as long as the issue has one is set.  However, with '*' it won't return issues that have NO
            // milestone setting.  Finally, if the URL includes a valid milestone (eg "milestone=15.8"), translate that string
            // value into the corresponding milstone ID and put that in the issue request...
            if (string.IsNullOrWhiteSpace(milestone) || milestone == "undefined")
            {
                milestone = null;
            }
            else if (milestone == "any" || milestone == "*")
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

            // Second, for labels.  In GitHub, issues can have zero or more label values, and the incoming URL could specify a 
            // query for multiple values.  Those URL values are passed to this function as a string of comma separated values.
            // No values in the URL results in labels param value of "undefined" (same as above for milestone); A URL value of
            // "label=test&label=VS1ES" results in "test,VS1ES" --> split those and add each value to the issue request
            // Labels collection...
            if (!string.IsNullOrWhiteSpace(labels) && !(labels == "undefined")) {
                string[] labelvalues = labels.Split(',');
                foreach (var label in labelvalues)
                {
                    issueRequest.Labels.Add(label);
                }
            }

            // This could throw a validation error if the milestone doesn't exist in the repo.
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
