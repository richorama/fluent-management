﻿/************************************************************************************************************
 * This software is distributed under a GNU Lesser License by Elastacloud Limited and it is free to         *
 * modify and distribute providing the terms of the license are followed. From the root of the source the   *
 * license can be found in /Resources/license.txt                                                           * 
 *                                                                                                          *
 * Web at: www.elastacloud.com                                                                              *
 * Email: info@elastacloud.com                                                                              *
 ************************************************************************************************************/
using System.Collections.Generic;

namespace Elastacloud.AzureManagement.Fluent.Types.Websites
{
    /// <summary>
    /// The size of the instances used by the web farm 
    /// </summary>
    public enum InstanceSize
    {
        /// <summary>
        /// A small web instance
        /// </summary>
        Small,
        /// <summary>
        /// A medium web instance
        /// </summary>
        Medium,
        /// <summary>
        /// A large web instance
        /// </summary>
        Large
    }
}