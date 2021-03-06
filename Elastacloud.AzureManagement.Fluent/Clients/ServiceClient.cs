﻿/************************************************************************************************************
 * This software is distributed under a GNU Lesser License by Elastacloud Limited and it is free to         *
 * modify and distribute providing the terms of the license are followed. From the root of the source the   *
 * license can be found in /Resources/license.txt                                                           * 
 *                                                                                                          *
 * Web at: www.elastacloud.com                                                                              *
 * Email: info@elastacloud.com                                                                              *
 ************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Elastacloud.AzureManagement.Fluent.Clients.Interfaces;
using Elastacloud.AzureManagement.Fluent.Commands.Certificates;
using Elastacloud.AzureManagement.Fluent.Commands.Services;
using Elastacloud.AzureManagement.Fluent.Helpers;
using Elastacloud.AzureManagement.Fluent.Helpers.PublishSettings;
using Elastacloud.AzureManagement.Fluent.Services.Classes;
using Elastacloud.AzureManagement.Fluent.Types;

namespace Elastacloud.AzureManagement.Fluent.Clients
{
    public class ServiceClient : IServiceClient 
    {
        /// <summary>
        /// Used to construct the ServiceClient
        /// </summary>
        public ServiceClient(string subscriptionId, X509Certificate2 certificate, string cloudService, DeploymentSlot slot = DeploymentSlot.Production)
        {
            SubscriptionId = subscriptionId;
            ManagementCertificate = certificate;
            Name = cloudService;
            Slot = slot;
        }

        /// <summary>
        /// gets or sets the deployment slot for the cloud service
        /// </summary>
        public DeploymentSlot Slot { get; set; }

        /// <summary>
        /// Starts all of the roles within a cloud service
        /// </summary>
        public void Start()
        {
            var command = new UpdateRoleStatusCommand(Name, Slot, UpdateDeploymentStatus.Running)
                {
                    SubscriptionId = SubscriptionId,
                    Certificate = ManagementCertificate
                };
            command.Execute();
        }

        /// <summary>
        /// Stops all of the roles within a cloud service
        /// </summary>
        public void Stop()
        {
            var command = new UpdateRoleStatusCommand(Name, Slot, UpdateDeploymentStatus.Suspended)
            {
                SubscriptionId = SubscriptionId,
                Certificate = ManagementCertificate
            };
            command.Execute();
        }

        /// <summary>
        /// The subscription being used
        /// </summary>
        public string SubscriptionId { get; set; }
        /// <summary>
        /// The management certificate previously uploaded to the portal
        /// </summary>
        public X509Certificate2 ManagementCertificate { get; set; }
        /// <summary>
        /// The name of the cloud service
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Adds a services certificate to the cloud service
        /// </summary>
        /// <param name="thumbprint">The thumbprint of the certificate to get from the store</param>
        /// <param name="password">The password used to export the key</param>
        public void UploadServiceCertificate(string thumbprint, string password)
        {
            X509Certificate2 certificate = null;
            try
            {
                certificate = PublishSettingsExtractor.FromStore(thumbprint, StoreLocation.LocalMachine);
            }
            catch (Exception)
            {
                certificate = certificate ?? PublishSettingsExtractor.FromStore(thumbprint);
                if(certificate == null)
                    throw new ApplicationException("unable to find certificate with thumbprint " + thumbprint + " in local store");
            }
            UploadServiceCertificate(certificate, password);            
        }
        /// <summary>
        /// Uploads a certificate given a valid service certificate and password
        /// </summary>
        /// <param name="certificate">The certificate being uploaded</param>
        /// <param name="password">The .pfx password for the certificate</param>
        /// <param name="includePrivateKey">The .pfx password for the certificate</param>
        public void UploadServiceCertificate(X509Certificate2 certificate, string password = "", bool includePrivateKey = false)
        {
            var certBytes = includePrivateKey
                                ? certificate.Export(X509ContentType.Pkcs12, password)
                                : certificate.Export(X509ContentType.Cert);
            
            var cert = new AddServiceCertificateCommand(certBytes, password, Name)
            {
                SubscriptionId = SubscriptionId,
                Certificate = ManagementCertificate
            };
            cert.Execute();
        }

        /// <summary>
        /// Creates a service certificate
        /// </summary>
        /// <param name="name">The name (CN) of the certificate</param>
        /// <param name="password">The password of the certificate</param>
        /// <param name="exportDirectory">Where the .pem, .cer and pfx will be put</param>
        public X509Certificate2 CreateServiceCertificate(string name, string password, string exportDirectory)
        {
            return CertificateGenerator.Create(name, DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)),
                                        DateTime.UtcNow.AddYears(2), password, true, exportDirectory);
        }

        /// <summary>
        /// Creates a new cloud service 
        /// </summary>
        /// <param name="location">the data centre location of the cloud service</param>
        /// <param name="description">The description of the cloud service</param>
        public void CreateNewCloudService(string location, string description = "Fluent Management created cloud service")
        {
            var hostedServiceCreate = new CreateCloudServiceCommand(Name, description, location)
            {
                Certificate = ManagementCertificate,
                SubscriptionId = SubscriptionId
            };
            hostedServiceCreate.Execute();
        }

        /// <summary>
        /// Used to delete the current cloud service
        /// </summary>
        public void DeleteCloudService()
        {
            // delete the hosted service
            var deleteService = new DeleteHostedServiceCommand(Name)
            {
                Certificate = ManagementCertificate,
                SubscriptionId = SubscriptionId
            };
            deleteService.Execute();
        }

        /// <summary>
        /// Used to delete a deployment in a respective slot 
        /// </summary>
        /// <param name="slot">Either production or staging</param>
        public void DeleteDeployment(DeploymentSlot slot = DeploymentSlot.Production)
        {
            var deleteDeployment = new DeleteDeploymentCommand(Name, slot)
            {
                Certificate = ManagementCertificate,
                SubscriptionId = SubscriptionId
            };
            deleteDeployment.Execute();
        }

        /// <summary>
        /// Updates a role instance count within a cloud services
        /// </summary>
        public void UpdateRoleInstanceCount(string roleName, int instanceCount)
        {
            var config = new GetDeploymenConfigurationCommand(Name)
                {
                    SubscriptionId = SubscriptionId,
                    Certificate = ManagementCertificate
                };
            config.Execute();
            config.Configuration.SetInstanceCountForRole(roleName, instanceCount);
            var update = new SetDeploymenConfigurationCommand(Name, config.Configuration)
                {
                    SubscriptionId = SubscriptionId,
                    Certificate = ManagementCertificate
                };
            update.Execute();
        }

        /// <summary>
        /// Creates a service certificate and adds to the remote config 
        /// </summary>
        public ServiceCertificate CreateServiceCertificateAndAddRemoteDesktop(string username, string password, ref CscfgFile file)
        {
            var certificate = new ServiceCertificate(username, password);
            certificate.Create();

            var desktop = new RemoteDesktop(certificate)
                              {
                                  Username = username,
                                  Password = password
                              };
            file.NewVersion = ((ICloudConfig) desktop).ChangeConfig(file.NewVersion);
            return certificate;
        }

        /// <summary>
        /// Returns a list of roles for a given cloud service
        /// </summary>
        public List<string> Roles
        {
            get
            {
                var command = new GetDeploymenRoleNamesCommand(Name, Slot)
                    {
                        SubscriptionId = SubscriptionId,
                        Certificate = ManagementCertificate
                    };
                command.Execute();
                return command.RoleNames;
            }
        }
    }
}
