//------------------------------------------------------------------------------
// <copyright file="WebDataService.svc.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using Simple.Data.OData.NorthwindModel;

namespace Simple.OData.NorthwindService
{
    public class Northwind : DataService<NorthwindEntities>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            Simple.Data.OData.NorthwindModel.NorthwindService.InitializeService(config);
        }
    }
}
