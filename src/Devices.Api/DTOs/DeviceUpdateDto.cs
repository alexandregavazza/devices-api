using System.ComponentModel.DataAnnotations;
using Devices.Api.Models;

namespace Devices.Api.DTOs
{
    public class DeviceUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public DeviceState State { get; set; }
    }
}