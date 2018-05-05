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
            var issues = await GetIssuesAsync(owner, repository, milestone, labels);
            var count = issues.Where(i => i.PullRequest == null).Count();
            return count;
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

            return new AssignedChartResult(
                milestone: milestone,
                assignees: counts.Select(c => new AssigneeCount(assignee: c.Key, count: c.Value)).OrderByDescending(a => a.count).ToArray()
            );
        }


        // The incoming param values come (ultimately) come from parsing the incoming URL in QueryCountComponent in
        // count.component.ts.  The URLs could include milestones and/or labels.  Here, we have to translate the values
        // to GitHub/Octokit to get the desired result set.  There are some quirks that need clarification below...
        private async Task<IReadOnlyList<Issue>> GetIssuesAsync(string owner, string repository, string milestone, string labels)
        {
            // First, for milestone.  The URL handled by the Angular app might not have a milestone query parameter so it
            // would look something like this:
            //    https://<host>/count/nuget/home?label=VS1ES
            // In that case, the angular app will set the milestone to "undefined" before calling this service, which will
            // receive a URL like:
            //    https://<host>/api/CountByMilestone/nuget/home/undefined/VS1ES
            // Map "undefined" and null/whitespace to `null` in the Octokit issue request.  This tells Octokit "don't
            // consider milestones in this query."  Then Octokit returns issues regardless of their milestone setting -
            // including issues with _no_ milestone setting.  The URL could have "milestone='*'" - the milestone parameter
            // will pass that value.  GitHub/Octokit treats milestone = '*' as "any _set_ milestone."  So Octokit would
            // return all issues that have any milestone setting of any value - as long as the issue has one is set.
            // However, with '*' it won't return issues that have NO milestone setting.  Finally, if the URL includes a
            // valid milestone (eg "milestone=15.8"), translate that string value into the corresponding milestone ID and
            // put that in the issue request...
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
                var labelvalues = labels.Split(',');
                foreach (var label in labelvalues)
                {
                    issueRequest.Labels.Add(label);
                }
            }

            var issues = await _gitHubClient.Issue.GetAllForRepository(owner, repository, issueRequest);
            issues = issues.Where(i => i.PullRequest == null).ToList();
            return issues;
        }
    }

    // Disable warnings about naming since these are designed to be used in json.
#pragma warning disable IDE1006
    public class AssigneeCount
    {
        public AssigneeCount(string assignee, int count)
        {
            this.assignee = assignee;
            this.count = count;
        }

        public readonly string assignee;
        public readonly int count;
    }


    public class AssignedChartResult
    {
        public AssignedChartResult(string milestone, AssigneeCount[] assignees)
        {
            this.milestone = milestone;
            this.assignees = assignees;
        }

        public string milestone;
        public AssigneeCount[] assignees;
    }
#pragma warning restore

}
