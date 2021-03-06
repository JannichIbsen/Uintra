﻿using System;
using System.Collections.Generic;
using System.Linq;
using Compent.Extensions;
using Nest;
using Uintra.Core.Extensions;

namespace Uintra.Search
{
    public class ElasticIndex : IElasticIndex
    {
        protected const int MinimumShouldMatches = 1;
        protected const int FieldWithReplaceAnalyzerBoost = 10;

        private readonly IElasticSearchRepository _elasticSearchRepository;
        private readonly IEnumerable<IElasticEntityMapper> _mappers;

        public ElasticIndex(IElasticSearchRepository elasticSearchRepository,
            IEnumerable<IElasticEntityMapper> mappers)
        {
            _elasticSearchRepository = elasticSearchRepository;
            _mappers = mappers;
        }

        public virtual SearchResult<SearchableBase> Search(SearchTextQuery query)
        {
            var searchRequest = GetSearchDescriptor()
                .Query(q =>
                    q.Bool(b => b
                        .Should(GetQueryContainers(query.Text))
                        .MinimumShouldMatch(MinimumShouldMatch.Fixed(MinimumShouldMatches))))
                .PostFilter(pf => pf.Bool(b => b.Must(GetSearchPostFilters(query))));

            ApplyAggregations(searchRequest, query);

            if (query.OrderingString.IsNullOrEmpty())
            {
                ApplySort(searchRequest);
            }
            else
            {
                ApplySort(searchRequest, query.OrderingDirection, query.OrderingString);
            }

            ApplyPaging(searchRequest, query);

            if (query.ApplyHighlights)
            {
                ApplyHighlight(searchRequest);
            }

            var queryResult = _elasticSearchRepository.SearchByIndex(searchRequest);
            var searchResult = ParseResults(queryResult);
            return searchResult;
        }


        protected virtual QueryContainer[] GetSearchPostFilters(SearchTextQuery query)
        {
            return new[]
            {
                GetSearchableTypeQueryContainers(query.SearchableTypeIds),
                GetOnlyPinnedQueryContainer(query.OnlyPinned)
            };
        }

        protected virtual SearchDescriptor<dynamic> GetSearchDescriptor()
        {
            return new SearchDescriptor<dynamic>()
                .TrackScores()
                .AllTypes();
        }

        public bool RecreateIndex(out string error)
        {
            _elasticSearchRepository.DeleteIndex();
            if (!_elasticSearchRepository.EnsureIndexExists(ElasticHelpers.SetAnalysis, out error)) return false;
            foreach (var mapper in _mappers)
                if (!mapper.CreateMap(out error)) return false;
            return true;
        }

        protected virtual QueryContainer GetBaseDescriptor(string query)
        {
            var desc = new QueryContainerDescriptor<SearchableBase>().Match(m => m
                .Query(query)
                .Analyzer(ElasticHelpers.Replace)
                .Field(f => f.Title));
            return desc;
        }

        protected virtual QueryContainer[] GetContentDescriptors(string query)
        {
            var desc = new List<QueryContainer>
            {
                new QueryContainerDescriptor<SearchableContent>().Match(m => m
                    .Query(query)
                    .Analyzer(ElasticHelpers.ReplaceNgram)
                    .Field(f => f.PanelContent)),
                new QueryContainerDescriptor<SearchableContent>().Match(m => m
                    .Query(query)
                    .Analyzer(ElasticHelpers.Replace)
                    .Field(f => f.PanelContent)
                    .Boost(FieldWithReplaceAnalyzerBoost)),
                new QueryContainerDescriptor<SearchableContent>().Match(m => m
                    .Query(query)
                    .Analyzer(ElasticHelpers.ReplaceNgram)
                    .Field(f => f.PanelTitle)),
                new QueryContainerDescriptor<SearchableContent>().Match(m => m
                    .Query(query)
                    .Analyzer(ElasticHelpers.Replace)
                    .Field(f => f.PanelTitle)
                    .Boost(FieldWithReplaceAnalyzerBoost))
            };

            return desc.ToArray();
        }

        protected virtual QueryContainer GetActivityDescriptor(string query)
        {
            var desc = new QueryContainerDescriptor<SearchableActivity>().Match(m => m
                .Query(query)
                .Analyzer(ElasticHelpers.Replace)
                .Field(f => f.Description));
            return desc;
        }

        protected virtual QueryContainer GetDocumentsDescriptor(string query)
        {
            var desc = new QueryContainerDescriptor<SearchableDocument>().Match(m => m
                 .Query(query)
                 .Analyzer(ElasticHelpers.Replace)
                 .Field(f => f.Attachment.Content));
            return desc;
        }

        protected virtual QueryContainer[] GetQueryContainers(string query)
        {
            var containers = new List<QueryContainer>();
            containers.AddRange(GetBaseDescriptor(query).ToEnumerable());
            containers.AddRange(GetContentDescriptors(query));
            containers.AddRange(GetActivityDescriptor(query).ToEnumerable());
            containers.AddRange(GetDocumentsDescriptor(query).ToEnumerable());
            return containers.ToArray();
        }

        protected virtual QueryContainer GetSearchableTypeQueryContainers(IEnumerable<int> searchableTypeIds)
        {
            return new QueryContainerDescriptor<SearchableBase>().Terms(t => t.Field(f => f.Type).Terms(searchableTypeIds));
        }

        protected virtual QueryContainer GetOnlyPinnedQueryContainer(bool onlyPinned)
        {
            var result = onlyPinned
                ? new QueryContainerDescriptor<SearchableActivity>().Terms(t => t.Field(f => f.IsPinActual).Terms(true))
                : new QueryContainerDescriptor<SearchableActivity>();
            return result;
        }

        protected virtual IEnumerable<BaseFacet> GlobalFacets(IReadOnlyDictionary<string, IAggregate> facets, string facetName)
        {
            var aggregations = new AggregationsHelper(facets);
            var globalAggregations = new AggregationsHelper(aggregations.Global(facetName).Aggregations);
            var globalFilter = globalAggregations.Filter(SearchConstants.SearchFacetNames.GlobalFilter);
            var items = globalFilter.Terms(facetName).Buckets.Select(bucket => new BaseFacet { Name = bucket.Key, Count = bucket.DocCount ?? default });
            return items;
        }

        protected virtual SearchResult<SearchableBase> ParseResults(ISearchResponse<dynamic> response)
        {
            var documents = ParseDocuments<SearchableBase>(response, true);

            var result = new SearchResult<SearchableBase>
            {
                TotalHits = response.Total,
                Documents = documents,
                TypeFacets = GlobalFacets(response.Aggs.Aggregations, SearchConstants.SearchFacetNames.Types)
            };

            return result;
        }

        protected virtual void ApplySort<T>(SearchDescriptor<T> searchDescriptor, int direction = 0, string propertyName = "_score") where T : class
        {
            if (propertyName.In("fullName", "mail"))
            {
                propertyName += $".{ElasticHelpers.Normalizer.Sort}";
            }

            switch (direction)
            {
                case 0:
                    searchDescriptor.Sort(s => propertyName.Equals("_score") ? s.Descending(propertyName) : s.Ascending(propertyName));
                    break;
                case 1:
                    searchDescriptor.Sort(s => propertyName.Equals("_score") ? s.Ascending(propertyName) : s.Descending(propertyName));
                    break;
            }
        }

        protected virtual void ApplyPaging<T>(SearchDescriptor<T> searchDescriptor, SearchTextQuery query) where T : class
        {
            searchDescriptor.Skip(query.Skip).Take(query.Take);
        }

        protected virtual void ApplyAggregations<T>(SearchDescriptor<T> searchDescriptor, SearchTextQuery query) where T : class
        {
            searchDescriptor.Aggregations(agg => agg
                .Global(SearchConstants.SearchFacetNames.Types, g => g
                    .Aggregations(a => a
                        .Filter(SearchConstants.SearchFacetNames.GlobalFilter, ss => ss
                            .Filter(fi => fi
                                .Bool(b => b
                                    .Should(GetQueryContainers(query.Text))
                                    .Must(GetSearchableTypeQueryContainers(query.SearchableTypeIds))
                                    .Must(GetOnlyPinnedQueryContainer(query.OnlyPinned))
                                ))
                            .Aggregations(
                                ia =>
                                    ia.Terms(SearchConstants.SearchFacetNames.Types,
                                        f => f.Field("type").Size(ElasticHelpers.MaxAggregationSize)))))));
        }

        protected virtual void ApplyHighlight<T>(SearchDescriptor<T> searchRequest) where T : class
        {
            searchRequest
                .Highlight(hd => hd
                    .Fields(ff => ff
                        .Field("*")
                        .PreTags(SearchConstants.Global.HighlightPreTag)
                        .PostTags(SearchConstants.Global.HighlightPostTag))
                );
        }

        protected virtual void HighlightResponse(ISearchResponse<dynamic> response)
        {
            var highlights = response.Hits.ToDictionary(x => x.Id, x => x.Highlights).ToList();

            foreach (var document in response.Documents)
            {
                var highlight = highlights.Find(el => el.Key == document.id.ToString());
                if (highlight.Key == null)
                {
                    continue;
                }
                Highlight(document, highlight.Value);
            }
        }

        protected virtual List<T> CollectDocuments<T>(ISearchResponse<dynamic> response)
            where T : SearchableBase
        {
            var documents = new List<T>();
            foreach (var document in response.Documents)
            {
                switch ((int)document.type)
                {
                    case (int)SearchableTypeEnum.Events:
                    case (int)SearchableTypeEnum.News:
                    case (int)SearchableTypeEnum.Bulletins:
                        documents.Add(SerializationExtensions.Deserialize<SearchableActivity>(document.ToString()));
                        break;
                    case (int)SearchableTypeEnum.Content:
                        documents.Add(SerializationExtensions.Deserialize<SearchableContent>(document.ToString()));
                        break;
                    case (int)SearchableTypeEnum.Document:
                        documents.Add(SerializationExtensions.Deserialize<SearchableDocument>(document.ToString()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Could not resolve type of searchable entity {(SearchableTypeEnum)document.type}");
                }
            }
            return documents;
        }

        protected virtual List<T> ParseDocuments<T>(ISearchResponse<dynamic> response, bool applyHighlight = false)
            where T : SearchableBase
        {
            if (applyHighlight)
            {
                HighlightResponse(response);
            }

            return CollectDocuments<T>(response);
        }

        protected virtual void Highlight(dynamic document, Dictionary<string, HighlightHit> fields)
        {
            var panelContent = new List<string>();

            foreach (var field in fields.Values)
            {
                var highlightedField = field.Highlights.FirstOrDefault();
                switch (field.Field)
                {
                    case "title":
                        document.title = highlightedField;
                        break;
                    case "description":
                        document.description = highlightedField;
                        break;

                    case "panelContent":
                        panelContent.AddRange(field.Highlights);
                        break;

                    case "panelTitle":
                        panelContent.AddRange(field.Highlights);
                        break;

                    case "attachment.content":
                        document.attachment.content = highlightedField;
                        break;
                }
            }

            HighlightAdditional(document, fields, panelContent);

            if (panelContent.Any())
            {
                document.panelContent = panelContent.ToDynamic();
            }
        }

        protected virtual void HighlightAdditional(dynamic document, Dictionary<string, HighlightHit> fields, List<string> panelContent)
        {
        }
    }
}