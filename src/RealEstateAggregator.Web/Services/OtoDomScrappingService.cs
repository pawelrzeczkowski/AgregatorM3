﻿using System.Collections.Generic;
using System.Net.Http;
using RealEstateAggregator.Web.Models;
using HtmlAgilityPack;

namespace RealEstateAggregator.Web.Services
{
    public class OtoDomScrappingService : IScrappingService
    {
        private readonly HttpClient _httpClient;

        public OtoDomScrappingService(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async IAsyncEnumerable<ResultModel> GetData(SearchModel searchModel)
        {
            for (var page = 1; page < 7; page++)
            {
                var paging = string.Empty;
                if (page > 1) paging = $"&page={page}";
                var url =
                    $"https://www.otodom.pl/sprzedaz/mieszkanie/?search%5Bfilter_float_price%3Afrom%5D={searchModel.PriceFrom}&search%5Bfilter_float_price%3Ato%5D={searchModel.PriceTo}"
                    + $"&search%5Bfilter_float_price_per_m%3Ato%5D={searchModel.PricePerMeterTo}&search%5Bfilter_float_m%3Afrom%5D={searchModel.SurfaceFrom}&search%5Bfilter_float_m%3Ato%5D={searchModel.SurfaceTo}&search%5B"
                    + $"filter_enum_rooms_num%5D%5B0%5D={searchModel.RoomsFrom}&search%5Bfilter_enum_rooms_num%5D%5B1%5D={searchModel.RoomsTo}&search%5Bfilter_enum_market%5D%5B0%5D=secondary&search%5B"
                    + $"filter_enum_floor_no%5D%5B0%5D=floor_3&search%5Bfilter_enum_floor_no%5D%5B1%5D=floor_4&search%5Bfilter_enum_floor_no%5D%5B2%5D=floor_5"
                    + $"&search%5Bfilter_enum_floor_no%5D%5B3%5D=floor_6&search%5Bfilter_enum_floor_no%5D%5B4%5D=floor_7&search%5Bfilter_enum_floor_no%5D%5B5%5D"
                    + $"=floor_8&search%5Bfilter_enum_floor_no%5D%5B6%5D=floor_9&search%5Bfilter_enum_floor_no%5D%5B7%5D=floor_10&search%5Bfilter_enum_floor_no%5D%5B8%5D"
                    + $"=floor_higher_10&search%5Bdescription%5D=1&locations%5B0%5D%5Bregion_id%5D=7&locations%5B0%5D%5Bsubregion_id%5D=197&locations%5B0%5D%5Bcity_id%5D"
                    + $"=26&locations%5B0%5D%5Bdistrict_id%5D=39&locations%5B1%5D%5Bregion_id%5D=7&locations%5B1%5D%5Bsubregion_id%5D=197&locations%5B1%5D%5Bcity_id%5D=26"
                    + $"&locations%5B1%5D%5Bdistrict_id%5D=7365&nrAdsPerPage=72" + paging;

                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html");
                var response = await _httpClient.GetAsync(url);
                if (response.RequestMessage.RequestUri.ToString().Length < url.Length - 20) break;
                if (!response.IsSuccessStatusCode) break;
                var content = await response.Content.ReadAsStringAsync();

                var htmDocument = new HtmlDocument();
                htmDocument.LoadHtml(content);
                var nodes = htmDocument.DocumentNode.SelectNodes(
                    "//div[@id='listContainer']/main/section[@id='body-container']/div/div/div/div/article/div[@class='offer-item-details']/header/h3/a");

                if (nodes == null)
                {
                    yield return new ResultModel("Gratka", "incorrect html select node query: otodom");
                }

                foreach (var node in nodes)
                {
                    yield return new ResultModel("OtoDom", $"{node.GetAttributeValue("href", "incorrect htmlNode query: OtoDom")}");
                }
            }
        }
    }
}