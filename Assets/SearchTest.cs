using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

public static class SearchTest {
    // Example search for an object's guid and returns all references to that guid. Works in the search window.
    private const string TestSearch = "ref={aef7d7dc5c9ed5748b3d4aa3d923ab69, @path}";

    [MenuItem("Examples/SearchTest Request")]
    public static void RunRequest() {
        // Search never completes unless a search window has been opened.
        ISearchList searchList = SearchService.Request(TestSearch);

        List<SearchItem> results = null;
        bool success = RunWithTimeout(() => {
            results = searchList.Fetch().ToList();
        });

        LogResults(success, results);
    }

    [MenuItem("Examples/SearchTest Request Broken")]
    public static void RunRequestBroken() {
        // This search never completes even if a search window has been opened.
        List<SearchItem> results = SearchService.Request(TestSearch).Fetch().ToList();
        string resultText = results.Select(r => r.GetDescription(r.context)).AggregateToString("\n");
        Debug.Log($"Found {results.Count} items:\n{resultText}");
    }

    [MenuItem("Examples/SearchTest GetItems")]
    public static void RunGetItems() {
        // Search never completes unless a search window has been opened.

        using SearchContext searchContext = SearchService.CreateContext("asset", TestSearch);
        List<SearchItem> results = SearchService.GetItems(searchContext);
        searchContext.asyncItemReceived += (_, incomingItems) => results.AddRange(incomingItems); 

        bool success = RunWithTimeout(() => {
            while (searchContext.searchInProgress) { }
        });

        LogResults(success, results);
    }

    [MenuItem("Examples/SearchTest GetItems Broken")]
    public static void RunGetItemsBroken() {
        using SearchContext searchContext = SearchService.CreateContext("asset", TestSearch);
        List<SearchItem> results = SearchService.GetItems(searchContext, SearchFlags.Synchronous);
        string resultText = results.Select(r => r.GetDescription(r.context)).AggregateToString("\n");
        Debug.Log($"Found {results.Count} items:\n{resultText}");
    }

    private static void LogResults(bool success, List<SearchItem> results) {
        if (success) {
            string resultText = results.Select(r => r.GetDescription(r.context)).AggregateToString("\n");
            Debug.Log($"Found {results.Count} items:\n{resultText}");
        } else {
            Debug.LogWarning($"Couldn't finish search for \"{TestSearch}\".");
        }
    }

    private static bool RunWithTimeout(Action action, float seconds = 3) {
        Task task = Task.Run(action);
        try {
            bool success = task.Wait(TimeSpan.FromSeconds(seconds));
            if (!success) {
                throw new TimeoutException();
            }

            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    private static string AggregateToString<T>(this IEnumerable<T> strings, string sep = ", ", Func<T, string> converter = null, string prepend = "", string append = "") {
        if (converter == null) {
            converter = v => v.ToString();
        }
        return strings.Aggregate(new StringBuilder(prepend), (c, n) => {
            if (c.Length > prepend.Length) {
                return c.Append(sep).Append(converter(n));
            }
            return c.Append(converter(n));
        }).Append(append).ToString();
    }
}
