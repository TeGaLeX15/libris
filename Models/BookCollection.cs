// Models/Collection.cs
using System;
using System.Collections.Generic;

namespace Libris.Models;

public sealed class BookCollection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public List<Guid> BookIds { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}