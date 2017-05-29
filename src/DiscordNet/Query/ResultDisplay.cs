﻿using Discord;
using DiscordNet.Query.Results;
using DiscordNet.Query.Wrappers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordNet.Query
{
    public partial class ResultDisplay
    {
        private SearchResult<object> _result;
        private Cache _cache;
        public ResultDisplay(SearchResult<object> result, Cache cache)
        {
            _result = result;
            _cache = cache;
        }

        public async Task<EmbedBuilder> RunAsync()
        {
            var list = _result.List.GroupBy(x => GetPath(x, false));
            if (list.Count() == 1)
                return await ShowAsync(list.ElementAt(0));
            else
                return await ShowMultipleAsync(list.Select(x => x.First()));
        }

        private async Task<EmbedBuilder> ShowAsync(params object[] o)
        {
            var first = o.First();
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.IconUrl = "http://i.imgur.com/XW4RU5e.png";
            EmbedBuilder eb = new EmbedBuilder().WithAuthor(eab);
            if (first is TypeInfoWrapper)
                eb = await ShowTypesAsync(eb, eab, o.Select(x => (TypeInfoWrapper)x));
            else if (first is MethodInfoWrapper)
                eb = await ShowMethodsAsync(eb, eab, o.Select(x => (MethodInfoWrapper)x));
            else if(first is PropertyInfoWrapper)
                eb = await ShowPropertiesAsync(eb, eab, o.Select(x => (PropertyInfoWrapper)x));
            else if (first is EventInfoWrapper)
                eb = await ShowEventsAsync(eb, eab, o.Select(x => (EventInfoWrapper)x));
            return eb;
        }

        private async Task<EmbedBuilder> ShowMultipleAsync(IEnumerable<object> obj)
        {
            EmbedBuilder eb = new EmbedBuilder();
            var same = obj.GroupBy(x => string.Join("", GetPath(x).Split(' ').Take(2)));
            if (same.Count() == 1)
            {
                eb = await ShowAsync(obj.First());
                eb.Author.Name = $"(Most likely) {eb.Author.Name}";
                var list = obj.Skip(1).Take(3);
                eb.AddField(x =>
                {
                    x.Name = $"Other results found ({list.Count()}/{obj.Count()-1}):";
                    x.Value = string.Join("\n", GetPaths(list));
                    x.IsInline = false;
                });
            }
            else
            {
                if (obj.Count() > 10)
                {
                    eb.Title = $"Too many results, try filtering your search. Some results (10/{obj.Count()}):";
                    eb.Description = string.Join("\n", GetPaths(obj.Take(10)));
                }
                else
                {
                    eb.Title = "Did you mean:";
                    eb.Description = string.Join("\n", GetPaths(obj));
                }
            }
            eb.Footer = new EmbedFooterBuilder().WithText("Type help to see keywords to filter your query.");
            return eb;
        }
    }
}
