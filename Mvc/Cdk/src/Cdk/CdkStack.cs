using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.Logs;
using Protocol = Amazon.CDK.AWS.ECS.Protocol;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var vpc = new Vpc(this, "api-vpc", new VpcProps
            {
                MaxAzs = 3 // Default is all AZs in region
            });
            
            var cluster = new Cluster(this, "api-ecs-cluster", new ClusterProps
            {
                Vpc = vpc,
            });

            var task = new FargateTaskDefinition(this, "api-fargate-task", new FargateTaskDefinitionProps()
            {
                Cpu = 256,
                MemoryLimitMiB = 512
            });

            var logGroup = new LogGroup(this, "loggroup-containers", new LogGroupProps()
            {
                Retention = RetentionDays.ONE_MONTH
            });
            
            var nginxContainer = new ContainerDefinition(this, "container-nginx", new ContainerDefinitionProps()
            {
                TaskDefinition = task,
                Image = ContainerImage.FromAsset("../Web/Nginx", new AssetImageProps()
                {
                    File = "Nginx.Dockerfile"
                }),
                PortMappings = new[]
                {
                    new PortMapping() { HostPort = 80, ContainerPort = 80, Protocol = Protocol.TCP }
                },
                Essential = true,
                Environment = new Dictionary<string, string>()
                {
                    { "WEB_HOST", "127.0.0.1" }
                },
                Logging = new AwsLogDriver(new AwsLogDriverProps()
                {
                    LogGroup = logGroup,
                    StreamPrefix = "nginx"
                })
            });

            var apiContainer = new ContainerDefinition(this, "container-api", new ContainerDefinitionProps()
            {
                TaskDefinition = task,
                Image = ContainerImage.FromAsset("../Web/Mvc", new AssetImageProps()
                {
                    File = "Mvc.Dockerfile"
                }),
                PortMappings = new[]
                {
                    new PortMapping() { HostPort = 5000, ContainerPort = 5000, Protocol = Protocol.TCP }
                },
                Essential = true,
                Logging = new AwsLogDriver(new AwsLogDriverProps()
                {
                    LogGroup = logGroup,
                    StreamPrefix = "web"
                })
            });

            // Create a load-balanced Fargate service and make it public
            new ApplicationLoadBalancedFargateService(this, "api-fargate-loadbalancer",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = cluster, // Required
                    // DesiredCount = 6,           // Default is 1
                    // TaskDefinition or TaskImageOptions must be specified, but not both.
                    TaskDefinition = task,
                    // MemoryLimitMiB = 2048,      // Default is 256
                    PublicLoadBalancer = true // Default is false
                }
            );
        }
    }
}