﻿using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.Mobile.Service.DynamicsCrm.WebHost
{
    public static class AutoMapperEntityMapper
    {
        public static void InitializeDynamicsCrmCommonMaps()
        {
            Mapper.CreateMap<EntityReference, Guid>().ConvertUsing(er => er == null ? Guid.Empty : er.Id);
            Mapper.CreateMap<EntityReference, Guid?>().ConvertUsing(er => er == null ? (Guid?)null : er.Id);
            Mapper.CreateMap<EntityReference, string>().ConvertUsing(er => er == null ? null : er.LogicalName);
            Mapper.CreateMap<Guid?, EntityReference>().ConvertUsing(g => g == null ? null : new EntityReference { Id = g.Value });
            Mapper.CreateMap<Guid, EntityReference>().ConvertUsing(g => g == Guid.Empty ? null : new EntityReference { Id = g });
            Mapper.CreateMap<OptionSetValue, int?>().ConvertUsing(osv => osv == null ? (int?)null : osv.Value);
            Mapper.CreateMap<OptionSetValue, int>().ConvertUsing(osv => osv == null ? 0 : osv.Value);
            Mapper.CreateMap<int, OptionSetValue>().ConvertUsing(i => i == 0 ? null : new OptionSetValue(i));
            Mapper.CreateMap<int?, OptionSetValue>().ConvertUsing(i => i == null ? null : new OptionSetValue(i.Value));
        }
    }

    public class AutoMapperEntityMapper<TTableData, TEntity> : IEntityMapper<TTableData, TEntity>
        where TTableData : class, ITableData
        where TEntity : Entity
    {
        protected Dictionary<String, IMemberAccessor> PropertyMap { get; set; }

        public AutoMapperEntityMapper()
        {
            var map = Mapper.FindTypeMapFor<TTableData, TEntity>();
            if (map == null) throw new InvalidOperationException(String.Format("Could not find a map from {0} to {1}.", typeof(TTableData), typeof(TEntity)));

            this.PropertyMap = new Dictionary<string, IMemberAccessor>(StringComparer.OrdinalIgnoreCase);
            var propertyMaps = map.GetPropertyMaps();
            foreach (var m in propertyMaps)
            {
                if (m.SourceMember != null)
                {
                    this.PropertyMap.Add(m.SourceMember.Name, m.DestinationProperty);
                }
            }
        }

        public string GetAttributeName(string propertyName)
        {
            return this.PropertyMap[propertyName].Name.ToLowerInvariant();
        }


        public IEnumerable<string> GetAttributeNames()
        {
            return from p in PropertyMap.Values
                   select p.Name.ToLowerInvariant();
        }

        public TEntity Map(TTableData data)
        {
            return Mapper.Map<TTableData, TEntity>(data);
        }

        public TTableData Map(TEntity data)
        {
            return Mapper.Map<TEntity, TTableData>(data);
        }
    }
}