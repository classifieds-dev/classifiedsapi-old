using System;
using System.Collections.Generic;
using Nest;
using Shared.Models;
using Shared.Enums;
using System.Linq;

namespace AdsApi.Helpers
{
    public class AdAttributesHelper
    {
        public QueryContainer applyAttributes(List<AdTypeAttribute> attributes, QueryContainerDescriptor<AdIndex> d, IDictionary<string, IDictionary<int, string>> attributeValues)
        {
            var leafNodes = extractLeafAttributes(attributes);
            QueryContainer query = null;
            foreach (var leafNode in leafNodes)
            {
                if (attributeValues.ContainsKey(leafNode.Name))
                {
                    query &= d.Nested(n => n
                        .Path(p => p.Attributes)
                        .Query(nq =>
                        {
                            QueryContainer andQuery = null;
                            IDictionary<int, string> attrValue;
                            string attrValue0;
                            attributeValues.TryGetValue(leafNode.Name, out attrValue);
                            attrValue.TryGetValue(0, out attrValue0);
                            andQuery &= nq.Term(p => p.Attributes.First().Name, leafNode.Name);
                            if(leafNode.Type == AttributeTypes.Number)
                            {
                                if(attrValue.Count == 1)
                                {
                                    var intValue = Int32.Parse(attrValue0);
                                    andQuery &= nq.Term(p => p.Attributes.First().intValue, intValue);
                                } else
                                {
                                    var intValues = attrValue.Select(v => Int32.Parse(v.Value)).ToArray();
                                    andQuery &= nq.Terms(t => t
                                        .Field(p => p.Attributes.First().intValue)
                                        .Terms(intValues)
                                    );
                                }
                            } else
                            {
                                if (attrValue.Count == 1)
                                {
                                    andQuery &= nq.Term(p => p.Attributes.First().stringValue, attrValue0);
                                } else
                                {
                                    var stringValues = attrValue.Select(v => v.Value).ToArray();
                                    andQuery &= nq.Terms(t => t
                                        .Field(p => p.Attributes.First().stringValue)
                                        .Terms(stringValues)
                                    );
                                }
                            }
                            return andQuery;
                        })
                    );
                }
            }
            return query;
        }

        public List<AdTypeAttribute> extractLeafAttributes(List<AdTypeAttribute> attributes)
        {
            List<AdTypeAttribute> leafNodes = new List<AdTypeAttribute>();
            foreach (var attribute in attributes)
            {
                if (attribute.Attributes == null || attribute.Attributes.Count == 0)
                {
                    leafNodes.Add(attribute);
                }
                else
                {
                    leafNodes.AddRange(extractLeafAttributes(attribute.Attributes));
                }
            }
            return leafNodes;
        }
    }
}
