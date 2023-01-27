using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

internal static class ExampleSearchService {
    // [MenuItem("Examples/Search Test Basic")]
    public static void Run() {
        // Create a container to hold found items.
        List<SearchItem> results = new List<SearchItem>();

        // Create the search context that will be used to execute the query.
        using (SearchContext searchContext = SearchService.CreateContext("asset", "ref={aef7d7dc5c9ed5748b3d4aa3d923ab69, @path}")) {
            // Set up a callback that will be used gather additional asynchronous results.
            searchContext.asyncItemReceived += (context, incomingItems) => results.AddRange(incomingItems);

            // Initiate the query and get the first results.
            results.AddRange(SearchService.GetItems(searchContext, SearchFlags.Synchronous));

            // ***IMPORTANT***: Wait for the search to finish. Note that often times, a search
            // provider will need to be ticked by EditorApplication to yield new search items. Unity doesn't recommends
            // to do an active wait on the main thread to process search results.
            while (searchContext.searchInProgress) {
                ;
            }

            // Print results
            foreach (SearchItem searchItem in results) {
                Debug.Log(searchItem.GetDescription(searchContext));
            }
        }
    }
}
