using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devices.Api.Models
{
    public enum DeviceState { Available, InUse, Inactive }

    public class Device
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public DeviceState State { get; set; } = DeviceState.Available;

        [Required]
        public DateTimeOffset CreationTime { get; set; }
    }
}