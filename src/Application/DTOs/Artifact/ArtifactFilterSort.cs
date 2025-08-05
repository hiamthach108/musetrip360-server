using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;
using System.Reflection;

namespace MuseTrip360.src.Application.DTOs.Artifact
{
    public class ArtifactFilterSort : PaginationReq
    {
        [Range(0, 5)]
        public int? Rating { get; set; }
        //public string? HistoricalPeriod { get; set; }
        public bool? IsActive { get; set; }
        public Guid? MuseumId { get; set; }
        public bool? IsDescending { get; set; }
        public static List<string> GetNonNullFields(ArtifactFilterSort filterSort)
        {
            var nonNullFields = new List<string>();
            var properties = typeof(ArtifactFilterSort).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.Name == "IsDescending" || prop.Name == "Page" || prop.Name == "PageSize")
                    continue;

                var value = prop.GetValue(filterSort);
                if (value != null)
                {
                    nonNullFields.Add(prop.Name);
                }
            }
            return nonNullFields;
        }
    }

}