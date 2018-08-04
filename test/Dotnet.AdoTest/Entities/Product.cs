﻿using Dotnet.Ado;
using Dotnet.Data;

namespace Dotnet.AdoTest
{
    public class Product : IEntity<int>
    {
        [Key(IsAutoGenerated = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? TenantId { get; set; }
    }




}