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
            // The code that defines your stack goes here

            var vpc = new Vpc(this, "api-vpc", new VpcProps
            {
                MaxAzs = 3 // Default is all AZs in region
            });

            //https://garbe.io/blog/2019/09/20/hey-cdk-how-to-use-existing-resources/
            // const vpc = Vpc.fromLookup(this, 'MyExistingVPC', { isDefault: true });
            // new cdk.CfnOutput(this, "MyVpc", {value: vpc.vpcId });

            var cluster = new Cluster(this, "api-ecs-cluster", new ClusterProps
            {
                Vpc = vpc,
                // ClusterName = "cfroehlich-testcluster"
            });

            var task = new FargateTaskDefinition(this, "api-fargate-task", new FargateTaskDefinitionProps()
            {
                Cpu = 256,
                MemoryLimitMiB = 512
            });
            // task.NetworkMode = NetworkMode.BRIDGE;
            // Network mode cannot be changed "Fargate tasks require the awsvpc network mode."
            // https://docs.aws.amazon.com/cdk/api/latest/docs/@aws-cdk_aws-ecs.FargateTaskDefinition.html#properties
            // https://aws.amazon.com/blogs/compute/task-networking-in-aws-fargate/
            // Containers can communicate on localhost

            var logGroup = new LogGroup(this, "loggroup-containers", new LogGroupProps()
            {
                Retention = RetentionDays.ONE_MONTH
            });
            
            var nginxContainer = new ContainerDefinition(this, "container-nginx", new ContainerDefinitionProps()
            {
                TaskDefinition = task,
                Image = ContainerImage.FromAsset("../Api/Nginx", new AssetImageProps()
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
                    { "API_HOST", "127.0.0.1" }
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
                Image = ContainerImage.FromAsset("../Api/Api", new AssetImageProps()
                {
                    File = "Api.Dockerfile"
                }),
                PortMappings = new[]
                {
                    new PortMapping() { HostPort = 5000, ContainerPort = 5000, Protocol = Protocol.TCP }
                },
                Essential = true,
                Logging = new AwsLogDriver(new AwsLogDriverProps()
                {
                    LogGroup = logGroup,
                    StreamPrefix = "api"
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