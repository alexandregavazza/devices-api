using System;
using Devices.Api.Models;

namespace Devices.Api.DTOs
{
    public class DeviceReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public DeviceState State { get; set; }
        public DateTimeOffset CreationTime { get; set; }
    }
}