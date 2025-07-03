using System;
using System.Collections.Generic;

namespace Core_DataAccessPlugIns.Models;

public partial class OrderDetailsDynamic
{
    public int OrderId { get; set; }

    public string? CustomerName { get; set; }

    public string EmployeeName { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public string ShipperName { get; set; } = null!;

    public decimal? Freight { get; set; }

    public string? ShipName { get; set; }

    public string? ShipAddress { get; set; }

    public string? City { get; set; }

    public string? ShipPostalCode { get; set; }

    public string? Country { get; set; }
}
