using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers {
    public static class Extension {
        public static void AddApplicationError (this HttpResponse response, string Message) {
            response.Headers.Add ("Application-Error", Message);
            response.Headers.Add ("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add ("Access-Control-Allow-Origin", "*");
        }

        public static int CalculateAge (this DateTime dateOfBirth) {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.AddYears (age) > DateTime.Today) {
                age--;
            }
            return age;
        }
        public static void AddPagination (this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages) {
            var paginationHeader = new PaginationHeader (currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings ();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver ();
            response.Headers.Add ("Pagination", JsonConvert.SerializeObject (paginationHeader, camelCaseFormatter));
            response.Headers.Add ("Access-Control-Expose-Headers", "Pagination");
        }
    }
}