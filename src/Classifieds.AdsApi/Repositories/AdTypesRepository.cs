using System.Collections.Generic;
using Shared.Models;
using Shared.Enums;

namespace AdsApi.Repositories
{
    public class AdTypesRepository
    {
        private static List<AdType> adTypes = new List<AdType> {
            new AdType {
                Id = AdTypes.General,
                Name = "general",
                Attributes = new List<AdTypeAttribute>(),
                Filters = new List<AdTypeAttribute>()
            },
            new AdType {
                Id = AdTypes.RealEstate,
                Name = "realestate",
                Attributes = new List<AdTypeAttribute> {
                    new AdTypeAttribute
                    {
                        Name = "price",
                        Type = AttributeTypes.Number,
                        Label = "Asking Price",
                        Required = true,
                        Widget = "text",
                        Attributes = new List<AdTypeAttribute>()
                    },
                    new AdTypeAttribute
                    {
                        Name = "beds",
                        Type = AttributeTypes.Number,
                        Label = "Beds",
                        Required = true,
                        Widget = "text",
                        Attributes = new List<AdTypeAttribute>()
                    },
                    new AdTypeAttribute
                    {
                        Name = "baths",
                        Type = AttributeTypes.Number,
                        Label = "Baths",
                        Required = true,
                        Widget = "text",
                        Attributes = new List<AdTypeAttribute>()
                    },
                    new AdTypeAttribute
                    {
                        Name = "sqft",
                        Type = AttributeTypes.Number,
                        Label = "Sqft",
                        Required = true,
                        Widget = "text",
                        Attributes = new List<AdTypeAttribute>()
                    },
                },
                Filters = new List<AdTypeAttribute>()
            },
            new AdType {
                Id = AdTypes.Rental,
                Name = "rentals",
                Attributes = new List<AdTypeAttribute>(),
                Filters = new List<AdTypeAttribute>()
            },
            new AdType {
                Id = AdTypes.Auto,
                Name = "auto",
                Attributes = new List<AdTypeAttribute>(),
                Filters = new List<AdTypeAttribute>()
            }
        };

        public AdTypesRepository()
        {
        }

        public List<AdType> Get()
        {
            return AdTypesRepository.adTypes;
        }

        public AdType Get(AdTypes id)
        {
            return AdTypesRepository.adTypes.Find(t => t.Id == id);
        }

    }
}
