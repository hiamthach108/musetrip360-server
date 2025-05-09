using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;
using Microsoft.EntityFrameworkCore;

namespace MuseTrip360.src.Application.DTOs.Artifact
{
    public class ArtifactQuery : PaginationReq
    {
        [MaxLength(100)]
        public string? SearchKeyword { get; set; }
        // // rating is between 0 and 5
        // [Range(0, 5)]
        // public float? Rating { get; set; }
    }
}