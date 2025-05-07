using Application.DTOs.Pagination;

namespace MuseTrip360.src.Application.DTOs.Artifact
{
    public class ArtifactQuery : PaginationReq
    {
        public string? SearchKeyword { get; set; }
    }
}
